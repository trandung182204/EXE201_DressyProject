// js/auth-header.js
(function () {
  const isLocal = location.hostname === "localhost" || location.hostname === "127.0.0.1";
  const API_BASE = isLocal ? "http://localhost:5135" : "";

  function waitForEl(selector, timeout = 4000) {
    return new Promise((resolve, reject) => {
      const start = Date.now();
      const timer = setInterval(() => {
        const el = document.querySelector(selector);
        if (el) {
          clearInterval(timer);
          resolve(el);
          return;
        }
        if (Date.now() - start > timeout) {
          clearInterval(timer);
          reject(new Error("Timeout waiting for " + selector));
        }
      }, 50);
    });
  }

  function getAvatarEl() {
    // ưu tiên nếu bạn có id="auth-avatar"
    return (
      document.querySelector("#auth-avatar") ||
      document.querySelector(".sherah-header__author-img img")
    );
  }

  function normalizeRole(role) {
    // nếu backend trả "Provider"/"provider" -> hiển thị đẹp hơn
    if (!role) return "";
    const s = String(role).trim();
    return s.charAt(0).toUpperCase() + s.slice(1);
  }

  async function loadAuthToHeader() {
    const token = localStorage.getItem("token");
    if (!token) return;

    try {
      // ✅ đúng id theo header bạn gửi
      const [nameEl, roleEl] = await Promise.all([
        waitForEl("#auth-username"),
        waitForEl("#auth-role"),
      ]);

      const avatarEl = getAvatarEl();

      // hiển thị cache trước cho nhanh
      const cachedName = localStorage.getItem("fullName");
      const cachedRole = localStorage.getItem("role");
      const cachedAvatar = localStorage.getItem("avatarUrl");

      if (cachedName) nameEl.textContent = cachedName;
      if (cachedRole) roleEl.textContent = normalizeRole(cachedRole);
      if (avatarEl && cachedAvatar) avatarEl.src = cachedAvatar;

      // gọi API lấy thông tin mới nhất
      const res = await fetch(API_BASE + "/api/auth/me", {
        headers: { Authorization: "Bearer " + token },
      });

      // token hết hạn / không hợp lệ
      if (res.status === 401) return;

      if (!res.ok) {
        console.error("[AUTH-HEADER] /api/auth/me failed:", res.status);
        return;
      }

      const me = await res.json();

      // ⚠️ tuỳ backend trả key khác nhau, hỗ trợ nhiều tên field
      const fullName = me.fullName || me.name || me.username || me.userName;
      const role = me.role || me.userRole;
      const avatarUrl = me.avatarUrl || me.avatar || me.photoUrl;

      if (fullName) {
        nameEl.textContent = fullName;
        localStorage.setItem("fullName", fullName);
      }
      if (role) {
        roleEl.textContent = normalizeRole(role);
        localStorage.setItem("role", role);
      }
      if (avatarEl && avatarUrl) {
        avatarEl.src = avatarUrl;
        localStorage.setItem("avatarUrl", avatarUrl);
      }
    } catch (e) {
      console.error("[AUTH-HEADER] loadAuthToHeader error:", e);
    }
  }

  // dropdown delegation (giữ nguyên)
  function initHeaderDropdown() {
    document.addEventListener("click", function (e) {
      const author = e.target.closest(".sherah-header__author");
      const clickedInsideProfileCard = e.target.closest(".sherah-dropdown-card__profile");

      if (author) {
        const card = author.querySelector(".sherah-dropdown-card__profile");
        if (card) card.classList.toggle("is-open");
        return;
      }

      if (clickedInsideProfileCard) return;

      document.querySelectorAll(".sherah-dropdown-card__profile.is-open").forEach((x) => {
        x.classList.remove("is-open");
      });
    });
  }

  // logout
  document.addEventListener("click", function (e) {
    const a = e.target.closest("#btn-logout");
    if (!a) return;
    e.preventDefault();
    ["token", "role", "fullName", "userId", "providerId", "avatarUrl"].forEach((k) =>
      localStorage.removeItem(k)
    );
    window.location.href = "../../../dress-rental-template/wpdemo.redq.io/sites/dress-rental/html/index.html";
  });

  window.loadAuthToHeader = loadAuthToHeader;
  window.initHeaderDropdown = initHeaderDropdown;
})();
