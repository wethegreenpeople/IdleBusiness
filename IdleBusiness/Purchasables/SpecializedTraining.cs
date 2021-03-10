using IdleBusiness.Data;
using IdleBusiness.Helpers;
using IdleBusiness.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Purchasables
{
    public class SpecializedTraining : ISpecialPurchasable
    {
        private readonly ApplicationDbContext _context;
        private readonly Business _business;
        private readonly PurchasableHelper _purchasableHelper;
        private readonly BusinessPurchase _randomPurchasable;

        public SpecializedTraining(ApplicationDbContext context, Business business, PurchasableHelper purchasableHelper)
        {
            _context = context;
            _business = business;
            _purchasableHelper = purchasableHelper;
            _randomPurchasable = _context.BusinessPurchases
                .Include(s => s.Purchase.Type)
                .Where(s => s.BusinessId == _business.Id)
                .Where(s => s.Purchase.Type.Id == 1)
                .Where(s => s.Purchase.Id != 1)
                .ToList()
                .OrderBy(r => Guid.NewGuid())
                .First();
        }

        public Purchasable Purchasable { get; set; }
        public PurchasableJsonReturn PurchaseResponse => new PurchasableJsonReturn() 
        { Id = "29", AfterPurchase = Purchasables.AfterPurchaseEffect.LockAfterPurchase, Message = $"Interns converted into {_randomPurchasable.Purchase.Name}" };

        public Task<object> AfterPurchaseEffect() => null;

        public async Task<object> OnPurchaseEffect()
        {
            var interns = _business.BusinessPurchases
                .Where(s => s.PurchaseId == 1)
                .FirstOrDefault();
            var amountOfInternsToRemove = 30;
            if (interns.AmountOfPurchases < amountOfInternsToRemove) return null;

            interns.AmountOfPurchases -= amountOfInternsToRemove;

            await _purchasableHelper.ApplyItemStatsToBussiness(_randomPurchasable.Purchase, _business, 1);

            _business.AmountEmployed -= amountOfInternsToRemove;
            _business.CashPerSecond -= amountOfInternsToRemove;
            _context.Business.Update(_business);
            _context.SaveChanges();

            return new PurchasableJsonReturn().CreateJsonReturn("29", $"Interns converted into {_randomPurchasable.Purchase.Name}", Purchasables.AfterPurchaseEffect.LockAfterPurchase);
        }
    }
}
