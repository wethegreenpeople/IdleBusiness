using IdleBusiness.Data;
using IdleBusiness.Extensions;
using IdleBusiness.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class BusinessHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ApplicationHelper _appHelper;

        public BusinessHelper(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _appHelper = new ApplicationHelper(_logger);
        }

        public async Task<double> CalculateGainsSinceLastCheckIn(int businessId)
        {
            var business = await _context.Business.SingleOrDefaultAsync(s => s.Id == businessId);
            var secondsSinceLastCheckin = (DateTime.UtcNow - business.LastCheckIn).TotalSeconds;
            return (double)(secondsSinceLastCheckin * business.CashPerSecond);
        }

        public async Task<Business> UpdateGainsSinceLastCheckIn(int businessId)
        {
            var business = await _context.Business
                .Include(s => s.BusinessInvestments)
                .SingleOrDefaultAsync(s => s.Id == businessId);
            var gains = await CalculateGainsSinceLastCheckIn(businessId);

            business.Cash += gains;
            if (business.LifeTimeEarnings < business.Cash) business.LifeTimeEarnings = business.Cash;
            business.LifeTimeEarnings += gains;

            if ((DateTime.UtcNow - business.LastCheckIn).TotalHours > 8)
            {
                business.ReceivedMessages.Add(new Message()
                {
                    DateReceived = DateTime.UtcNow,
                    MessageBody = $"You've gained {gains.ToKMB()} since you last visited on {business.LastCheckIn}",
                    ReceivingBusinessId = business.Id,
                });
            }

            business.LastCheckIn = DateTime.UtcNow;
            await _appHelper.TrySaveChangesConcurrentAsync(_context);

            return business;
        }

        public async Task<List<(BusinessInvestment Investee, BusinessInvestment Investor)>> GetInvestmentsInCompany(int businessId)
        {
            var investments = await _context.BusinessInvestments
                .Include(s => s.Business)
                .Include(s => s.Investment)
                .Where(s => s.BusinessId == businessId)
                .Where(s => s.InvestmentType == InvestmentType.Investment)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investee)
                .ToListAsync();

            return (await _context.BusinessInvestments
                .Include(s => s.Investment)
                .Include(s => s.Business)
                .Where(s => s.BusinessId != businessId)
                .Where(s => s.InvestmentType == InvestmentType.Investment)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investor)
                .ToListAsync())
                .Where(s => investments.Any(d => d.Investment.Id == s.Investment.Id))
                .Select(s => (Investee: investments.Single(d => d.Investment.Id == s.Investment.Id), Investor: s))
                .ToList();
        }

        public async Task<List<(BusinessInvestment Investee, BusinessInvestment Investor)>> GetInvestmentsCompanyHasMade(int businessId)
        {
            var investments = await _context.BusinessInvestments
                .Include(s => s.Business)
                .Include(s => s.Investment)
                .Where(s => s.BusinessId == businessId)
                .Where(s => s.InvestmentType == InvestmentType.Investment)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investor)
                .ToListAsync();

            return (await _context.BusinessInvestments
                .Include(s => s.Investment)
                .Include(s => s.Business)
                .Where(s => s.BusinessId != businessId)
                .Where(s => s.InvestmentType == InvestmentType.Investment)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investee)
                .ToListAsync())
                .Where(s => investments.Any(d => d.Investment.Id == s.Investment.Id))
                .Select(s => (Investee: s, Investor: investments.Single(d => d.Investment.Id == s.Investment.Id)))
                .ToList();
        }

        public async Task<List<(BusinessInvestment Investee, BusinessInvestment Investor)>> GetEspionagesAgainstCompany(int businessId)
        {
            var espionages = await _context.BusinessInvestments
                .Include(s => s.Business)
                .Include(s => s.Investment)
                .Where(s => s.BusinessId == businessId)
                .Where(s => s.InvestmentType == InvestmentType.Espionage)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investee)
                .ToListAsync();

            return (await _context.BusinessInvestments
                .Include(s => s.Investment)
                .Include(s => s.Business)
                .Where(s => s.BusinessId != businessId)
                .Where(s => s.InvestmentType == InvestmentType.Espionage)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investor)
                .ToListAsync())
                .Where(s => espionages.Any(d => d.Investment.Id == s.Investment.Id))
                .Select(s => (Investee: espionages.Single(d => d.Investment.Id == s.Investment.Id), Investor: s))
                .ToList();
        }

        public async Task<List<(BusinessInvestment Investee, BusinessInvestment Investor)>> GetEspionagesCompanyHasComitted(int businessId)
        {
            var espionages = await _context.BusinessInvestments
                .Include(s => s.Business)
                .Include(s => s.Investment)
                .Where(s => s.BusinessId == businessId)
                .Where(s => s.InvestmentType == InvestmentType.Espionage)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investor)
                .ToListAsync();

            return (await _context.BusinessInvestments
                .Include(s => s.Investment)
                .Include(s => s.Business)
                .Where(s => s.BusinessId != businessId)
                .Where(s => s.InvestmentType == InvestmentType.Espionage)
                .Where(s => s.InvestmentDirection == InvestmentDirection.Investee)
                .ToListAsync())
                .Where(s => espionages.Any(d => d.Investment.Id == s.Investment.Id))
                .Select(s => (Investee: s, Investor: espionages.Single(d => d.Investment.Id == s.Investment.Id)))
                .ToList();
        }
    }
}
