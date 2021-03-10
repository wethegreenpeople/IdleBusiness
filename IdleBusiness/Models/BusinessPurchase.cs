using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class BusinessPurchase
    {
        public int BusinessId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public virtual Business Business { get; set; }

        public int PurchaseId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public virtual Purchasable Purchase { get; set; }

        public int AmountOfPurchases { get; set; }
    }
}
