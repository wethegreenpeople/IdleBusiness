using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class BusinessPurchase
    {
        public int BusinessId { get; set; }
        public Business Business { get; set; }

        public int PurchaseId { get; set; }
        public Purchasable Purchase { get; set; }

        public int AmountOfPurchases { get; set; }
    }
}
