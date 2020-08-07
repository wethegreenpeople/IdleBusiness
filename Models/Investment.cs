using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class Investment
    {
        public int Id { get; set; }
        public double InvestmentAmount { get; set; }
        public DateTime InvestmentExpiration { get; set; }
        public double InvestedBusinessCashAtInvestment { get; set; }
        public double InvestedBusinessCashPerSecondAtInvestment { get; set; }
        public InvestmentType InvestmentType { get; set; }

        public virtual ICollection<BusinessInvestment> BusinessInvestments { get; set; } = new List<BusinessInvestment>();
    }

    public enum InvestmentType
    {
        Investment = 10,
        Espionage = 20,
        Group = 30,
    }
}
