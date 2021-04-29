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
        public double CashModifier { get; set; }
        public double EspionageModifier { get; set; }
        public double EspionageDefenseModifier { get; set; }
        public int MaxEmployeeModifier { get; set; }
        public int MaxItemAmountModifier { get; set; }
        public double Cost { get; set; }
        public double PerOwnedModifier { get; set; }
        public string Description { get; set; }
        public double UnlocksAtTotalEarnings { get; set; }
        public bool IsSinglePurchase { get; set; }
        public bool IsGlobalPurchase { get; set; }
        public bool IsUpgrade { get; set; }
        public int CreatedByBusinessId { get; set; }
        public int AmountAvailable { get; set; }

        public virtual ICollection<BusinessPurchase> BusinessPurchases { get; set; }

        public int PurchasableTypeId { get; set; }
        public virtual PurchasableType Type { get; set; }

        public int PurchasableUpgradeId { get; set; }
        public virtual Purchasable PurchasableUpgrade { get; set;  }
    }
}
