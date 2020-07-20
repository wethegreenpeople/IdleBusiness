using IdleBusiness.Data;
using IdleBusiness.Models;
using Microsoft.EntityFrameworkCore;
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

        public BusinessHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<float> CalculateGainsSinceLastCheckIn(int businessId)
        {
            var business = await _context.Business.SingleOrDefaultAsync(s => s.Id == businessId);
            var secondsSinceLastCheckin = (DateTime.UtcNow - business.LastCheckIn).TotalSeconds;
            return (float)(secondsSinceLastCheckin * business.CashPerSecond);
        }

        public async Task<Business> UpdateGainsSinceLastCheckIn(int businessId)
        {
            var business = await _context.Business
                .Include(s => s.Investments)
                .SingleOrDefaultAsync(s => s.Id == businessId);
            var gains = await CalculateGainsSinceLastCheckIn(businessId);

            business.Cash += gains;
            if (business.LifeTimeEarnings < business.Cash) business.LifeTimeEarnings = business.Cash;
            business.LifeTimeEarnings += gains;
            business.LastCheckIn = DateTime.UtcNow;
            await ApplicationHelper.TrySaveChangesConcurrentAsync(_context);

            return business;
        }

        public async Task<List<Investment>> GetInvestmentsInCompany(int businessId)
        {
            return await _context.Investments
                .Where(s => s.BusinessToInvest.Id == businessId)
                .ToListAsync();
        }
    }
}
