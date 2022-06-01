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

    $(".fnDeleteEntrant").on("click", function () {
        var entrant = $(this).data("entrant");
        $.post("/Olympiad/DeleteEntrant", { entrantId: entrant }, function (data) {
            alert("deleted, please refresh");
        });
    });

    $("#openAddContestantModal").on("click", function () {
        $("#newName").hide();
        $("#searchName").focus();
    });

    $("#searchName").on("keypress", function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13') {
            $("#searchContestant").click();
            return false;
        }
    });

    $("#uploadFromPokerstars").on("click", function () {
        var eventId = $("#Id").val();
        window.location = "/Upload/Pokerstars/" + eventId;
    });

    $("#uploadFromWorldPuzzle").on("click", function () {
        var eventId = $("#Id").val();
        window.location = "/Upload/WorldPuzzle/" + eventId;
    });


    $("#uploadFromRussianFed").on("click", function () {
        var eventId = $("#Id").val();
        window.location = "/Upload/RussianDraughts/" + eventId;
    });

    $("#uploadFromPlayStrategy").on("click", function () {
        var eventId = $("#Id").val();
        window.location = "/Upload/PlayStrategy/" + eventId;
    });

    $("#uploadFromBoardGameArena").on("click", function () {
        var eventId = $("#Id").val();
        window.location = "/Upload/BoardGameArena/" + eventId;
    }); 

    $("#generateStandings").on("click", function () {
        var eventId = $("#Id").val();
        $.post("/Olympiad/FreezeEvent", { eventId: eventId }, function (data) {
            alert("generated, please refresh");
        });
    });

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
            $("#suggestions").append("<tr><td><a href=\"#\" class=\"addContToEvent\" data-event=\"" + eventId + "\" data-contestant=\"0\">New contestant</a><br /></td></tr>");
            $("#newNameLink").html("<a href=\"#\" class=\"addContToEvent\" data-event=\"" + eventId + "\" data-contestant=\"0\">Add</a>");
            $("#newName").show();
        }).fail(function () {
            alert("Too many results - please be more specific");
        });
    });

    $("#addContestantModal").on("click", "a", function () {
        var eventId = $(this).data("event");
        var contestantId = $(this).data("contestant");
        if (contestantId > 0) {
            $.post("/Contestant/AddContestantToEvent", { contestantId: contestantId, eventId: eventId }, function (data) {
                $("#addContestantModal").hide();
                $("form").submit();
            });
        } else {
            var newFirst = $("#newFirstName").val();
            var newLast = $("#newLastName").val();
            $.post("/Contestant/AddNewContestantToEvent", { firstName: newFirst, lastName: newLast, eventId: eventId }, function (data) {
                $("#addContestantModal").hide();
                $("form").submit();
            });
        }
    });
});
