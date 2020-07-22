using IdleBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class InvestmentHelper
    {
        public static double CalculateInvestmentProfit(Investment investment)
        {
            var investmentPercentage = (investment.InvestmentAmount / investment.InvestedBusinessCashPerSecondAtInvestment);
            if (investmentPercentage > 1) investmentPercentage = 1;
            var profitSinceInvestment = (investment.BusinessToInvest.Cash - investment.InvestedBusinessCashAtInvestment);

            return profitSinceInvestment * investmentPercentage;
        }
    }
}
