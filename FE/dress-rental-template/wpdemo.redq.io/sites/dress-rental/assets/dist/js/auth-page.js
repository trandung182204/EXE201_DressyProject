/**
 * Auth Page - Login & Register Handling
 * Handles authentication and redirects based on user role
 * 
 * PRODUCTION: xungxinh.io.vn - all pages in same folder, use relative paths
 * LOCAL: localhost - need full paths for development folder structure
 */

// Detect environment by checking hostname
const hostname = location.hostname;
const isLocal = hostname === "localhost" || hostname === "127.0.0.1";

// API_BASE: empty for production (nginx proxy), localhost:5135 for local
const API_BASE = isLocal ? "http://localhost:5135" : "";

// Debug logs - check browser console to verify which environment is detected
console.log("=== AUTH-PAGE LOADED ===");
console.log("Hostname:", hostname);
console.log("isLocal:", isLocal);
console.log("API_BASE:", API_BASE);

/**
 * Get redirect URL after login/register
 * Production: just use "index.html" (all files in same /html folder)
 * Local: use full path to navigate between folders
 */
function mapRoleToRedirect(role) {
  const roleLower = (role || "customer").toLowerCase().trim();

  // PRODUCTION
  // PRODUCTION
  if (!isLocal) {
    switch (roleLower) {
      case "admin":
        return "/Admin/";
      case "provider":
        return "/Manager/";
      default:
        return "/index.html";
    }
  }


  // LOCAL (giữ nguyên code của bạn)
  const path = location.pathname;
  const marker = "dress-rental-template";
  const idx = path.indexOf(marker);
  const basePath = idx > 0 ? path.substring(0, idx) : "/";

  switch (roleLower) {
    case "admin":
      return `${basePath}Admin/admin-dashboard/index.html`;
    case "provider":
      return `${basePath}Manager/ExeManager/nta0309-ecommerce-admin-dashboard.netlify.app/index.html`;
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
  console.log("POST to:", url);
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
  console.log("DOM loaded, setting up forms...");

  // LOGIN
  const loginForm = document.getElementById("loginForm");
  if (loginForm) {
    console.log("Login form found");
    loginForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      setMsg("loginMsg", "");
      const fd = new FormData(loginForm);

      try {
        const data = await postJson(`${API_BASE}/api/auth/login`, {
          email: fd.get("email"),
          password: fd.get("password"),
        });

        console.log("Login response:", data);

        if (data?.token) localStorage.setItem("token", data.token);
        if (data?.role) localStorage.setItem("role", data.role);
        if (data?.fullName) localStorage.setItem("fullName", data.fullName);
        if (data?.userId != null) localStorage.setItem("userId", data.userId);
        if (data?.providerId != null) localStorage.setItem("providerId", data.providerId);
        else if (data?.userName) localStorage.setItem("fullName", data.userName);
        else if (data?.name) localStorage.setItem("fullName", data.name);

        // SYNC CART
        if (data?.token && (data?.role || "customer").toLowerCase().trim() === "customer") {
          try {
            await syncCartToServer(data.token);
          } catch (syncErr) {
            console.warn("Cart sync failed:", syncErr);
          }
        }

        const redirectUrl = mapRoleToRedirect(data.role);
        console.log("Redirecting to:", redirectUrl);
        window.location.href = redirectUrl;
      } catch (err) {
        console.error("Login error:", err);
        const text = err?.message || "Đăng nhập thất bại";
        if (window && typeof window.showToast === 'function') {
          window.showToast(text, 'error');
        } else {
          setMsg("loginMsg", text);
        }
      }
    });
  }

  // REGISTER
  const registerForm = document.getElementById("registerForm");
  if (registerForm) {
    console.log("Register form found");
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
          role: fd.get("role"),
        });

        console.log("Register response:", data);

        if (data?.token) localStorage.setItem("token", data.token);
        if (data?.role) localStorage.setItem("role", data.role);
        if (data?.fullName) localStorage.setItem("fullName", data.fullName);
        if (data?.userId != null) localStorage.setItem("userId", data.userId);
        if (data?.providerId != null) localStorage.setItem("providerId", data.providerId);
        else if (data?.userName) localStorage.setItem("fullName", data.userName);
        else if (data?.name) localStorage.setItem("fullName", data.name);

        const redirectUrl = mapRoleToRedirect(data.role);
        console.log("Redirecting to:", redirectUrl);
        window.location.href = redirectUrl;
      } catch (err) {
        console.error("Register error:", err);
        const text = err?.message || "Đăng ký thất bại";
        if (window && typeof window.showToast === 'function') {
          window.showToast(text, 'error');
        } else {
          setMsg("registerMsg", text);
        }
      }
    });
  }
});

/**
 * Syncs the local guest cart to the server after successful login
 */
async function syncCartToServer(token) {
  if (!token) return;

  // 1. Push guest items to server
  const rawItems = localStorage.getItem("cartItems");
  if (rawItems) {
    try {
      const items = JSON.parse(rawItems);
      if (Array.isArray(items) && items.length > 0) {
        console.log(`[AUTH-PAGE] Syncing ${items.length} guest cart items to server...`);

        for (const item of items) {
          if (!item.variantId) continue;

          const startDateOnly = item.startDate ? item.startDate.split('T')[0] : null;
          const endDateOnly = item.endDate ? item.endDate.split('T')[0] : null;

          try {
            const res = await fetch(`${API_BASE}/api/Carts/me/items`, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
              },
              body: JSON.stringify({
                productVariantId: item.variantId,
                quantity: item.quantity || 1,
                startDate: startDateOnly,
                endDate: endDateOnly
              })
            });
            if (!res.ok) console.warn('[AUTH-PAGE] Sync item failed', await res.text());
          } catch (err) {
            console.warn('[AUTH-PAGE] Sync item fetch error:', err);
          }
        }
        console.log('[AUTH-PAGE] Guest cart sync push complete');
      }
    } catch (e) {
      console.error('[AUTH-PAGE] Error parsing cartItems for sync:', e);
    }
  }

  // 2. Fetch the final merged cart from server and save to localStorage 
  // This ensures the header cart dropdown is instantly populated upon redirect
  try {
    console.log('[AUTH-PAGE] Fetching final server cart to populate header...');
    const res = await fetch(`${API_BASE}/api/Carts/me`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      }
    });

    if (res.ok) {
      const data = await res.json();
      const detail = data.data || data;

      if (detail && detail.items && detail.items.length > 0) {
        const finalItems = detail.items.map(item => ({
          productId: item.productId,
          variantId: item.productVariantId,
          productName: item.productName,
          color: item.colorName || 'N/A',
          size: item.sizeLabel || 'N/A',
          startDate: item.startDate ? item.startDate + 'T00:00:00' : null,
          endDate: item.endDate ? item.endDate + 'T00:00:00' : null,
          days: item.rentalDays || 0,
          quantity: item.quantity || 1,
          pricePerDay: item.pricePerDay || 0,
          depositAmount: item.depositAmount || 0,
          totalPrice: item.totalPrice || 0,
          thumbnailUrl: item.imageUrl || '',
          cartItemId: item.cartItemId,
        }));
        localStorage.setItem("cartItems", JSON.stringify(finalItems));
        console.log(`[AUTH-PAGE] Downloaded ${finalItems.length} items to localStorage`);
      } else {
        localStorage.removeItem("cartItems"); // Empty cart
      }
    } else {
      // Fallback to empty if server fails to ensure clean state
      localStorage.removeItem("cartItems");
    }
  } catch (fetchErr) {
    console.warn('[AUTH-PAGE] Failed to pull auth cart state:', fetchErr);
  }
}
