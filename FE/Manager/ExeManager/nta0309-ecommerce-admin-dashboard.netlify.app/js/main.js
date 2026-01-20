/* =========================
   FULLSCREEN
========================= */
function initFullscreen() {
  const fullscreenButton = document.getElementById('sherah-header__full');
  if (!fullscreenButton) return;

  fullscreenButton.addEventListener('click', () => {
    if (document.fullscreenElement) {
      document.exitFullscreen();
    } else {
      document.documentElement.requestFullscreen();
    }
  });
}

/* =========================
   DARK MODE – FIX MINIMAL
========================= */
function initDarkMode() {
  const button = document.getElementById("sherah-dark-light-button");
  const body = document.getElementById("sherah-dark-light");

  if (!button || !body) return;

  // Load trạng thái đã lưu
  if (localStorage.getItem("isDark") === "true") {
    body.classList.add("dark");
  }

  button.addEventListener("click", () => {
    body.classList.toggle("dark");

    localStorage.setItem(
      "isDark",
      body.classList.contains("dark")
    );
  });
}




/* =========================
   SIDEBAR TOGGLE
========================= */
function initSidebar() {
  const cs_button = document.querySelectorAll(".sherah__sicon");
  const cs_action = document.querySelectorAll(
    ".sherah-smenu, .sherah-header, .sherah-adashboard"
  );

  if (cs_button.length === 0) return;

  cs_button.forEach(button => {
    button.addEventListener("click", function () {
      cs_action.forEach(el => el.classList.toggle("sherah-close"));

      localStorage.setItem(
        "iscicon",
        cs_action[0].classList.contains("sherah-close")
      );
    });
  });

  // Load saved state
  if (localStorage.getItem("iscicon") === "true") {
    cs_action.forEach(el => el.classList.add("sherah-close"));
  }
}

/* =========================
   INIT ALL (CALL AFTER FETCH)
========================= */
function initSherahLayout() {
  initFullscreen();
  initDarkMode();
  initSidebar();
}
