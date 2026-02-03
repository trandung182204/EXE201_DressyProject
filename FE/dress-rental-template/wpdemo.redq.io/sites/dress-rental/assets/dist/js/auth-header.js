/**
 * Kiểm tra xem có đang ở môi trường local không
 */
function isLocalEnv() {
  return location.hostname === "localhost" ||
    location.hostname === "127.0.0.1";
}

/**
 * Lấy đường dẫn redirect sau khi logout
 * Vì tất cả file HTML đều ở cùng thư mục (html/), chỉ cần dùng relative path
 */
function getLogoutRedirect() {
  // Relative path hoạt động cho cả production và local
  // vì logout được gọi từ các trang trong cùng thư mục html/
  console.log("[AUTH-HEADER] Logout redirect to: index.html");
  return "index.html";
}

function renderAuthHeader() {
  const label = document.getElementById("auth-label");
  const title = document.getElementById("auth-title");
  const list = document.getElementById("auth-list");

  // Nếu header chưa vào DOM thì thôi
  if (!label || !title || !list) return false;

  const token = localStorage.getItem("token");
  const fullName = localStorage.getItem("fullName");

  // CHƯA LOGIN -> giữ nguyên HTML cứng trong header.html (ĐĂNG NHẬP + login/register)
  if (!token || !fullName) return true;

  // ĐÃ LOGIN -> đổi sang tên + logout
  label.textContent = fullName;
  title.textContent = fullName;
  list.innerHTML = `<li><a href="javascript:void(0)" id="btn-logout">Đăng xuất</a></li>`;

  document.getElementById("btn-logout")?.addEventListener("click", () => {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    localStorage.removeItem("fullName");
    localStorage.removeItem("userId");
    localStorage.removeItem("providerId");
    window.location.href = getLogoutRedirect();
  });

  return true;
}

// chạy ngay khi DOM ready
document.addEventListener("DOMContentLoaded", () => {
  if (renderAuthHeader()) return;

  // nếu header được inject sau đó -> observer để bắt đúng thời điểm
  const obs = new MutationObserver(() => {
    if (renderAuthHeader()) obs.disconnect();
  });
  obs.observe(document.documentElement, { childList: true, subtree: true });
});

