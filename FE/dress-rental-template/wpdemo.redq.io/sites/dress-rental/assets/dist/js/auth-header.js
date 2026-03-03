/**
 * Lấy đường dẫn redirect sau khi logout
 */
function getLogoutRedirect() {
  return "index.html";
}

/**
 * Hàm xử lý Đăng xuất
 */
function processLogout() {
  try {
    // Giữ lại cartItems:user:${uid} để khi đăng nhập lại vẫn còn giỏ hàng
    // Key này được phân tách theo userId nên user khác sẽ không thấy
    localStorage.removeItem('cartItems:anon');
    localStorage.removeItem('cartItems');
  } catch (e) { console.warn('[AUTH-HEADER] Lỗi xóa giỏ hàng', e); }

  localStorage.removeItem("token");
  localStorage.removeItem("role");
  localStorage.removeItem("fullName");
  localStorage.removeItem("userId");
  localStorage.removeItem("providerId");
  localStorage.removeItem("currentBooking");
  window.location.href = getLogoutRedirect();
}

/**
 * Cập nhật giao diện Đăng nhập/Đăng xuất
 */
function renderAuthHeader() {
  const label = document.getElementById("auth-label");
  const title = document.getElementById("auth-title");
  const list = document.getElementById("auth-list");

  if (!label || !title || !list) return false;

  const token = localStorage.getItem("token");
  const fullName = localStorage.getItem("fullName");

  if (!token || !fullName) return true; // Chưa login

  // Đã login -> Cập nhật tên và chèn nút Đăng xuất
  label.textContent = fullName;
  title.textContent = fullName;
  list.innerHTML = `<li><a href="#" id="btn-logout">Đăng xuất</a></li>`;

  return true;
}

/**
 * Render giỏ hàng từ LocalStorage
 */
window.renderHeaderCart = function () {
  var listEl = document.getElementById('header-cart-items');
  var countEl = document.getElementById('header-cart-count');
  var subtotalEl = document.getElementById('header-cart-subtotal');
  var subtotalValEl = document.getElementById('header-subtotal-val');

  if (!listEl) return false;

  function readHeaderCart() {
    const uid = localStorage.getItem('userId');
    const keys = uid ? [`cartItems:user:${uid}`, 'cartItems:anon', 'cartItems'] : ['cartItems:anon', 'cartItems'];
    for (const k of keys) {
      const r = localStorage.getItem(k);
      if (!r) continue;
      try { const parsed = JSON.parse(r); if (Array.isArray(parsed)) return parsed; } catch (e) { }
    }
    return [];
  }
  var items = readHeaderCart();

  if (items.length === 0) {
    var single = localStorage.getItem('currentBooking');
    if (single) { try { items = [JSON.parse(single)]; } catch (e) { } }
  }

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
    subtotal += (item.pricePerDay || 0) * (item.days || 1) * q;
    var variantStr = [item.color || '', item.size || ''].filter(Boolean).join(' - ');

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

// ==========================================
// CƠ CHẾ BẮT CLICK TỔNG LỰC (KHÔNG BAO GIỜ TRƯỢT)
// ==========================================
if (!window.__globalClickBound) {
  window.__globalClickBound = true;

  document.addEventListener('click', function (e) {
    const target = e.target;

    // 1. Nếu click vào nút Đăng xuất
    if (target.closest('#btn-logout')) {
      e.preventDefault();
      processLogout();
      return;
    }

    // 2. Nếu click vào nút mở Menu (Giỏ hàng / Đăng nhập)
    const toggleBtn = target.closest('.rq-shopping-cart-items-list');
    if (toggleBtn) {
      e.preventDefault();
      e.stopPropagation(); // Cấm các thư viện khác can thiệp

      const parent = toggleBtn.parentElement;
      const panel = parent ? parent.querySelector('.rq-shopping-cart-inner-div') : null;

      // Đóng các menu khác
      document.querySelectorAll('.rq-shopping-cart-inner-div.rq-visible').forEach(p => {
        if (p !== panel) p.classList.remove('rq-visible');
      });
      document.querySelectorAll('.rq-shopping-cart-items-list.active').forEach(a => {
        if (a !== toggleBtn) a.classList.remove('active');
      });

      // Bật/tắt menu hiện tại
      if (panel) {
        toggleBtn.classList.toggle('active');
        panel.classList.toggle('rq-visible');
      }
      return;
    }

    // 3. Nếu click vào mở/đóng Search
    if (target.closest('.rq_btn_header_search')) {
      const searchPanel = document.querySelector('.header-search.open-search');
      if (searchPanel) searchPanel.classList.add('open');
      return;
    }
    if (target.closest('.search-close.close')) {
      const searchPanel = document.querySelector('.header-search.open-search');
      if (searchPanel) searchPanel.classList.remove('open');
      return;
    }

    // 4. Bỏ qua nếu click TRONG nội dung của menu đang mở
    if (target.closest('.rq-shopping-cart-inner-div.rq-visible')) {
      return;
    }

    // 5. Nếu click ra khoảng trắng bên ngoài -> Đóng sạch các menu
    document.querySelectorAll('.rq-shopping-cart-inner-div.rq-visible').forEach(p => p.classList.remove('rq-visible'));
    document.querySelectorAll('.rq-shopping-cart-items-list.active').forEach(a => a.classList.remove('active'));

  }, true); // "true" ở đây là chìa khóa: Ép sự kiện này chạy TRƯỚC mọi thứ khác
}

// Khởi chạy việc đổ dữ liệu
document.addEventListener("DOMContentLoaded", () => {
  var authDone = renderAuthHeader();
  var cartDone = window.renderHeaderCart();

  // Chỉ dùng Observer để điền DATA vào giỏ hàng/login, KHÔNG dùng để gắn sự kiện click nữa
  if (!authDone || !cartDone) {
    const obs = new MutationObserver(() => {
      var a = renderAuthHeader();
      var c = window.renderHeaderCart();
      if (a && c) obs.disconnect();
    });
    obs.observe(document.documentElement, { childList: true, subtree: true });
  }
});