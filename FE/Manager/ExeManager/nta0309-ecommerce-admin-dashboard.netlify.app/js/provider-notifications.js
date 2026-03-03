(function () {
  const isLocal = location.hostname === "localhost" || location.hostname === "127.0.0.1";
  const API_BASE = isLocal ? "http://localhost:5135" : "";

  function getToken() {
    return localStorage.getItem("token") || "";
  }

  function money(v) {
    return (v ?? 0).toLocaleString("vi-VN") + " VNĐ";
  }

  function timeAgo(date) {
    const d = new Date(date);
    const diff = Date.now() - d.getTime();
    const m = Math.floor(diff / 60000);
    if (m < 1) return "Vừa xong";
    if (m < 60) return `${m} phút trước`;
    const h = Math.floor(m / 60);
    if (h < 24) return `${h} giờ trước`;
    const day = Math.floor(h / 24);
    return `${day} ngày trước`;
  }

  async function apiGet(path) {
    const res = await fetch(API_BASE + path, {
      headers: { "Authorization": "Bearer " + getToken() }
    });
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  }

  async function apiPut(path, body) {
    const res = await fetch(API_BASE + path, {
      method: "PUT",
      headers: {
        "Authorization": "Bearer " + getToken(),
        "Content-Type": "application/json"
      },
      body: JSON.stringify(body ?? {})
    });
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  }

  function setCount(n) {
    const el = document.getElementById("ntf-count");
    if (!el) return;
    if (n > 0) {
      el.style.display = "inline-flex";
      el.textContent = String(n);
    } else {
      el.style.display = "none";
      el.textContent = "0";
    }
  }

  function renderList(items, lastSeenAt) {
    const ul = document.getElementById("ntf-list");
    if (!ul) return;

    ul.innerHTML = "";

    if (!items || items.length === 0) {
      ul.innerHTML = `<li style="padding:12px 14px; opacity:.8;">Chưa có thông báo</li>`;
      return;
    }

    const seenTime = lastSeenAt ? new Date(lastSeenAt).getTime() : 0;

    items.forEach(it => {
      const isNew = it.createdAt && (new Date(it.createdAt).getTime() > seenTime);

      ul.insertAdjacentHTML("beforeend", `
        <li>
          <div class="sherah-paymentm__name">
            <div class="sherah-paymentm__content" style="margin-left:0;">
              <h4 class="sherah-notifications__title">
                Đơn hàng mới <span>#${it.bookingId}</span>
                ${isNew ? `<span style="margin-left:8px; font-size:12px; color:#ff4d4f;">NEW</span>` : ``}
              </h4>
              <p class="sherah-paymentm__text sherah-paymentm__text--notify">
                ${it.customerName || "Unknown"} • ${money(it.totalPrice)} • ${timeAgo(it.createdAt)}
              </p>
              <a href="order-details.html?id=${it.bookingId}" style="font-size:12px; text-decoration:underline;">
                Xem chi tiết
              </a>
            </div>
          </div>
        </li>
      `);
    });
  }

  async function refreshNotifications() {
    // unread-count
    const c = await apiGet("/api/provider/notifications/unread-count");
    setCount(c.data ?? 0);

    // list
    const list = await apiGet("/api/provider/notifications/orders?limit=5");
    const lastSeenAt = list.data?.lastSeenAt || null;
    renderList(list.data?.items || [], lastSeenAt);
  }

  async function markSeenNow() {
    await apiPut("/api/provider/notifications/seen", {}); // server set NOW()
    setCount(0);
    // refresh list to remove NEW label
    await refreshNotifications();
  }

  function bindBellOpenMarkSeen() {
    // Trong template của bạn, icon chuông nằm trong .sherah-header__dropmenu
    const bellWrap = document.querySelector(".sherah-header__dropmenu");
    if (!bellWrap) return;

    if (bellWrap.dataset.boundSeen === "1") return;
    bellWrap.dataset.boundSeen = "1";

    bellWrap.addEventListener("click", async () => {
      // khi user bấm mở dropdown -> mark seen
      try { await markSeenNow(); } catch (e) { console.error(e); }
    });
  }

  // Public function để bạn gọi sau khi inject header.html xong
  window.initProviderNotifications = async function initProviderNotifications() {
    try {
      bindBellOpenMarkSeen();
      await refreshNotifications();

      // polling mỗi 25s
      setInterval(() => {
        refreshNotifications().catch(console.error);
      }, 25000);
    } catch (e) {
      console.error("initProviderNotifications error:", e);
    }
  };
})();