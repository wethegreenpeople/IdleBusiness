using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Product { get; set; }
        public float Cash { get; set; }
        public float LifeTimeEarnings { get; set; }
        public float CashPerSecond { get; set; }
        public DateTime LastCheckIn { get; set; }
        public float EspionageChance { get; set; }
        public float EspionageDefense { get; set; }
        public int MaxEmployeeAmount { get; set; }
        public int AmountEmployed { get; set; }
        public int MaxItemAmount { get; set; }
        public int AmountOwnedItems { get; set; }


        public virtual Sector Sector { get; set; }
        public virtual Entrepreneur Owner { get; set; }
        public virtual ICollection<BusinessPurchase> BusinessPurchases { get; set; }
        public virtual ICollection<Investment> Investments { get; set; } = new List<Investment>();

        public Business()
        {
            this.Cash = 100;
            this.LastCheckIn = DateTime.UtcNow;
            this.MaxEmployeeAmount = 50;
            this.MaxItemAmount = 5;
        }
    }
}
