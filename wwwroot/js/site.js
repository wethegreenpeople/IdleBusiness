$(document).ready(function () {
    $(".formatted-number").on('numberChange', function () {
        var numberToFormat = $(this).attr("data-number-to-format");
        var formatStyle = $(this).attr("data-number-format-style")
        if (formatStyle != null) {
            $(this).text(numeral(numberToFormat).format(formatStyle));
        }
        else {
            $(this).text(numeral(numberToFormat).format('$0.000a'));
        }
    });

    $(".formatted-number").trigger('numberChange');
});

function DisableUnavailablePurchases() {
    var items = $("[data-purchase-item-id]");
    var currentCash = parseFloat($("#businessCurrentCash").attr("data-number-to-format"));
    items.each(function () {
        var itemCost = parseFloat($(this).attr("data-purchase-item-adjusted-cost"));
        var itemId = $(this).attr("data-purchase-item-id");
        var isSinglePurchase = $(this).attr("data-purchase-item-isSinglePurchase") == "True";
        var amountOwned = parseInt($(this).attr("data-purchase-item-amountOwned"));
        var itemType = parseInt($(this).attr("data-purchase-item-type"));
        if (currentCash < itemCost) { $("#purchase-item-" + itemId).addClass("disabled-card"); }
        else if (isSinglePurchase && amountOwned >= 1) { $("#purchase-item-" + itemId).addClass("disabled-card"); }
        else if (itemType == 2) {
            var currentMaxItems = parseInt($("#businessMaxAmountItemsAllowed").text());
            var amountOfOwnedItems = parseInt($("#businessTotalItemsOwned").text());
            var maxItemMod = parseFloat($(this).attr("data-purchase-item-maxItemMod"));
            if (amountOfOwnedItems >= currentMaxItems && maxItemMod <= 0) $("#purchase-item-" + itemId).addClass("disabled-card");

            if (itemId == "29") { // Intern training item
                var iternAmount = parseInt($("#amountOfItemsPurchased-item-1").text());
                if (iternAmount < 30) $("#purchase-item-" + itemId).addClass("disabled-card");
            }
        }
        else { $("#purchase-item-" + itemId).removeClass("disabled-card"); }
    });
}