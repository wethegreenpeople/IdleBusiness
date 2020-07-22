using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class Investment
    {
        public int Id { get; set; }
        public float InvestmentAmount { get; set; }
        public DateTime InvestmentExpiration { get; set; }
        public double InvestedBusinessCashAtInvestment { get; set; }
        public float InvestedBusinessCashPerSecondAtInvestment { get; set; }
        public InvestmentType InvestmentType { get; set; }


        public int BusinessToInvestId { get; set; }
        public virtual Business BusinessToInvest { get; set; }

        public int InvestingBusinessId { get; set; }
        public virtual Business InvestingBusiness { get; set; }

    }

    public enum InvestmentType
    {
        Investment = 10,
        Espionage = 20,
    }
}
