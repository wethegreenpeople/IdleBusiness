$(document).ready(function () {
    $('#attemptEspionage').click(debounce(function () {
        $.ajax({
            url: "/business/CommitEspionage",
            type: "POST",
            data: {
                "companyToEspionageId": $("#businessId").val(),
            },
            headers: {
            },
            success: function (data) {
                $("#espionageAlert").removeClass("alert-primary");
                $("#espionageAlert").removeClass("alert-danger");
                var result = jQuery.parseJSON(data);
                console.log(result);
                if (result.SuccessfulEspionage) {
                    $("#espionageAlert").text("Espionage successful. Removed $" + result.EspionageAmount + " from business");
                    $("#espionageAlert").addClass("alert-primary");
                    $("#espionageAlert").fadeIn(500);
                    $("#espionageAlert").delay(5000).fadeOut(500);
                }
                if (!result.SuccessfulEspionage) {
                    $("#espionageAlert").text("Espionage unsuccessful");
                    $("#espionageAlert").addClass("alert-danger");
                    $("#espionageAlert").fadeIn(500);
                    $("#espionageAlert").delay(2000).fadeOut(500);
                }
                $("[name='espionageCost']").each(function () { $(this).attr("data-number-to-format", result.UpdatedEspionageCost).trigger('numberChange'); });
            },
            error: function (data) {
                console.log(data);
                window.location.href = "/business/index/" + $("#businessId").val();
            },
        });
    }, 500))
});