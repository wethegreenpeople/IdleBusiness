using IdleBusiness.Data;
using IdleBusiness.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class PurchasableHelper
    {
        private readonly ApplicationDbContext _context;

        public PurchasableHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ApplyGlobalPurchaseBonus(Purchasable purchase, Business purchasingBusiness)
        {
            if (!purchase.IsGlobalPurchase) return;

            var sectorBusinesses = await _context.Business
                .Where(s => s.Sector.Id == purchasingBusiness.Sector.Id)
                .Where(s => s.Id != purchasingBusiness.Id)
                .ToListAsync();

            _context.Business.UpdateRange(sectorBusinesses.Select(s => 
                { 
                    s.CashPerSecond += purchase.CashModifier;
                    s.MaxEmployeeAmount += purchase.MaxEmployeeModifier;
                    return s; 
                }));
            await _context.SaveChangesAsync();
        }

        public static Purchasable AdjustPurchasableCostWithSectorBonus(Purchasable purchase, Business business)
        {
            if (business.Sector == null)
                return purchase;
            switch (business.Sector.Id)
            {
                case (int)SectorType.Tech:
                    if (purchase.Type.Id == (int)PurchasableTypeEnum.Buff)
                        purchase.Cost -= (float)(purchase.Cost * .1);
                    break;
                case (int)SectorType.Marketing:
                    if (purchase.Type.Id == (int)PurchasableTypeEnum.Employee)
                        purchase.Cost -= (float)(purchase.Cost * .1);
                    break;
                case (int)SectorType.RealEstate:
                    if (purchase.Type.Id == (int)PurchasableTypeEnum.RealEstate)
                        purchase.CashModifier = 0;
                    break;
            }

            return purchase;
        }

        public static Purchasable SwapPurchaseForUpgradeIfAlreadyBought(Purchasable purchase, Business business)
        {
            if (purchase.PurchasableUpgrade == null) return purchase;
            if (business.BusinessPurchases.Any(s => s.Purchase.Id == purchase.Id)) 
                return SwapPurchaseForUpgradeIfAlreadyBought(purchase.PurchasableUpgrade, business);

            return purchase;
        }

        public static bool EnsurePurchaseIsValid(Purchasable purchase, Business business, int purchaseCount)
        {
            switch (purchase.Type.Id)
            {
                case (int)PurchasableTypeEnum.Employee:
                    if (purchase.MaxEmployeeModifier >= 0)
                        if (business.MaxEmployeeAmount >= business.AmountEmployed + purchaseCount)
                            return true;
                    if (purchase.MaxEmployeeModifier < 0)
                        if (business.MaxEmployeeAmount - purchase.MaxEmployeeModifier >= business.AmountEmployed + purchaseCount)
                            return true;
                    break;
                case (int)PurchasableTypeEnum.Buff:
                    if (business.MaxItemAmount + purchase.MaxItemAmountModifier >= (business.AmountOwnedItems + purchaseCount))
                        return true;
                    break;
                case (int)PurchasableTypeEnum.RealEstate:
                    return true;
            }

            return false;
        }
    }
}
