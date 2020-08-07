using IdleBusiness.Data;
using IdleBusiness.Models;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class EntrepreneurHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ApplicationHelper _appHelper;
        private readonly BusinessHelper _businessHelper;

        public EntrepreneurHelper(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _appHelper = new ApplicationHelper(_logger);
            _businessHelper = new BusinessHelper(_context, _logger);
        }

        public async Task<int> CalculateScore(int businessId)
        {
            var business = await _context.Business
                .Include(s => s.Owner)
                .SingleOrDefaultAsync(s => s.Id == businessId);

            var cashScore = business.Cash / 10000000;
            var lifeTimeScore = business.LifeTimeEarnings / 100000000;
            var cashPerSecondScore = business.CashPerSecond / 10;
            var employeesScore = business.AmountEmployed * 2;
            var itemScore = business.AmountOwnedItems * 3;
            var espionageScore = (business.EspionageChance * 100);

            var totalScore =
                //cashScore + 
                lifeTimeScore +
                cashPerSecondScore +
                employeesScore +
                itemScore +
                espionageScore
                ;

            var investments = await _businessHelper.GetInvestmentsCompanyHasMade(businessId);
            foreach (var item in investments)
            {
                var investmentPercentage = (item.Investor.Investment.InvestmentAmount / item.Investee.Investment.InvestedBusinessCashPerSecondAtInvestment);
                var investmentScore = (int)(300 * investmentPercentage);
                if (investmentScore > 300) investmentScore = 300;
                totalScore += investmentScore;
            }

            return (int)totalScore;
        }

        public async Task<Entrepreneur> UpdateEntrepreneurScore(int businessId)
        {
            var business = await _context.Business
                .Include(s => s.Owner)
                .SingleOrDefaultAsync(s => s.Id == businessId);

            business.Owner.Score = await CalculateScore(businessId);
            _context.Entrepreneurs.Update(business.Owner);
            await _appHelper.TrySaveChangesConcurrentAsync(_context);

            return business.Owner;
        }
    }
}
