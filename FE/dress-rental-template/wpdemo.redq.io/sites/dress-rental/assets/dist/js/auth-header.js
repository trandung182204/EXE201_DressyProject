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

/**
 * Render header cart dropdown from localStorage.cartItems
 */
window.renderHeaderCart = function () {
  var listEl = document.getElementById('header-cart-items');
  var countEl = document.getElementById('header-cart-count');
  var subtotalEl = document.getElementById('header-cart-subtotal');
  var subtotalValEl = document.getElementById('header-subtotal-val');

  if (!listEl) return false;

  var raw = localStorage.getItem('cartItems');
  var items = [];
  try { items = JSON.parse(raw) || []; } catch (e) { }

  if (items.length === 0) {
    var single = localStorage.getItem('currentBooking');
    if (single) { try { items = [JSON.parse(single)]; } catch (e) { } }
  }

  // Badge
  if (countEl) {
    if (items.length > 0) { countEl.textContent = items.length; countEl.style.display = 'inline'; }
    else { countEl.style.display = 'none'; }
  }

  if (items.length === 0) {
    listEl.innerHTML = '<li><div class="wrapper"><p style="text-align:center;color:#999;">Giỏ hàng trống</p></div></li>';
    if (subtotalEl) subtotalEl.style.display = 'none';
    return true;
  }

  if (subtotalEl) subtotalEl.style.display = 'flex';

  function _fm(v) { return v ? new Intl.NumberFormat('vi-VN').format(v) + ' ₫' : '0 ₫'; }
  function _fd(s) {
    if (!s) return '--';
    var d = new Date(s);
    if (isNaN(d.getTime())) return '--';
    return String(d.getDate()).padStart(2, '0') + '/' + String(d.getMonth() + 1).padStart(2, '0');
  }
  function _img(u) {
    if (!u) return '../assets/dist/img/product-details/1.jpg';
    if (u.startsWith('http')) return u;
    var loc = location.hostname === 'localhost' || location.hostname === '127.0.0.1';
    return (loc ? 'http://localhost:5135' : '') + u;
  }

  var subtotal = 0;

  listEl.innerHTML = items.map(function (item, i) {
    var q = item.quantity || 1;
    var per = _fd(item.startDate) + ' - ' + _fd(item.endDate);
    var cls = i === items.length - 1 ? ' class="last"' : '';

    // Add to subtotal
    var itemTotal = (item.pricePerDay || 0) * (item.days || 1) * q;
    subtotal += itemTotal;

    var colorStr = item.color || '';
    var sizeStr = item.size || '';
    var variantStr = [colorStr, sizeStr].filter(Boolean).join(' - ');

    return '<li' + cls + '>' +
      '<div class="wrapper" style="display:flex;gap:10px;align-items:flex-start;">' +
      '<a href="booking.html?id=' + item.productId + '" class="wrapper-img" style="flex-shrink:0;width:60px;height:60px;">' +
      '<img src="' + _img(item.thumbnailUrl) + '" alt="" style="width:60px;height:60px;object-fit:cover;border-radius:4px;" />' +
      '</a>' +
      '<div style="flex:1;min-width:0;">' +
      '<a href="booking.html?id=' + item.productId + '" class="product-title" style="font-weight:600;display:block;margin-bottom:3px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">' + (item.productName || 'Sản phẩm') + '</a>' +
      (variantStr ? '<p style="margin:2px 0;font-size:12px;color:#999;">Loại: ' + variantStr + '</p>' : '') +
      '<p style="margin:2px 0;font-size:12px;color:#666;">SL: ' + q + ' &middot; ' + _fm(item.pricePerDay) + '/ngày</p>' +
      (item.depositAmount ? '<p style="margin:2px 0;font-size:12px;color:#666;">Cọc: ' + _fm(item.depositAmount) + '</p>' : '') +
      '<p style="margin:2px 0;font-size:12px;color:#999;"><i class="fa fa-calendar-o"></i> ' + per + '</p>' +
      '</div>' +
      '</div>' +
      '</li>';
  }).join('');

  if (subtotalValEl) subtotalValEl.textContent = _fm(subtotal);

  return true;
};

// chạy ngay khi DOM ready
document.addEventListener("DOMContentLoaded", () => {
  var authDone = renderAuthHeader();
  var cartDone = window.renderHeaderCart();

  if (authDone && cartDone) return;

  // nếu header được inject sau đó -> observer để bắt đúng thời điểm
  const obs = new MutationObserver(() => {
    var a = renderAuthHeader();
    var c = window.renderHeaderCart();
    if (a && c) obs.disconnect();
  });
  obs.observe(document.documentElement, { childList: true, subtree: true });
});

