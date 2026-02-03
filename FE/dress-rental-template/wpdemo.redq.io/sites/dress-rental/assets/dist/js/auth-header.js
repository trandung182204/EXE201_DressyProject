/**
 * Kiểm tra xem có đang ở môi trường production không
 */
function isProductionEnv() {
  return location.hostname === "xungxinh.io.vn" ||
    location.hostname === "www.xungxinh.io.vn";
}

/**
 * Lấy đường dẫn redirect sau khi logout
 */
function getLogoutRedirect() {
  if (isProductionEnv()) {
    return "/index.html";
  }
  // Local: sử dụng đường dẫn tương đối đến cùng thư mục
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

