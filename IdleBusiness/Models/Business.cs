using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public double BusinessScore { get; set; }
        public byte[] RowVersion { get; set; }

        [NotMapped]
        private double _espionageCost;
        [NotMapped]
        public double EspionageCost
        {
            get
            {
                var cost = this.Cash * 0.01;
                if (cost < 10000) cost = 10000;
                return cost;
            }
            set
            {
                _espionageCost = value;
            }
        }


        public virtual Sector Sector { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public virtual Entrepreneur Owner { get; set; }
        public virtual ICollection<BusinessPurchase> BusinessPurchases { get; set; }
        public virtual ICollection<BusinessInvestment> BusinessInvestments { get; set; } = new List<BusinessInvestment>();
        public virtual ICollection<Investment> GroupInvestments { get; set; } = new List<Investment>();
        [Newtonsoft.Json.JsonIgnore]
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        [Newtonsoft.Json.JsonIgnore]
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        public Business()
        {
            this.Cash = 100;
            this.LifeTimeEarnings = 100;
            this.LastCheckIn = DateTime.UtcNow;
            this.MaxEmployeeAmount = 50;
            this.MaxItemAmount = 5;
        }
    }
}
