using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class BusinessInvestment
    {
        public int BusinessId { get; set; }
        public Business Business { get; set; }

        public int InvestmentId { get; set; }
        public Investment Investment { get; set; }

        public InvestmentType InvestmentType { get; set; }
        public InvestmentDirection InvestmentDirection { get; set; }
    }

    public enum InvestmentDirection
    {
        Investor,
        Investee,
        Partner,
    }
}
