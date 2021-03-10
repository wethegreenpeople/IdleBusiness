using IdleBusiness.Data;
using IdleBusiness.Helpers;
using IdleBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Purchasables
{
    public class SpecialPurchasableRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly PurchasableHelper _purchasableHelper;
        public SpecialPurchasableRepo(ApplicationDbContext context, PurchasableHelper purchasableHelper)
        {
            _context = context;
            _purchasableHelper = purchasableHelper;
        }

        public ISpecialPurchasable GetSpecialPurchasable(Purchasable purchasable, Business business)
        {
            return purchasable.Id switch
            {
                29 => new SpecializedTraining(_context, business, _purchasableHelper),
                _ => null,
            };
        }
    }
}
