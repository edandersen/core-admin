// Sidebar
var mobileDeviceBreak = 768;
var lastWindowWidth = window.innerWidth;
var sidebarWidth = localStorage.getItem("sidebar-width");

function checkIfSidebarNeedToBeClosed() {
    const windowWidth = window.innerWidth;

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
        document.querySelector("#page-content-wrapper").style.marginLeft = "";
    } else {
        document.querySelector("#page-content-wrapper").style.marginLeft = sidebarWidth + "px";
    }
}

checkIfSidebarNeedToBeClosed();

addEventListener("resize", (event) => {
    checkIfSidebarNeedToBeClosed();
});

window.addEventListener("DOMContentLoaded", (event) => {
    // Toggle the side navigation
    const sidebarToggle = document.body.querySelector("#sidebarToggle");
    if (sidebarToggle) {
        // Persist sidebar toggle between refreshes
        if (
            localStorage.getItem("sb|sidebar-toggle") === "true" &&
            document.querySelector(window).width() > mobileDeviceBreak
        ) {
            document.body.classList.toggle("sb-sidenav-toggled");
        }
        updateSidebarWidth();

        // Add event listener
        sidebarToggle.addEventListener("click", (event) => {
            event.preventDefault();
            document.body.classList.toggle("sb-sidenav-toggled");

            updateSidebarWidth();

            if (document.querySelector(window).width() > mobileDeviceBreak) {
                localStorage.setItem(
                    "sb|sidebar-toggle",
                    document.body.classList.contains("sb-sidenav-toggled")
                );
            }
        });
    }
});
