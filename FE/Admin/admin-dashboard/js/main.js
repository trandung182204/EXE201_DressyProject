/* Full Screen */
const fullscreenButton = document.getElementById('sherah-header__full');
const htmlElement = document.documentElement;

if (fullscreenButton) {
    fullscreenButton.addEventListener('click', () => {
        if (document.fullscreenElement) {
            document.exitFullscreen();
        } else {
            htmlElement.requestFullscreen();
        }
    });
}


/* Dark Mode */
const button = document.getElementById("sherah-dark-light-button");
const action = document.querySelectorAll("#sherah-sidebarmenu__dark, #sherah-dark-light");

if (button) {
    button.addEventListener("click", function() {
        action.forEach((el) => {
            el.classList.toggle("active");
        });
        if (action[0]) localStorage.setItem("isDark", action[0].classList.contains("active"));
    });
}

if (localStorage.getItem("isDark") === "true") {
    action.forEach((el) => {
        el.classList.add("active");
    });
}




/* Sherah Sidebar Menu */
const cs_button = document.querySelectorAll(".sherah__sicon");
const cs_action = document.querySelectorAll(".sherah-smenu, .sherah-header, .sherah-adashboard");

if (cs_button && cs_button.length) {
    cs_button.forEach(button => {
        button.addEventListener("click", function() {
            cs_action.forEach((el) => {
                el.classList.toggle("sherah-close");
            });
            if (cs_action[0]) localStorage.setItem("iscicon", cs_action[0].classList.contains("sherah-close"));
        });
    });
}

if (localStorage.getItem("iscicon") === "true") {
    cs_action.forEach((el) => {
        el.classList.add("sherah-close");
    });
}

// Minimal stub used by pages that call initSherahLayout();
function initSherahLayout() {
    // Keep this lightweight: additional page-specific initializations can be added here.
}

