// SỬA base URL theo BE của bạn:
const isLocal =
  location.hostname === "localhost" ||
  location.hostname === "127.0.0.1";

const API_BASE = isLocal
  ? "http://localhost:5135/api"
  : "/api";

/**
 * Xây dựng đường dẫn redirect dựa trên role
 * Đường dẫn tương đối từ thư mục html/ hiện tại
 * Khi deploy, chỉ cần cập nhật các đường dẫn này
 */
function mapRoleToRedirect(role) {
  const roleLower = (role || "customer").toLowerCase().trim();

  // Cấu trúc thư mục:
  // FE/dress-rental-template/wpdemo.redq.io/sites/dress-rental/html/login.html
  // FE/Admin/admin-dashboard/index.html
  // FE/Manager/index.html
  // 
  // Từ html/ lên FE/ cần 6 cấp:
  // html → dress-rental → sites → wpdemo.redq.io → dress-rental-template → FE

  switch (roleLower) {
    case "admin":
      // Từ html/ lên 6 cấp về FE/, rồi vào Admin/admin-dashboard/
      return "../../../../../../Admin/admin-dashboard/index.html";
    case "provider":
      // Từ html/ lên 6 cấp về FE/, rồi vào Manager/
      return "../../../../../../Manager/index.html";
    case "customer":
    default:
      // Cùng thư mục html/
      return "index.html";
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
        const data = await postJson(`${API_BASE}/auth/login`, {
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
        const data = await postJson(`${API_BASE}/auth/register`, {
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
