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
        public double Cash { get; set; }
        public double LifeTimeEarnings { get; set; }
        public double CashPerSecond { get; set; }
        public DateTime LastCheckIn { get; set; }
        public double EspionageChance { get; set; }
        public double EspionageDefense { get; set; }
        public int MaxEmployeeAmount { get; set; }
        public int AmountEmployed { get; set; }
        public int MaxItemAmount { get; set; }
        public int AmountOwnedItems { get; set; }
        public byte[] RowVersion { get; set; }


        public virtual Sector Sector { get; set; }
        public virtual Entrepreneur Owner { get; set; }
        public virtual ICollection<BusinessPurchase> BusinessPurchases { get; set; }
        public virtual ICollection<Investment> Investments { get; set; } = new List<Investment>();
        public virtual ICollection<Investment> GroupInvestments { get; set; } = new List<Investment>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        public Business()
        {
            this.Cash = 100;
            this.LastCheckIn = DateTime.UtcNow;
            this.MaxEmployeeAmount = 50;
            this.MaxItemAmount = 5;
        }
    }
}
