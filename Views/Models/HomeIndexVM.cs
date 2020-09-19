﻿using IdleBusiness.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Views.Models
{
    public class HomeIndexVM
    {
        public Business Business { get; set; }
        public List<Business> MostSuccessfulBusinesses { get; set; }
        public List<Purchasable> Purchasables { get; set; }
        public List<(Purchasable purchasable, int amount)> PurchasedItems { get; set; }
        public List<SelectListItem> AvailableSectors { get; set; }
        public List<(BusinessInvestment Investee, BusinessInvestment Investor)> InvestmentsInBusiness { get; set; }
        public List<(BusinessInvestment Investee, BusinessInvestment Investor)> EspionagesAgainstBusiness { get; set; }

        public string TotalInvestmentsInCompany => InvestmentsInBusiness?.Count.ToString();
        public string CurrentCash => Business?.Cash.ToString();
        public string TotalEmployed => Business?.AmountEmployed.ToString();
        public string UnreadMessageAmount => Business?.ReceivedMessages.Where(s => !s.ReadByBusiness).Count().ToString();
        
        public string TotalEmployeePurchasableCount => Purchasables.Where(s => s.Type.Id == 1).Count().ToString();
        public string TotalEmployeesUnlocked => Purchasables.Where(s => s.Type.Id == 1 && s.UnlocksAtTotalEarnings <= Business.LifeTimeEarnings).Count().ToString();
        public string TotalItemPurchasableCount => Purchasables.Where(s => s.Type.Id == 2).Count().ToString();
        public string TotalItemsUnlocked => Purchasables.Where(s => s.Type.Id == 2 && s.UnlocksAtTotalEarnings <= Business.LifeTimeEarnings).Count().ToString();

        public string TotalRealEstatePurchasableCount => Purchasables.Where(s => s.Type.Id == 3).Count().ToString();
        public string TotalRealEstateUnlocked => Purchasables.Where(s => s.Type.Id == 3 && s.UnlocksAtTotalEarnings <= Business.LifeTimeEarnings).Count().ToString();


        public bool HasSeekingAlphaItem => PurchasedItems.SingleOrDefault(s => s.purchasable.Id == 25).amount > 0;
        public bool HasSeekingAlphaProItem => PurchasedItems.SingleOrDefault(s => s.purchasable.Id == 26).amount > 0;

        public double? AdjustedPurchasableCost(int purchasableId)
        {
            if (PurchasedItems == null) return null;
            var purchase = PurchasedItems.SingleOrDefault(s => s.purchasable.Id == purchasableId);
            if (purchase.purchasable == null) return null;
            return (double)(purchase.purchasable.Cost * Math.Pow((1 + purchase.purchasable.PerOwnedModifier), purchase.amount));
        }
    }
}
