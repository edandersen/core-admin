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

// Sidebar
var mobileDeviceBreak = 768;
var lastWindowWidth = $(window).width();
var sidebarWidth = localStorage.getItem("sidebar-width");

function checkIfSidebarNeedToBeClosed() {
  const windowWidth = $(window).width();

  // Only update at the mobile UI transition
  if (
    windowWidth <= mobileDeviceBreak &&
    lastWindowWidth >= mobileDeviceBreak &&
    !document.body.classList.contains("sb-sidenav-toggled")
  ) {
    document.body.classList.add("sb-sidenav-toggled");
  } else if (
    windowWidth >= mobileDeviceBreak &&
    lastWindowWidth <= mobileDeviceBreak &&
    document.body.classList.contains("sb-sidenav-toggled")
  ) {
    if (localStorage.getItem("sb|sidebar-toggle") === "true") {
      document.body.classList.add("sb-sidenav-toggled");
    } else {
      document.body.classList.remove("sb-sidenav-toggled");
    }
  }
  updateSidebarWidth();
  lastWindowWidth = windowWidth;
}

function updateSidebarWidth() {
  if (document.body.classList.contains("sb-sidenav-toggled")) {
    $("#page-content-wrapper").css({ marginLeft: "" });
  } else {
    $("#page-content-wrapper").css({ marginLeft: sidebarWidth + "px" });
  }
}

$(document).ready(function () {
  checkIfSidebarNeedToBeClosed();
});

$(window).resize(function () {
  checkIfSidebarNeedToBeClosed();
});

window.addEventListener("DOMContentLoaded", (event) => {
  // Toggle the side navigation
  const sidebarToggle = document.body.querySelector("#sidebarToggle");
  if (sidebarToggle) {
    // Persist sidebar toggle between refreshes
    if (
      localStorage.getItem("sb|sidebar-toggle") === "true" &&
      $(window).width() > mobileDeviceBreak
    ) {
      document.body.classList.toggle("sb-sidenav-toggled");
    }
    updateSidebarWidth();

    // Add event listener
    sidebarToggle.addEventListener("click", (event) => {
      event.preventDefault();
      document.body.classList.toggle("sb-sidenav-toggled");

      updateSidebarWidth();

      if ($(window).width() > mobileDeviceBreak) {
        localStorage.setItem(
          "sb|sidebar-toggle",
          document.body.classList.contains("sb-sidenav-toggled")
        );
      }
    });
  }
});

// Sidebar resizing
$(document).ready(function () {
  if (sidebarWidth) {
    $("#sidebar-wrapper").width(sidebarWidth);
    $(".navbar-brand").width(sidebarWidth);
  }

  $("#sidebar-wrapper").resizable({
    resize: function (e, ui) {
      sidebarWidth = ui.size.width;
      localStorage.setItem("sidebar-width", sidebarWidth);
      $("#page-content-wrapper").css({ marginLeft: ui.size.width + "px" });
      $(".navbar-brand").width(sidebarWidth);
    },
  });
});
