using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdleBusiness.Data;
using IdleBusiness.Extensions;
using IdleBusiness.Helpers;
using IdleBusiness.Models;
using IdleBusiness.Views.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IdleBusiness.Controllers
{
    public class BusinessController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BusinessHelper _businessHelper;
        private readonly ILogger<BusinessController> _logger;

        public BusinessController(ApplicationDbContext context, ILogger<BusinessController> logger)
        {
            _context = context;
            _logger = logger;
            _businessHelper = new BusinessHelper(context, _logger);
        }

        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            var vm = new BusinessIndexVM();
            var business = await _businessHelper.UpdateGainsSinceLastCheckIn(id);
            vm.Business = business;
            vm.CurrentEntrepreneur = await GetCurrentEntrepreneur();
            vm.CurrentBusinessInvestments = await _businessHelper.GetInvestmentsCompanyHasMade(id);
            vm.HasCurrentEntrepreneurInvestedInBusiness = business.Investments.Any(s => s.InvestingBusinessId == vm.CurrentEntrepreneur.Business.Id && s.InvestmentType != InvestmentType.Espionage);
            if (vm.HasCurrentEntrepreneurInvestedInBusiness)
            {
                var investments = business.Investments
                    .Where(s => s.InvestingBusinessId == vm.CurrentEntrepreneur.Business.Id)
                    .Where(s => s.InvestmentType == InvestmentType.Investment)
                    .ToList();
                vm.CurrentEntrepreneurInvestments = investments;
                foreach (var item in investments)
                {
                    vm.TotalInvestedAmount += item.InvestmentAmount;
                    vm.InvestedProfits += InvestmentHelper.CalculateInvestmentProfit(item);
                }
            }
            if (vm.Business == null) return RedirectToAction("Index", "Home");

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> BusinessDirectory()
        {
            var businesses = await _context.Business
                .Where(s => !string.IsNullOrEmpty(s.Name))
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(businesses);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> InvestInCompany(int companyToInvestInId, float investmentAmount)
        {
            var user = await GetCurrentEntrepreneur();
            if (user.Business.LifeTimeEarnings < 1000000) return RedirectToAction("Index", "Business", new { id = companyToInvestInId });
            if (investmentAmount <= 0 || investmentAmount > user.Business.CashPerSecond) return RedirectToAction("Index", "Business", new { id = companyToInvestInId });
            var companyToInvestIn = await _context.Business.SingleOrDefaultAsync(s => s.Id == companyToInvestInId);

            companyToInvestIn.Investments.Add(
                new Investment() 
                { 
                    BusinessToInvestId = companyToInvestIn.Id, 
                    InvestmentAmount = investmentAmount,
                    InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                    InvestedBusinessCashAtInvestment = companyToInvestIn.Cash,
                    InvestedBusinessCashPerSecondAtInvestment = companyToInvestIn.CashPerSecond,
                    InvestmentType = InvestmentType.Investment,
                    InvestingBusinessId = user.Business.Id,
                });

            user.Business.CashPerSecond -= investmentAmount;
            companyToInvestIn.CashPerSecond += investmentAmount;

            _context.Business.Update(user.Business);
            _context.Business.Update(companyToInvestIn);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Business", new { id = companyToInvestInId });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FindAvailableGroupInvestors([FromQuery]string companyName)
        {
            if (string.IsNullOrEmpty(companyName)) return Json("");

            var availableBusinesses = _context.Business
                .Include(s => s.BusinessPurchases)
                .Where(s => s.Name.ToLower().Contains(companyName.ToLower()))
                .ToList()
                .Where(s => PurchasableHelper.HasBusinessPurchasedItem(s.BusinessPurchases, 33));

            return Ok(JsonConvert.SerializeObject(availableBusinesses, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GroupInvestInCompany(int companyToPartnerWith, int businessSendingRequest, int companyToInvestInId, double investmentAmount)
        {
            var user = await GetCurrentEntrepreneur();
            var companyToInvestIn = await _context.Business.SingleOrDefaultAsync(s => s.Id == companyToInvestInId);
            var partnerCompany = await _context.Business.Where(s => s.Id == companyToPartnerWith).SingleOrDefaultAsync();

            if (investmentAmount <= 0 || investmentAmount > user.Business.CashPerSecond || investmentAmount > partnerCompany.CashPerSecond) 
                return RedirectToAction("Index", "Business", new { id = companyToInvestInId });

            var investment = new Investment()
            {
                BusinessToInvestId = companyToInvestIn.Id,
                InvestmentAmount = investmentAmount,
                InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                InvestedBusinessCashAtInvestment = companyToInvestIn.Cash,
                InvestedBusinessCashPerSecondAtInvestment = companyToInvestIn.CashPerSecond,
                InvestmentType = InvestmentType.Group,
                InvestingBusinessId = user.Business.Id,
                PartnerBusinessId = companyToPartnerWith,
            };
            companyToInvestIn.Investments.Add(investment);
            await _context.SaveChangesAsync();

            var bsr = await _context.Business.Where(s => s.Id == businessSendingRequest).SingleOrDefaultAsync();
            var investmentLink = @$"<a href='/Business/GroupInvestment/{investment.Id}'>Click here to accept</a>";
            var message = $"{bsr.Name} wants to invest ${investmentAmount.ToKMB()} in {companyToInvestIn.Name} with you. {investmentLink}";
            partnerCompany.ReceivedMessages.Add(
                new Message() { DateReceived = DateTime.UtcNow, SendingBusinessId = businessSendingRequest, ReceivingBusinessId = companyToPartnerWith, MessageBody = message });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Business", new { id = companyToInvestInId });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GroupInvestment(int id)
        {
            var groupInvestment = await _context.Investments
                .Include(s => s.BusinessToInvest)
                .Include(s => s.InvestingBusiness)
                .Include(s => s.PartnerBusiness)
                .SingleOrDefaultAsync(s => s.Id == id);

            var investingBusiness = groupInvestment.InvestingBusiness;
            var partnerBusiness = groupInvestment.PartnerBusiness;
            var businessToInvestIn = groupInvestment.BusinessToInvest;

            if (groupInvestment.PartnerBusinessId != partnerBusiness.Id) return RedirectToAction("Index", "Home");

            partnerBusiness.CashPerSecond -= groupInvestment.InvestmentAmount;
            investingBusiness.CashPerSecond -= groupInvestment.InvestmentAmount;
            businessToInvestIn.CashPerSecond += groupInvestment.InvestmentAmount * 2;

            _context.Business.Update(partnerBusiness);
            _context.Business.Update(investingBusiness);
            _context.Business.Update(businessToInvestIn);
            await _context.SaveChangesAsync();

            var investment = new Investment()
            {
                BusinessToInvestId = businessToInvestIn.Id,
                InvestmentAmount = groupInvestment.InvestmentAmount,
                InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                InvestedBusinessCashAtInvestment = groupInvestment.BusinessToInvest.Cash,
                InvestedBusinessCashPerSecondAtInvestment = groupInvestment.BusinessToInvest.CashPerSecond,
                InvestmentType = InvestmentType.Investment,
                InvestingBusinessId = partnerBusiness.Id,
            };
            _context.Investments.Add(investment);

            _context.Investments.Add(new Investment()
            {
                BusinessToInvestId = businessToInvestIn.Id,
                InvestmentAmount = groupInvestment.InvestmentAmount,
                InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                InvestedBusinessCashAtInvestment = groupInvestment.BusinessToInvest.Cash,
                InvestedBusinessCashPerSecondAtInvestment = groupInvestment.BusinessToInvest.CashPerSecond,
                InvestmentType = InvestmentType.Investment,
                InvestingBusinessId = investingBusiness.Id,
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CommitEspionage(int companyToEspionageId)
        {
            var user = await GetCurrentEntrepreneur();
            var companyToEspionage = await _context.Business.SingleOrDefaultAsync(s => s.Id == companyToEspionageId);
            var costOfEspionage = 10000;
            if (companyToEspionage.AmountEmployed < 70) return RedirectToAction("Index", "Business", new { id = companyToEspionageId });
            if (user.Business.Cash < costOfEspionage) return RedirectToAction("Index", "Business", new { id = companyToEspionageId });

            user.Business.Cash -= costOfEspionage;
            _context.Business.Update(user.Business);
            await _context.SaveChangesAsync();

            var rand = new Random();
            if (((user.Business.EspionageChance * 100) - (companyToEspionage.EspionageDefense * 100)) < rand.Next(0, 100)) return RedirectToAction("Index", "Business", new { id = companyToEspionageId });

            companyToEspionage.Investments.Add(
                new Investment() 
                { 
                    BusinessToInvestId = companyToEspionage.Id,
                    InvestingBusinessId = user.Business.Id,
                    InvestmentAmount = (companyToEspionage.CashPerSecond / 2), 
                    InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                    InvestmentType = InvestmentType.Espionage,
                });
            companyToEspionage.CashPerSecond -= (companyToEspionage.CashPerSecond / 2);
            companyToEspionage.EspionageDefense += .05F;

            _context.Business.Update(companyToEspionage);
            _context.Business.Update(user.Business);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Business", new { id = companyToEspionageId });
        }

        [NonAction]
        private async Task<Entrepreneur> GetCurrentEntrepreneur()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ent = await _context.Entrepreneurs
                .Include(s => s.Business)
                    .ThenInclude(s => s.Investments)
                .Include(s => s.Business.BusinessPurchases)
                .FirstOrDefaultAsync(s => s.Id == userId);

            if (ent == null) return null;
            return ent;
        }
    }
}
