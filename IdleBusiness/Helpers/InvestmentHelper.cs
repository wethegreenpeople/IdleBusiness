using IdleBusiness.Data;
using IdleBusiness.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class InvestmentHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ApplicationHelper _appHelper;

        public InvestmentHelper(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _appHelper = new ApplicationHelper(_logger);
        }

        public async Task<List<BusinessInvestment>> GetInvestmentsBusinessHasMadeInAnotherBusinessAsync(int investingBusiness, int businessToInvest)
        {
            var investingBusinessInvestments = (await _context.Business
                .SingleOrDefaultAsync(s => s.Id == investingBusiness))
                .BusinessInvestments
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investor)
                .Where(s => s.InvestmentType == InvestmentType.Investment);

            if (investingBusinessInvestments == null || investingBusinessInvestments.Count() <= 0) return null;

            return (await _context.BusinessInvestments
                .Include(s => s.Investment)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investee)
                .Where(s => s.Business.Id == businessToInvest)
                .ToListAsync())
                .Where(s => investingBusinessInvestments.Any(d => d.Investment.Id == s.Investment.Id))
                .ToList();
        }

        public static double CalculateInvestmentProfit(BusinessInvestment investment)
        {
            var investmentPercentage = (double)(investment.Investment.InvestmentAmount / investment.Investment.InvestedBusinessCashPerSecondAtInvestment);
            if (investmentPercentage > 1) investmentPercentage = 1;
            var profitSinceInvestment = (investment.Business.Cash - investment.Investment.InvestedBusinessCashAtInvestment);
            var profit = 0.00;
            if (profitSinceInvestment > 0) profit = profitSinceInvestment * investmentPercentage;

            return profit;
        }


    }
}
