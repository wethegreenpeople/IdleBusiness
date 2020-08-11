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
using Microsoft.AspNetCore.Mvc.ApplicationParts;
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
        private readonly EntrepreneurHelper _entrepreneurHelper;
        private readonly ApplicationHelper _applicationHelper;
        private readonly InvestmentHelper _investmentHelper;

        public BusinessController(ApplicationDbContext context, ILogger<BusinessController> logger)
        {
            _context = context;
            _logger = logger;
            _businessHelper = new BusinessHelper(context, _logger);
            _entrepreneurHelper = new EntrepreneurHelper(context, _logger);
            _applicationHelper = new ApplicationHelper(_logger);
            _investmentHelper = new InvestmentHelper(_context, _logger);
        }

        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            var vm = new BusinessIndexVM();
            var business = await _businessHelper.UpdateGainsSinceLastCheckIn(id);
            business.Owner = await _entrepreneurHelper.UpdateEntrepreneurScore(id);
            vm.Business = business;
            vm.CurrentEntrepreneur = await GetCurrentEntrepreneur();
            vm.CurrentBusinessInvestments = await _businessHelper.GetInvestmentsCompanyHasMade(id);

            var currentEntrepreneursInvestmentsInBusiness = await _investmentHelper.GetInvestmentsBusinessHasMadeInAnotherBusinessAsync(vm.CurrentEntrepreneur.BusinessId, business.Id);
            vm.HasCurrentEntrepreneurInvestedInBusiness = currentEntrepreneursInvestmentsInBusiness?.Count > 0;
            if (vm.HasCurrentEntrepreneurInvestedInBusiness)
            {
                vm.CurrentEntrepreneurInvestments = currentEntrepreneursInvestmentsInBusiness.ToList();
                foreach (var item in vm.CurrentEntrepreneurInvestments)
                {
                    vm.TotalInvestedAmount += item.Investment.InvestmentAmount;
                    vm.InvestedProfits += InvestmentHelper.CalculateInvestmentProfit(item);
                }
            }
            vm.CurrentBusinessEspionages = await _businessHelper.GetEspionagesCompanyHasComitted(id);
            vm.EspionagePercentage = CalculateEspionagePercentage(business, vm.CurrentEntrepreneur.Business);
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
            if (companyToInvestInId == user.Business.Id) return RedirectToAction("Index", "Business", new { id = companyToInvestInId }); // cannot invest in yourself
            if (user.Business.LifeTimeEarnings < 1000000) return RedirectToAction("Index", "Business", new { id = companyToInvestInId });
            if (investmentAmount <= 0 || investmentAmount > user.Business.CashPerSecond) return RedirectToAction("Index", "Business", new { id = companyToInvestInId });
            var companyToInvestIn = await _context.Business.SingleOrDefaultAsync(s => s.Id == companyToInvestInId);

            var investment = new Investment()
            {
                InvestmentAmount = investmentAmount,
                InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                InvestedBusinessCashAtInvestment = companyToInvestIn.Cash,
                InvestedBusinessCashPerSecondAtInvestment = companyToInvestIn.CashPerSecond,
                InvestmentType = InvestmentType.Investment,
            };
            var investorBusinessInvestment = new BusinessInvestment()
            {
                Investment = investment,
                InvestmentDirection = InvestmentDirection.Investor,
                InvestmentType = InvestmentType.Investment
            };
            var investeeBusinessInvestment = new BusinessInvestment()
            {
                Investment = investment,
                InvestmentDirection = InvestmentDirection.Investee,
                InvestmentType = InvestmentType.Investment
            };

            user.Business.BusinessInvestments.Add(investorBusinessInvestment);
            companyToInvestIn.BusinessInvestments.Add(investeeBusinessInvestment);

            user.Business.CashPerSecond -= investmentAmount;
            companyToInvestIn.CashPerSecond += investmentAmount;

            _context.Business.Update(user.Business);
            _context.Business.Update(companyToInvestIn);
            await _applicationHelper.TrySaveChangesConcurrentAsync(_context);

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
            var partnerCompany = await _context.Business.SingleOrDefaultAsync(s => s.Id == companyToPartnerWith);

            if (investmentAmount <= 0 || investmentAmount > user.Business.CashPerSecond || investmentAmount > partnerCompany.CashPerSecond) 
                return RedirectToAction("Index", "Business", new { id = companyToInvestInId });

            var investment = new Investment()
            {
                InvestmentAmount = investmentAmount,
                InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                InvestedBusinessCashAtInvestment = companyToInvestIn.Cash,
                InvestedBusinessCashPerSecondAtInvestment = companyToInvestIn.CashPerSecond,
                InvestmentType = InvestmentType.Group,
            };
            var investorBusinessInvestment = new BusinessInvestment()
            {
                Business = user.Business,
                Investment = investment,
                InvestmentType = InvestmentType.Group,
                InvestmentDirection = InvestmentDirection.Investor
            }; 
            var partnerBusinessInvestment = new BusinessInvestment()
            {
                Business = partnerCompany,
                Investment = investment,
                InvestmentType = InvestmentType.Group,
                InvestmentDirection = InvestmentDirection.Partner
            };
            var companyToInvestInBusinessInvestment = new BusinessInvestment()
            {
                Business = companyToInvestIn,
                Investment = investment,
                InvestmentType = InvestmentType.Group,
                InvestmentDirection = InvestmentDirection.Investee
            };
            _context.BusinessInvestments.Add(investorBusinessInvestment);
            _context.BusinessInvestments.Add(partnerBusinessInvestment);
            _context.BusinessInvestments.Add(companyToInvestInBusinessInvestment);
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
            var groupInvestment = await _context.BusinessInvestments
                .Include(s => s.Investment)
                .Include(s => s.Business)
                .Where(s => s.Investment.Id == id)
                .ToListAsync();

            var user = await GetCurrentEntrepreneur();

            if (groupInvestment.Count != 3) return RedirectToAction("Index", "Home");

            var investingBusiness = groupInvestment.Where(s => s.InvestmentDirection == InvestmentDirection.Investor).ToList()[0];
            var partnerBusiness = groupInvestment.Where(s => s.InvestmentDirection == InvestmentDirection.Partner).ToList()[0];
            var businessToInvestIn = groupInvestment.Where(s => s.InvestmentDirection == InvestmentDirection.Investee).ToList()[0];

            if (partnerBusiness.Business.Id == investingBusiness.Business.Id) return RedirectToAction("Index", "Home");
            if (partnerBusiness.Business.Id != user.Business.Id) return RedirectToAction("Index", "Home");

            partnerBusiness.Business.CashPerSecond -= partnerBusiness.Investment.InvestmentAmount;
            partnerBusiness.Investment.InvestmentType = InvestmentType.Investment;

            investingBusiness.Business.CashPerSecond -= investingBusiness.Investment.InvestmentAmount;
            investingBusiness.Investment.InvestmentType = InvestmentType.Investment;

            businessToInvestIn.Business.CashPerSecond += businessToInvestIn.Investment.InvestmentAmount * 2;
            businessToInvestIn.Investment.InvestmentType = InvestmentType.Investment;

            _context.BusinessInvestments.Update(partnerBusiness);
            _context.BusinessInvestments.Update(investingBusiness);
            _context.BusinessInvestments.Update(businessToInvestIn);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CommitEspionage(int companyToEspionageId)
        {
            double CalculateEspionageCost(Business business)
            {
                var cost = business.Cash * 0.01;
                if (cost < 10000) cost = 10000;
                return cost;
            }
            var user = await GetCurrentEntrepreneur();
            var companyToEspionage = await _context.Business.SingleOrDefaultAsync(s => s.Id == companyToEspionageId);
            var costOfEspionage = CalculateEspionageCost(user.Business);
            if (companyToEspionage.AmountEmployed < 70) return RedirectToAction("Index", "Business", new { id = companyToEspionageId });
            if (user.Business.Cash < costOfEspionage) return RedirectToAction("Index", "Business", new { id = companyToEspionageId });
            if (user.Business.Id == companyToEspionageId) return RedirectToAction("Index", "Business", new { id = companyToEspionageId });

            user.Business.Cash -= costOfEspionage;
            _context.Business.Update(user.Business);
            await _context.SaveChangesAsync();

            var rand = new Random();
            if (((user.Business.EspionageChance * 100) - (companyToEspionage.EspionageDefense * 100)) < rand.Next(0, 100)) return Ok(JsonConvert.SerializeObject(new { SuccessfulEspionage = false, EspionageAmount = 0, UpdatedEspionageCost = CalculateEspionageCost(user.Business) }));

            var espionageAmountPercentage = CalculateEspionagePercentage(companyToEspionage, user.Business);
            var espionageAmount = companyToEspionage.CashPerSecond * espionageAmountPercentage;

            var investment = new Investment()
            {
                InvestmentAmount = (espionageAmount),
                InvestmentExpiration = DateTime.UtcNow.AddDays(1),
                InvestmentType = InvestmentType.Espionage,
            };
            var investorBusinessInvestment = new BusinessInvestment()
            {
                InvestmentType = InvestmentType.Espionage,
                Investment = investment,
                InvestmentDirection = InvestmentDirection.Investor,
            };
            var investeeBusinessInvestment = new BusinessInvestment()
            {
                InvestmentType = InvestmentType.Espionage,
                Investment = investment,
                InvestmentDirection = InvestmentDirection.Investee,
            };

            companyToEspionage.CashPerSecond = espionageAmount;
            companyToEspionage.EspionageDefense += .05F;

            user.Business.BusinessInvestments.Add(investorBusinessInvestment);
            companyToEspionage.BusinessInvestments.Add(investeeBusinessInvestment);
            _context.Business.Update(companyToEspionage);
            _context.Business.Update(user.Business);
            await _context.SaveChangesAsync();

            return Ok(JsonConvert.SerializeObject(new { SuccessfulEspionage = true, EspionageAmount = espionageAmount, UpdatedEspionageCost = CalculateEspionageCost(user.Business)}));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AttemptTheft(int companyToThieveId)
        {
            double CalculateEspionageCost(Business business)
            {
                return business.Cash * 0.01;
            }
            var user = await GetCurrentEntrepreneur();
            var companyToEspionage = await _context.Business.SingleOrDefaultAsync(s => s.Id == companyToThieveId);
            if (companyToEspionage == null) return RedirectToAction("Index", "Business", new { id = companyToThieveId });
            if (user.Business.Id == companyToThieveId) return RedirectToAction("Index", "Business", new { id = companyToThieveId });
            var costOfEspionage = CalculateEspionageCost(user.Business);
            if (!PurchasableHelper.HasBusinessPurchasedItem(user.Business.BusinessPurchases, 35)) return RedirectToAction("Index", "Business", new { id = companyToThieveId });

            user.Business.Cash -= costOfEspionage;
            _context.Business.Update(user.Business);
            await _context.SaveChangesAsync();

            var rand = new Random();
            if (((user.Business.EspionageChance * 100) - ((companyToEspionage.EspionageDefense * 100) *.85)) < rand.Next(0, 100)) return Ok(JsonConvert.SerializeObject(new { SuccessTheft = false, TheftAmount = 0, UpdatedEspionageCost = CalculateEspionageCost(user.Business) }));

            double theftPercentage = rand.Next(1, 15);
            var theftAmount = companyToEspionage.Cash * (theftPercentage / 100);

            companyToEspionage.Cash -= theftAmount;
            companyToEspionage.EspionageDefense += .05;
            user.Business.Cash += theftAmount;
            _context.Business.Update(companyToEspionage);
            _context.Business.Update(user.Business);
            await _context.SaveChangesAsync();

            return Ok(JsonConvert.SerializeObject(new { SuccessTheft = true, TheftAmount = theftAmount, UpdatedEspionageCost = CalculateEspionageCost(user.Business) }));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AttemptArson(int companyToArsonId)
        {
            double CalculateEspionageCost(Business business)
            {
                return business.Cash * 0.01;
            }
            var user = await GetCurrentEntrepreneur();
            var companyToEspionage = await _context.Business.SingleOrDefaultAsync(s => s.Id == companyToArsonId);
            if (companyToEspionage == null) return RedirectToAction("Index", "Business", new { id = companyToArsonId });
            if (user.Business.Id == companyToArsonId) return RedirectToAction("Index", "Business", new { id = companyToArsonId });
            if (!PurchasableHelper.HasBusinessPurchasedItem(user.Business.BusinessPurchases, 35)) return RedirectToAction("Index", "Business", new { id = companyToArsonId });

            var costOfEspionage = CalculateEspionageCost(user.Business);

            user.Business.Cash -= costOfEspionage;
            _context.Business.Update(user.Business);
            await _context.SaveChangesAsync();

            var rand = new Random();
            if (((user.Business.EspionageChance * 100) - ((companyToEspionage.EspionageDefense * 100) * .95)) < rand.Next(0, 100)) return Ok(JsonConvert.SerializeObject(new { SuccessTheft = false, ArsonAmount = 0, UpdatedEspionageCost = CalculateEspionageCost(user.Business) }));

            var arsonAmount = rand.Next(10, 40);

            companyToEspionage.MaxEmployeeAmount -= arsonAmount;
            companyToEspionage.EspionageDefense += .05;
            _context.Business.Update(companyToEspionage);
            await _context.SaveChangesAsync();

            return Ok(JsonConvert.SerializeObject(new { SuccessTheft = true, ArsonAmount = arsonAmount, UpdatedEspionageCost = CalculateEspionageCost(user.Business) }));
        }

        [NonAction]
        private async Task<Entrepreneur> GetCurrentEntrepreneur()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ent = await _context.Entrepreneurs
                .Include(s => s.Business)
                    .ThenInclude(s => s.BusinessInvestments)
                .Include(s => s.Business.BusinessPurchases)
                .FirstOrDefaultAsync(s => s.Id == userId);

            if (ent == null) return null;
            return ent;
        }

        [NonAction]
        private double CalculateEspionagePercentage(Business businessToEspionage, Business espionagingBusiness)
        {
            var espionageAmount = espionagingBusiness.EspionageChance - businessToEspionage.EspionageDefense;
            if (espionageAmount <= 0) espionageAmount = 0;
            else if (espionageAmount >= .5) espionageAmount = .5;
            return espionageAmount;
        }
    }
}
