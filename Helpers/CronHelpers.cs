using IdleBusiness.Data;
using IdleBusiness.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class CronHelpers
    {
        private readonly ApplicationDbContext _context;

        public CronHelpers(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AwardInvestmentProfits()
        {
            var investments = _context.Investments
                .Include(s => s.BusinessToInvest)
                .Include(s => s.InvestingBusiness)
                .Where(s => s.InvestmentExpiration.Date <= DateTime.UtcNow)
                .Where(s => s.InvestmentType == InvestmentType.Investment)
                .ToList();

            foreach (var item in investments)
            {
                var investorsProfit = InvestmentHelper.CalculateInvestmentProfit(item);

                item.InvestingBusiness.Cash += investorsProfit;
                item.InvestingBusiness.CashPerSecond += item.InvestmentAmount;
                item.BusinessToInvest.CashPerSecond -= item.InvestmentAmount;

                item.InvestingBusiness.ReceivedMessages.Add(new Message()
                {
                    DateReceived = DateTime.UtcNow,
                    MessageBody = $"You gained ${(int)investorsProfit} from your investments in {item.BusinessToInvest.Name}",
                    ReceivingBusinessId = item.InvestingBusinessId,
                });

                item.BusinessToInvest.ReceivedMessages.Add(new Message()
                {
                    DateReceived = DateTime.UtcNow,
                    MessageBody = $"After investments were removed, you lost ${(int)item.InvestmentAmount} CPS",
                    ReceivingBusinessId = item.BusinessToInvestId,
                });

                _context.Business.Update(item.InvestingBusiness);
                _context.Business.Update(item.BusinessToInvest);
                _context.Investments.Remove(item);
            }

            _context.SaveChanges();
        }

        public void RemoveEspionageInvestments()
        {
            var espionages = _context.Investments
                .Include(s => s.BusinessToInvest)
                .Where(s => s.InvestmentType == InvestmentType.Espionage)
                .Where(s => s.InvestmentExpiration.Date <= DateTime.UtcNow)
                .ToList();

            foreach (var item in espionages)
            {
                item.BusinessToInvest.CashPerSecond += item.InvestmentAmount;

                item.BusinessToInvest.ReceivedMessages.Add(new Message()
                {
                    DateReceived = DateTime.UtcNow,
                    MessageBody = $"After espionages were removed, you gained ${(int)item.InvestmentAmount} CPS",
                    ReceivingBusinessId = item.BusinessToInvestId,
                });

                _context.Business.Update(item.BusinessToInvest);
                _context.Investments.Remove(item);
            }

            _context.SaveChanges();
        }
    }
}
