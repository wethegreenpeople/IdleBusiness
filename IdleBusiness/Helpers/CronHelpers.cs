using IdleBusiness.Data;
using IdleBusiness.Extensions;
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
            var businessInvestments = _context.BusinessInvestments
                .Include(s => s.Business)
                .Include(s => s.Investment)
                    .ThenInclude(s => s.BusinessInvestments)
                .Where(s => s.InvestmentType == InvestmentType.Investment)
                .AsEnumerable()
                .Where(s => { var elapsedTime = DateTime.UtcNow - s.Investment.InvestmentExpiration.Date.ToUniversalTime(); return elapsedTime.TotalHours > 12; })
                .ToList()
                .GroupBy(s => s.InvestmentId);
                

            foreach (var item in businessInvestments)
            {
                var investorInvestment = item.First(s => s.InvestmentDirection == InvestmentDirection.Investor);
                var investeeInvestment = item.First(s => s.InvestmentDirection == InvestmentDirection.Investee);
                var investorsProfit = InvestmentHelper.CalculateInvestmentProfit(investeeInvestment);

                // Investor
                investorInvestment.Business.Cash += investorsProfit;
                investorInvestment.Business.CashPerSecond += investorInvestment.Investment.InvestmentAmount;
                investorInvestment.Business.ReceivedMessages.Add(new Message()
                {
                    DateReceived = DateTime.UtcNow,
                    MessageBody = $"You gained ${investorsProfit.ToKMB()} from your investments in {investeeInvestment.Business.Name}",
                    ReceivingBusinessId = investorInvestment.Business.Id,
                    ReadByBusiness = false
                });

                // Investee
                investeeInvestment.Business.CashPerSecond -= investeeInvestment.Investment.InvestmentAmount;
                investeeInvestment.Business.ReceivedMessages.Add(new Message()
                {
                    DateReceived = DateTime.UtcNow,
                    MessageBody = $"After investments were removed, you lost ${investeeInvestment.Investment.InvestmentAmount.ToKMB()} CPS",
                    ReceivingBusinessId = investeeInvestment.Business.Id,
                    ReadByBusiness = false
                });

                _context.Business.Update(investorInvestment.Business);
                _context.Business.Update(investeeInvestment.Business);

                _context.BusinessInvestments.Remove(investorInvestment);
                _context.Investments.Remove(investorInvestment.Investment);
                _context.BusinessInvestments.Remove(investeeInvestment);
                _context.Investments.Remove(investeeInvestment.Investment);
            }

            _context.SaveChanges();
        }

        public void RemoveEspionageInvestments()
        {
            var espionages = _context.BusinessInvestments
                .Include(s => s.Business)
                .Include(s => s.Investment)
                .Where(s => s.InvestmentType == InvestmentType.Espionage)
                .AsEnumerable()
                .Where(s => { var elapsedTime = DateTime.UtcNow - s.Investment.InvestmentExpiration.Date.ToUniversalTime(); return elapsedTime.TotalHours > 12; })
                .ToList();

            foreach (var item in espionages)
            {
                if (item.InvestmentDirection == InvestmentDirection.Investee)
                {
                    item.Business.CashPerSecond += item.Investment.InvestmentAmount;

                    item.Business.ReceivedMessages.Add(new Message()
                    {
                        DateReceived = DateTime.UtcNow,
                        MessageBody = $"After espionages were removed, you gained ${item.Investment.InvestmentAmount.ToKMB()} CPS",
                        ReceivingBusinessId = item.BusinessId,
                    });
                }

                _context.Business.Update(item.Business);
                _context.BusinessInvestments.Remove(item);
            }

            _context.SaveChanges();
        }
    }
}
