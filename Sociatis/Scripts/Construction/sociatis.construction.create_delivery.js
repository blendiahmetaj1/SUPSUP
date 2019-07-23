﻿function calculateTotal(amount, offerID) {
    var $input = $("input[data-offerid='" + offerID + "']");
    if (!(amount) || !(offerID))
        return;

    var min = Number($input.attr("min"));
    var max = Number($input.attr("max"));

    if (amount < min) {
        $input.val(min);
        amount = min;
    } else if (amount > max) {
        $input.val(max);
        amount = max;
    }

    var data =
        {
            offerID: offerID,
            amount: amount,
            constructionID: Sociatis.Global.ConstructionID
        };

    var url = "/Construction/CalculateTotal";

    var updateField = function (data) {
        $("div[data-pricecostofferid='" + offerID + "']").text(data.ProductPrice);
        $("div[data-fuelcostofferid='" + offerID + "']").text(data.FuelPrice);
        console.log(data);
    };

    $.postJSON(url, data, data => Sociatis.HandleJson(data, updateField));
}

function recalculateTotal(offerID) {
    var amount = $("input[data-offerid='" + offerID + "']").val();
    calculateTotal(amount, offerID);
}

$(function () {
    var walletChooser = new Sociatis.WalletChooser($("#walletChooser"), onAmountChosen);

    $("#buyAsConstructionButton").click(buyAsConstruction);

    var currentOfferID;
    var symbol;
    function buyAsConstruction(e) {
        var $dialog = $("#walletChooser");
        currentOfferID = $(e.target).data("offerid");
        var cost = $("[data-pricecostofferid='" + currentOfferID + "']").text();
        $("#neededMoney").text(cost);
        symbol = $("[data-pricecostofferid='" + currentOfferID + "']").data("symbol");
        walletChooser.SetCurrencyID($("[data-pricecostofferid='" + currentOfferID + "']").data("currencyid"));
        walletChooser.onWalletChange();

        $dialog.dialog({
            title: "Choose wallet"
        });

        $("#deliveryAmount").val($("input[data-offerid='" + currentOfferID + "']").val());
        $("#deliveryOfferID").val(currentOfferID);
        
    }

    function onAmountChosen(amount) {
        $("#availableMoney").text(amount + " " + symbol);
    }

    $("#proceedWallet").click(function () {
        Sociatis.UI.BlockUI();
    });


});


