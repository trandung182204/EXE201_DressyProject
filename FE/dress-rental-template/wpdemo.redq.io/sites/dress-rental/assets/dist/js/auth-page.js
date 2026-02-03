// SỬA base URL theo BE của bạn:
const isLocal =
  location.hostname === "localhost" ||
  location.hostname === "127.0.0.1";

// Production: API_BASE rỗng để gọi /api/... (nginx proxy đến backend)
// Local: sử dụng localhost:5135
const API_BASE = isLocal
  ? "http://localhost:5135"
  : "";

console.log("[AUTH] Environment:", isLocal ? "LOCAL" : "PRODUCTION");
console.log("[AUTH] API_BASE:", API_BASE);
console.log("[AUTH] Hostname:", location.hostname);

/**
 * Kiểm tra xem có đang ở môi trường production không
 */
function isProduction() {
  return location.hostname === "xungxinh.io.vn" ||
    location.hostname === "www.xungxinh.io.vn";
}

/**
 * Xây dựng đường dẫn redirect dựa trên role
 * Production: tất cả đều về index.html (cùng thư mục)
 * Local: sử dụng đường dẫn đầy đủ
 */
function mapRoleToRedirect(role) {
  const roleLower = (role || "customer").toLowerCase().trim();

  console.log("[AUTH] mapRoleToRedirect - role:", roleLower);
  console.log("[AUTH] mapRoleToRedirect - isProduction:", isProduction());
  console.log("[AUTH] mapRoleToRedirect - isLocal:", isLocal);

  // Production - tất cả các trang đều ở cùng thư mục html/
  // Vì nginx config có root tại .../html, nên chỉ cần dùng tên file
  if (!isLocal) {
    console.log("[AUTH] Using production redirect: index.html");
    // Dùng relative path vì tất cả file đều ở cùng thư mục
    return "index.html";
  }

  // Local development - cần đường dẫn đầy đủ
  const path = location.pathname;
  const marker = "dress-rental-template";
  const idx = path.indexOf(marker);
  const basePath = idx > 0 ? path.substring(0, idx) : "/";

  console.log("[AUTH] Using local redirect with basePath:", basePath);

  switch (roleLower) {
    case "admin":
      return `${basePath}Admin/admin-dashboard/index.html`;
    case "provider":
      return `${basePath}Manager/ExeManager/nta0309-ecommerce-admin-dashboard.netlify.app/index.html`;
    case "customer":
    default:
      return `${basePath}dress-rental-template/wpdemo.redq.io/sites/dress-rental/html/index.html`;
  }
}

function setMsg(id, text, ok = false) {
  const el = document.getElementById(id);
  if (!el) return;
  el.textContent = text;
  el.style.color = ok ? "green" : "red";
}

async function postJson(url, body) {
  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(data?.message || "Request failed");
  return data;
}

document.addEventListener("DOMContentLoaded", () => {
  // LOGIN
  const loginForm = document.getElementById("loginForm");
  if (loginForm) {
    loginForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      setMsg("loginMsg", "");
      const fd = new FormData(loginForm);

      try {
        const data = await postJson(`${API_BASE}/api/auth/login`, {
          email: fd.get("email"),
          password: fd.get("password"),
        });

        if (data?.token) localStorage.setItem("token", data.token);
        if (data?.role) localStorage.setItem("role", data.role);
        if (data?.fullName) localStorage.setItem("fullName", data.fullName);
        if (data?.userId != null) localStorage.setItem("userId", data.userId);
        if (data?.providerId != null) localStorage.setItem("providerId", data.providerId);
        else if (data?.userName) localStorage.setItem("fullName", data.userName);
        else if (data?.name) localStorage.setItem("fullName", data.name);

        // Sử dụng role để xây dựng redirect URL (không phụ thuộc vào backend)
        const redirectUrl = mapRoleToRedirect(data.role);
        window.location.href = redirectUrl;
      } catch (err) {
        setMsg("loginMsg", err?.message || "Đăng nhập thất bại");
      }
    });
  }

  // REGISTER
  const registerForm = document.getElementById("registerForm");
  if (registerForm) {
    registerForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      setMsg("registerMsg", "");
      const fd = new FormData(registerForm);

      try {
        const data = await postJson(`${API_BASE}/api/auth/register`, {
          fullName: fd.get("fullName"),
          phone: fd.get("phone"),
          email: fd.get("email"),
          password: fd.get("password"),
          role: fd.get("role"), // customer/provider
        });

        if (data?.token) localStorage.setItem("token", data.token);
        if (data?.role) localStorage.setItem("role", data.role);
        if (data?.fullName) localStorage.setItem("fullName", data.fullName);
        if (data?.userId != null) localStorage.setItem("userId", data.userId);
        if (data?.providerId != null) localStorage.setItem("providerId", data.providerId);
        else if (data?.userName) localStorage.setItem("fullName", data.userName);
        else if (data?.name) localStorage.setItem("fullName", data.name);

        // Sử dụng role để xây dựng redirect URL (không phụ thuộc vào backend)
        const redirectUrl = mapRoleToRedirect(data.role);
        window.location.href = redirectUrl;
      } catch (err) {
        setMsg("registerMsg", err?.message || "Đăng ký thất bại");
      }
    });
  }
});
