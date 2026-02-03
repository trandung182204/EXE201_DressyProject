// SỬA base URL theo BE của bạn:
const isLocal =
  location.hostname === "localhost" ||
  location.hostname === "127.0.0.1";

const API_BASE = isLocal
  ? "http://localhost:5135"
  : "";

/**
 * Kiểm tra xem có đang ở môi trường production không
 */
function isProduction() {
  return location.hostname === "xungxinh.io.vn" ||
    location.hostname === "www.xungxinh.io.vn";
}

/**
 * Tự động detect base path từ URL hiện tại
 * Tìm vị trí của "dress-rental-template" trong path và tính đường dẫn về FE/
 */
function getBasePath() {
  // Nếu đang ở production, trả về root
  if (isProduction()) {
    return "/";
  }

  const path = location.pathname;

  // Tìm vị trí của dress-rental-template trong path
  const marker = "dress-rental-template";
  const idx = path.indexOf(marker);

  if (idx > 0) {
    // Trả về phần path trước "dress-rental-template"
    // Ví dụ: /some/path/dress-rental-template/... → /some/path/
    return path.substring(0, idx);
  }

  // Fallback: giả sử đang ở root
  return "/";
}

/**
 * Xây dựng đường dẫn redirect dựa trên role
 * Tự động detect base path để hoạt động với mọi cấu hình server
 */
function mapRoleToRedirect(role) {
  const roleLower = (role || "customer").toLowerCase().trim();

  // Nếu đang ở production (xungxinh.io.vn)
  if (isProduction()) {
    switch (roleLower) {
      case "admin":
        // TODO: Cập nhật đường dẫn admin cho production nếu cần
        return "/index.html";
      case "provider":
        // TODO: Cập nhật đường dẫn provider cho production nếu cần  
        return "/index.html";
      case "customer":
      default:
        return "/index.html";
    }
  }

  // Local development
  const basePath = getBasePath();

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
