﻿$(function () {
    $("#adminLogin").click(function () {
        window.location = "/Home/Login";
    });
    $("#adminLogout").click(function () {
        window.location = "/Home/Logout";
    });

    setTimeout(function () { $("#bannerSuccess").hide("slow"); }, 3000);
})