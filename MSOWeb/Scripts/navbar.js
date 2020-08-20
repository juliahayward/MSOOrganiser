$(function () {
    $("#adminLogin").click(function () {
        window.location = "/Home/Login";
    });
    $("#adminLogout").click(function () {
        window.location = "/Home/Logout";
    });

    // Add the following code if you want the name of the file appear on select
    $(".custom-file-input").on("change", function () {
        var fileName = $(this).val().split("\\").pop();
        $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
    });

    setTimeout(function () { $("#bannerSuccess").hide("slow"); }, 3000);

    $("#searchContestant").on("click", function () {
        var name = $("#searchName").val();
        $.get("/Contestant/ContestantForName", { name: name }, function (data) {
            if (data.length == 0) {
                alert("No matches");
            }
            $("#suggestions").empty();
            var eventId = $("#Id").val();
            for (var i = 0; i < data.length; i++) {
                $("#suggestions").append("<tr><td><a href=\"#\" class=\"addContToEvent\" data-event=\"" + eventId + "\" data-contestant=\"" + data[i].Id + "\">" + data[i].Name + "</a><br />" + data[i].Nickname + "</td></tr>");
            }
        }).fail(function () {
            alert("Too many results - please be more specific");
        });
    });

    $("#addContestantModal").on("click", "a", function () {
        var eventId = $(this).data("event");
        var contestantId = $(this).data("contestant");
        alert("add " + contestantId + " to " + eventId);
        $.post("/Contestant/AddContestantToEvent", { contestantId: contestantId, eventId: eventId }, function (data) {
            alert("added")
        });
    });
});
