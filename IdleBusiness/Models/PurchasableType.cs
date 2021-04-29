using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class PurchasableType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Purchasable> Purchasables { get; set; }
    }

    public enum PurchasableTypeEnum
    {
        Employee = 1,
        Buff = 2,
        RealEstate = 3,
        Marketplace = 4,
    }
}
