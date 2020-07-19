using IdleBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Views.Models
{
    public class BusinessIndexVM
    {
        public Business Business { get; set; }
        public Entrepreneur CurrentEntrepreneur { get; set; }
        public bool HasCurrentEntrepreneurInvestedInBusiness { get; set; }
        public float TotalInvestedAmount { get; set; }
        public float InvestedProfits { get; set; }
        public List<Investment> Investments { get; set; }
    }
}
