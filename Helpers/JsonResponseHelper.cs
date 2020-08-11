using IdleBusiness.Models;
using IdleBusiness.Purchasables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class JsonResponseHelper
    {
        public static string PurchaseResponse(Business business, BusinessPurchase businessPurchase, PurchasableJsonReturn purchaseResponse = null)
        {
            var response = JsonConvert.SerializeObject(new
            {
                Business = business,
                BusinessPurchase = businessPurchase,
                PurchaseResponse = purchaseResponse,
            },
            new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            return response;
        }
    }
}
