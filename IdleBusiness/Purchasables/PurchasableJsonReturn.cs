using IdleBusiness.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdleBusiness.Purchasables
{
    public class PurchasableJsonReturn
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public AfterPurchaseEffect AfterPurchase { get; set; }

        public string CreateJsonReturn(string purchasableId, string returnMessage = "", AfterPurchaseEffect afterPurchase = AfterPurchaseEffect.Nothing)
        {
            this.Id = purchasableId;
            this.Message = returnMessage;
            AfterPurchase = afterPurchase;

            return JsonConvert.SerializeObject(this);
        }
    }

    public enum AfterPurchaseEffect
    {
        Nothing,
        Redirect,
        LockAfterPurchase
    }
}
