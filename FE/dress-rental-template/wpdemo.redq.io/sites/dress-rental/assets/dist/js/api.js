/**
 * api.js - Shared API Utility for Dress Rental Website
 * =====================================================
 * Environment detection, fetch wrapper, toast notifications, loading overlay.
 * Load this file BEFORE any page-specific JS (cart.js, checkout.js, etc.)
 */

// ============================================================
// 1. ENVIRONMENT DETECTION
// ============================================================
const hostname = location.hostname;
const isLocal = hostname === "localhost" || hostname === "127.0.0.1";

// Production: empty string → nginx proxies /api/* to backend
// Local: full localhost URL for CORS dev
const API_BASE = isLocal ? "http://localhost:5135" : "";

console.log("=== API.JS LOADED ===");
console.log("Hostname:", hostname);
console.log("isLocal:", isLocal);
console.log("API_BASE:", API_BASE);

// ============================================================
// 2. AUTH HELPERS
// ============================================================

/** Get the stored JWT token (if any) */
function getToken() {
    return localStorage.getItem("token") || "";
}

/** Check if user is currently logged in */
function isLoggedIn() {
    return !!getToken();
}

// ============================================================
// 3. LOADING OVERLAY
// ============================================================

/** Create the loading overlay element (once) */
function _ensureOverlay() {
    let overlay = document.getElementById("api-loading-overlay");
    if (!overlay) {
        overlay = document.createElement("div");
        overlay.id = "api-loading-overlay";
        overlay.innerHTML = `
      <div style="display:flex;align-items:center;justify-content:center;height:100%;">
        <div style="text-align:center;color:#fff;">
          <div class="api-spinner"></div>
          <p style="margin-top:12px;font-size:15px;">Đang xử lý...</p>
        </div>
      </div>`;
        overlay.style.cssText =
            "position:fixed;inset:0;background:rgba(0,0,0,0.45);z-index:99999;display:none;";
        document.body.appendChild(overlay);

        // Spinner CSS (injected once)
        if (!document.getElementById("api-spinner-style")) {
            const style = document.createElement("style");
            style.id = "api-spinner-style";
            style.textContent = `
        .api-spinner {
          width:40px;height:40px;margin:0 auto;
          border:4px solid rgba(255,255,255,0.3);
          border-top-color:#fff;border-radius:50%;
          animation:api-spin .7s linear infinite;
        }
        @keyframes api-spin { to { transform:rotate(360deg); } }
      `;
            document.head.appendChild(style);
        }
    }
    return overlay;
}

/** Show loading overlay */
function showLoading() {
    const o = _ensureOverlay();
    o.style.display = "block";
}

/** Hide loading overlay */
function hideLoading() {
    const o = document.getElementById("api-loading-overlay");
    if (o) o.style.display = "none";
}

// ============================================================
// 4. TOAST NOTIFICATIONS
// ============================================================

/** Inject toast container + CSS (once) */
function _ensureToastContainer() {
    let container = document.getElementById("api-toast-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "api-toast-container";
        container.style.cssText =
            "position:fixed;top:24px;right:24px;z-index:100000;display:flex;flex-direction:column;gap:10px;max-width:400px;";
        document.body.appendChild(container);

        // Toast CSS
        if (!document.getElementById("api-toast-style")) {
            const style = document.createElement("style");
            style.id = "api-toast-style";
            style.textContent = `
        .api-toast {
          padding:14px 20px;border-radius:8px;color:#fff;font-size:14px;
          box-shadow:0 4px 20px rgba(0,0,0,0.25);
          display:flex;align-items:center;gap:10px;
          animation:api-toast-in .35s ease;
          transition:opacity .3s,transform .3s;
          font-family:'Open Sans','Roboto',sans-serif;
        }
        .api-toast.success { background:linear-gradient(135deg,#28a745,#20c997); }
        .api-toast.error   { background:linear-gradient(135deg,#dc3545,#e74c3c); }
        .api-toast.info    { background:linear-gradient(135deg,#007bff,#6610f2); }
        .api-toast.hide    { opacity:0;transform:translateX(60px); }
        @keyframes api-toast-in { from{opacity:0;transform:translateX(60px);} to{opacity:1;transform:translateX(0);} }
        .api-toast i { font-size:18px; }
      `;
            document.head.appendChild(style);
        }
    }
    return container;
}

/**
 * Show a toast notification.
 * @param {string} message - The message to display
 * @param {"success"|"error"|"info"} type - Toast type
 * @param {number} duration - Auto-dismiss in ms (default 3500)
 */
function showToast(message, type = "info", duration = 3500) {
    const container = _ensureToastContainer();
    const toast = document.createElement("div");
    toast.className = `api-toast ${type}`;

    const icons = {
        success: "fa-check-circle",
        error: "fa-times-circle",
        info: "fa-info-circle",
    };
    toast.innerHTML = `<i class="fa ${icons[type] || icons.info}"></i><span>${message}</span>`;
    container.appendChild(toast);

    // Auto dismiss
    setTimeout(() => {
        toast.classList.add("hide");
        setTimeout(() => toast.remove(), 350);
    }, duration);
}

// ============================================================
// 5. API FETCH WRAPPER
// ============================================================

/**
 * Generic API call with loading indicator, auth header, and error handling.
 * @param {"GET"|"POST"|"PUT"|"DELETE"} method
 * @param {string} path - e.g. "/api/Orders"
 * @param {object|null} body - JSON body (for POST/PUT)
 * @param {boolean} withLoading - Show loading overlay (default true)
 * @returns {Promise<any>} Parsed JSON response
 */
async function apiFetch(method, path, body = null, withLoading = true) {
    const url = `${API_BASE}${path}`;
    console.log(`[API] ${method} ${url}`, body || "");

    if (withLoading) showLoading();

    try {
        const headers = { "Content-Type": "application/json" };
        const token = getToken();
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const options = { method, headers };
        if (body && (method === "POST" || method === "PUT")) {
            options.body = JSON.stringify(body);
        }

        const res = await fetch(url, options);
        const data = await res.json().catch(() => ({}));

        if (!res.ok) {
            const msg = data?.message || data?.title || `Lỗi ${res.status}`;
            throw new Error(msg);
        }
        return data;
    } finally {
        if (withLoading) hideLoading();
    }
}

// ============================================================
// 6. CURRENCY FORMATTER
// ============================================================

/**
 * Format a number as Vietnamese Dong (₫)
 * @param {number} value
 * @returns {string}
 */
function formatMoney(value) {
    if (value == null || isNaN(value)) return "0 ₫";
    return new Intl.NumberFormat("vi-VN").format(value) + " ₫";
}

/**
 * Build full image URL (handles relative backend paths)
 * @param {string} url
 * @returns {string}
 */
function buildImageUrl(url) {
    if (!url) return "../assets/dist/img/cart/1.jpg";
    if (url.startsWith("http")) return url;
    return API_BASE + url;
}
