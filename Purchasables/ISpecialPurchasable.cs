using IdleBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Purchasables
{
    public interface ISpecialPurchasable
    {
        Purchasable Purchasable { get; set; }
        Task OnPurchaseEffect();
        Task AfterPurchaseEffect();
    }
}
