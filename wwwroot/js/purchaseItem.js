var purchaseCount = 0;
var purchaseId = 0;
var currentCash = 0;
var cashToSub = 0;
var totalEmployed = 0;
var totalItemsOwned = 0;
var isPurchaseValid = false;

function ResetValues() {
     purchaseCount = 0;
     purchaseId = 0;
     currentCash = 0;
     cashToSub = 0;
     totalEmployed = 0;
     totalItemsOwned = 0;
     isPurchaseValid = false;
     serverPurchase = null;
}

function CheckIfPurchaseIsValid(button) {
    totalEmployed = parseInt($("#businessTotalEmployed").text());
    totalItemsOwned = parseInt($("#businessTotalItemsOwned").text());
    currentCash = parseInt($("#businessCurrentCash").attr("data-number-to-format"));
    cashToSub = parseInt(button.getAttribute("data-purchase-item-adjusted-cost"));
    if (cashToSub > currentCash) { return false; }

    if (button.getAttribute("data-purchase-item-type") == 1) {
        var maxEmployees = parseInt($("#businessMaxEmployees").text());
        if (totalEmployed >= maxEmployees) { return false; }
    }
    if (button.getAttribute("data-purchase-item-type") == 2) {
        var maxItems = parseInt($("#businessMaxAmountItemsAllowed").text());
        if (totalItemsOwned >= maxItems + parseInt(button.getAttribute("data-purchase-item-maxItemMod"))) { return false; }
    }

    return true;
}

function UpdateAdjustedItemCost(button) {
    var currentCost = parseFloat($("#purchase-item-" + button.getAttribute("data-purchase-item-id") + "-cost").attr("data-number-to-format"));
    var adjustedItemCost = (currentCost += parseFloat((currentCost * parseFloat(button.getAttribute("data-purchase-item-PerOwned"))))).toFixed(2);
    $("#purchase-item-" + purchaseId + "-cost").attr("data-number-to-format", adjustedItemCost).trigger('numberChange');

    button.setAttribute("data-purchase-item-adjusted-cost", adjustedItemCost);
}

function UpdateBusinessCurrentCash() {
    $("#businessCurrentCash").attr("data-number-to-format", (currentCash - cashToSub)).trigger('numberChange');
}

function UpdateBusinessCashPerSecond(button) {
    var currentCps = parseInt($("#businessCashPerSecond").attr("data-number-to-format"));
    var cpsIncrease = parseInt(button.getAttribute("data-purchase-item-cps"));
    $("#businessCashPerSecond").attr("data-number-to-format", currentCps + cpsIncrease).trigger('numberChange');
}

function UpdateEspionageDefense(button) {
    var espDefense = parseFloat($("#businessEspionageDefense").attr("data-number-to-format"));
    $("#businessEspionageDefense").attr("data-number-to-format", espDefense + parseFloat(button.getAttribute("data-purchase-item-espDefenseMod"))).trigger('numberChange');
}

function UpdateEspionage(button) {
    var espionageIncrease = parseFloat($(button).attr("data-purchase-item-ei"));
    if (!Number.isNaN(espionageIncrease)) {
        var currentEspionage = parseFloat($("#businessEspionageChance").text()) / 100;
        $("#businessEspionageChance").text(parseFloat((currentEspionage + espionageIncrease) * 100).toFixed(2));
    }
}

function UpdateBusinessMaxEmployees(button) {
    if ($(button).attr("data-purchase-item-type") == 3) {
        $("#businessMaxEmployees").text(parseInt(button.getAttribute("data-purchase-item-maxEmployeeMod")) + parseInt($("#businessTotalEmployed").text()));
    }
}

function UpdateBusinessMaxItems(button) {
    if ($(button).attr("data-purchase-item-type") == 2) {
        $("#businessTotalItemsOwned").text(totalItemsOwned += 1);
        var maxItemAmount = parseInt($("#businessMaxAmountItemsAllowed").text());
        var maxItemMod = parseInt(button.getAttribute("data-purchase-item-maxItemMod"));
        $("#businessMaxAmountItemsAllowed").text(maxItemAmount + maxItemMod)
    }
}

function UpdateTotalEmployed(button) {
    if (button.getAttribute("data-purchase-item-type") == 1) {
        $("#businessTotalEmployed").text(totalEmployed += 1);
    }
}

function UpdateBusinessOwnedItems(button) {
    var amountOfItemsPurchased = $("#amountOfItemsPurchased-item-" + $(button).attr("data-purchase-item-id"));
    var adjustedPurchasedAmount = parseInt(amountOfItemsPurchased.text()) + 1;
    amountOfItemsPurchased.text(adjustedPurchasedAmount);

    $(button).attr("data-purchase-item-amountOwned", adjustedPurchasedAmount);
}

function UiPurchaseItem() {
    $('[data-purchase-item-id]').click(function () {
        isPurchaseValid = CheckIfPurchaseIsValid(this);
        if (!isPurchaseValid) { return };

        purchaseId = this.getAttribute("data-purchase-item-id");

        UpdateAdjustedItemCost(this);

        purchaseCount++;

        UpdateBusinessCurrentCash();
        UpdateBusinessCashPerSecond(this);
        UpdateEspionageDefense(this);
        UpdateEspionage(this);
        UpdateBusinessMaxEmployees(this);
        UpdateTotalEmployed(this);
        UpdateBusinessOwnedItems(this);       
        UpdateBusinessMaxItems(this);

        DisableUnavailablePurchases();
    });
}

function ServerPurchaseItem() {
    $('[data-purchase-item-id]').click(debounce(function () {
        $.ajax({
            url: "/home/PurchaseItem",
            type: "POST",
            data: {
                "purchasableId": purchaseId,
                "purchaseCount": purchaseCount,
            },
            headers: {
            },
            success: function (data) {
            },
            error: function (data) {
                console.log(data);
                window.location.href = "/";
            },
        });

        ResetValues();
    }, 250))
}

$(document).ready(function () {
    UiPurchaseItem();
    ServerPurchaseItem();
});