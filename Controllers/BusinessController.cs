using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdleBusiness.Data;
using IdleBusiness.Helpers;
using IdleBusiness.Models;
using IdleBusiness.Views.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdleBusiness.Controllers
{
    public class BusinessController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BusinessHelper _businessHelper;

        public BusinessController(ApplicationDbContext context)
        {
            _context = context;
            _businessHelper = new BusinessHelper(context);
        }

        public async Task<IActionResult> Index(int id)
        {
            var vm = new BusinessIndexVM();
            var business = await _businessHelper.UpdateGainsSinceLastCheckIn(id);
            vm.Business = business;
            vm.CurrentEntrepreneur = await GetCurrentEntrepreneur();
            vm.HasCurrentEntrepreneurInvestedInBusiness = business.Investments.Any(s => s.InvestingBusinessId == vm.CurrentEntrepreneur.Business.Id && s.InvestmentType != InvestmentType.Espionage);
            if (vm.HasCurrentEntrepreneurInvestedInBusiness)
            {
                var investments = business.Investments
                    .Where(s => s.InvestingBusinessId == vm.CurrentEntrepreneur.Business.Id)
                    .Where(s => s.InvestmentType == InvestmentType.Investment)
                    .ToList();
                vm.Investments = investments;
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

        [HttpPost]
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
                .FirstOrDefaultAsync(s => s.Id == userId);

            if (ent == null) return null;
            return ent;
        }
    }
}
