// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


$(document).ready(function () {
    // If prefers-color-scheme is not supported, on older browser for example (IE).
    // Use the light theme.
    if (window.matchMedia("(prefers-color-scheme: dark)").media === "not all") {
        $(".meta-css-dark").remove();
    }
});