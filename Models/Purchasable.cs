using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class Purchasable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float CashModifier { get; set; }
        public float EspionageModifier { get; set; }
        public float EspionageDefenseModifier { get; set; }
        public int MaxEmployeeModifier { get; set; }
        public int MaxItemAmountModifier { get; set; }
        public float Cost { get; set; }
        public float PerOwnedModifier { get; set; }
        public string Description { get; set; }
        public float UnlocksAtTotalEarnings { get; set; }
        public bool IsSinglePurchase { get; set; }
        public bool IsGlobalPurchase { get; set; }

        public virtual ICollection<BusinessPurchase> BusinessPurchases { get; set; }

        public int PurchasableTypeId { get; set; }
        public virtual PurchasableType Type { get; set; }
    }
}
