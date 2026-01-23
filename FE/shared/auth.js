/**
 * ============================================
 * auth.js - Shared Authentication Utility
 * ============================================
 * Dùng chung cho tất cả FE pages: Customer, Admin, Manager
 * Import từ /FE/shared/auth.js
 */

const Auth = (function () {
    // ============================================
    // (A) CONFIGURATION
    // ============================================

    /**
     * Lấy API Base URL
     * Ưu tiên window.__API_BASE__ nếu được set, fallback dev default
     */
    function getApiBase() {
        if (window.__API_BASE__) {
            return window.__API_BASE__;
        }

        const hostname = window.location.hostname;
        if (hostname === 'localhost' || hostname === '127.0.0.1') {
            return 'http://localhost:5135/api';
        }

        // Production - thay bằng domain thật
        return 'https://api.your-production-domain.com/api';
    }

    /**
     * Mapping role → redirect path (CÙNG ORIGIN)
     */
    const ROLE_REDIRECT_MAP = {
        admin: '/EXE201_DressyProject/FE/Admin/admin-dashboard/index.html',
        provider: '/EXE201_DressyProject/FE/Manager/ExeManager/nta0309-ecommerce-admin-dashboard.netlify.app/index.html',
        customer: '/EXE201_DressyProject/FE/bean-style.mysapo.net/index.html'
    };

    const LOGIN_PATH = '/EXE201_DressyProject/FE/bean-style.mysapo.net/account/login.html';

    // ============================================
    // (B) STORAGE KEYS
    // ============================================
    const STORAGE_KEYS = {
        token: 'auth_token',
        userId: 'auth_userId',
        email: 'auth_email',
        role: 'auth_role',
        providerId: 'auth_providerId',
        name: 'auth_name'
    };

    // ============================================
    // (C) CORE AUTH FUNCTIONS
    // ============================================

    /**
     * Lưu thông tin auth vào localStorage
     * Nếu đổi userId thì xóa auth_name để buộc load lại
     */
    function save(data) {
        if (!data) return;

        const currentUserId = localStorage.getItem(STORAGE_KEYS.userId);
        const newUserId = data.userId ? String(data.userId) : null;

        // Nếu đổi user thì xóa cached name
        if (newUserId && currentUserId && currentUserId !== newUserId) {
            localStorage.removeItem(STORAGE_KEYS.name);
        }

        if (data.token) localStorage.setItem(STORAGE_KEYS.token, data.token);
        if (data.userId) localStorage.setItem(STORAGE_KEYS.userId, String(data.userId));
        if (data.email) localStorage.setItem(STORAGE_KEYS.email, data.email);
        if (data.role) localStorage.setItem(STORAGE_KEYS.role, data.role.toLowerCase());
        if (data.providerId) localStorage.setItem(STORAGE_KEYS.providerId, String(data.providerId));
        if (data.name || data.fullName) localStorage.setItem(STORAGE_KEYS.name, data.name || data.fullName);
    }

    function getToken() {
        return localStorage.getItem(STORAGE_KEYS.token);
    }

    function getUserId() {
        return localStorage.getItem(STORAGE_KEYS.userId);
    }

    function getRole() {
        return localStorage.getItem(STORAGE_KEYS.role)?.toLowerCase() || null;
    }

    function getEmail() {
        return localStorage.getItem(STORAGE_KEYS.email);
    }

    function getProviderId() {
        return localStorage.getItem(STORAGE_KEYS.providerId);
    }

    function getName() {
        return localStorage.getItem(STORAGE_KEYS.name);
    }

    /**
     * Kiểm tra đã đăng nhập chưa
     */
    function isLoggedIn() {
        const token = getToken();
        const userId = getUserId();
        return !!(token && userId);
    }

    /**
     * Lấy profile từ localStorage
     */
    function getProfile() {
        return {
            name: getName(),
            email: getEmail(),
            role: getRole(),
            userId: getUserId(),
            providerId: getProviderId()
        };
    }

    /**
     * Lấy tất cả user info (alias)
     */
    function getUser() {
        return {
            token: getToken(),
            userId: getUserId(),
            email: getEmail(),
            role: getRole(),
            providerId: getProviderId(),
            name: getName()
        };
    }

    function isAdmin() {
        return getRole() === 'admin';
    }

    function isProvider() {
        return getRole() === 'provider';
    }

    function isCustomer() {
        return getRole() === 'customer';
    }

    function getAuthHeader() {
        const token = getToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    }

    /**
     * Xóa toàn bộ auth data và redirect về login
     */
    function logout(redirectToLogin = true) {
        Object.values(STORAGE_KEYS).forEach(key => {
            localStorage.removeItem(key);
        });
        if (redirectToLogin) {
            window.location.href = LOGIN_PATH;
        }
    }

    /**
     * Xóa auth data mà không redirect
     */
    function clearAuth() {
        Object.values(STORAGE_KEYS).forEach(key => {
            localStorage.removeItem(key);
        });
    }

    // ============================================
    // (D) FETCH WITH AUTH
    // ============================================

    /**
     * Fetch với auto-attach Authorization header + xử lý 401
     */
    async function fetchWithAuth(url, options = {}) {
        const token = getToken();
        const fullUrl = url.startsWith('http') ? url : `${getApiBase()}${url.startsWith('/') ? '' : '/'}${url}`;

        const headers = {
            'Content-Type': 'application/json',
            ...(options.headers || {}),
            ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        };

        try {
            const response = await fetch(fullUrl, {
                ...options,
                headers
            });

            if (response.status === 401) {
                console.warn('[Auth] Token expired or invalid, logging out...');
                logout(true);
                return Promise.reject(new Error('Unauthorized - Session expired'));
            }

            return response;
        } catch (error) {
            console.error('[Auth] Fetch error:', error);
            throw error;
        }
    }

    /**
     * Fetch + auto parse JSON
     */
    async function fetchJson(pathOrUrl, options = {}) {
        const response = await fetchWithAuth(pathOrUrl, options);

        if (!response.ok) {
            let errorMsg = `HTTP ${response.status}`;
            try {
                const errorData = await response.json();
                errorMsg = errorData.message || errorMsg;
            } catch { }
            throw new Error(errorMsg);
        }

        return response.json();
    }

    // ============================================
    // (E) LOAD PROFILE + CACHE NAME
    // ============================================

    /**
     * Load profile từ API nếu chưa có cached name
     * Return profile object hoặc null nếu fail
     */
    async function loadProfile() {
        if (!isLoggedIn()) {
            return null;
        }

        // Nếu đã có cached name → return ngay
        const cachedName = getName();
        if (cachedName) {
            return getProfile();
        }

        // Gọi API để lấy profile
        try {
            const data = await fetchJson('/auth/me');

            // Lưu fullName vào localStorage
            const displayName = data.fullName || data.email || getEmail();
            localStorage.setItem(STORAGE_KEYS.name, displayName);

            return getProfile();
        } catch (error) {
            console.error('[Auth] loadProfile error:', error);
            // Nếu 401 đã được xử lý trong fetchJson → user đã bị logout
            // Nếu lỗi khác, fallback về email
            const email = getEmail();
            if (email) {
                localStorage.setItem(STORAGE_KEYS.name, email);
                return getProfile();
            }
            return null;
        }
    }

    // ============================================
    // (F) GLOBAL HEADER UI RENDERER
    // ============================================

    // Lưu reference để unbind listener
    let _logoutHandler = null;

    /**
     * Render header UI dựa trên trạng thái đăng nhập
     */
    function renderHeaderUI(options = {}) {
        const selectors = {
            guest: options.guest || '#auth-guest',
            user: options.user || '#auth-user',
            name: options.name || '#auth-name',
            logout: options.logout || '#auth-logout'
        };

        const guestEl = document.querySelector(selectors.guest);
        const userEl = document.querySelector(selectors.user);
        const nameEl = document.querySelector(selectors.name);
        const logoutEl = document.querySelector(selectors.logout);

        if (isLoggedIn()) {
            // Hide guest, show user
            if (guestEl) guestEl.style.display = 'none';
            if (userEl) userEl.style.display = '';

            // Set name
            if (nameEl) {
                const profile = getProfile();
                nameEl.textContent = profile.name || profile.email || 'User';
            }

            // Bind logout
            if (logoutEl) {
                // Remove old listener
                if (_logoutHandler) {
                    logoutEl.removeEventListener('click', _logoutHandler);
                }
                _logoutHandler = function (e) {
                    e.preventDefault();
                    logout(true);
                };
                logoutEl.addEventListener('click', _logoutHandler);
            }
        } else {
            // Show guest, hide user
            if (guestEl) guestEl.style.display = '';
            if (userEl) userEl.style.display = 'none';
        }
    }

    /**
     * Mount auth UI - gọi 1 lần khi page load
     */
    async function mount(options = {}) {
        await loadProfile();
        renderHeaderUI(options);
    }

    // ============================================
    // (G) GUARD FUNCTIONS
    // ============================================

    function requireAuth() {
        if (!isLoggedIn()) {
            window.location.href = LOGIN_PATH;
            return false;
        }
        return true;
    }

    function requireRole(allowedRoles, redirectUrl) {
        if (!requireAuth()) return false;

        const currentRole = getRole();
        const roles = Array.isArray(allowedRoles) ? allowedRoles : [allowedRoles];
        const normalizedRoles = roles.map(r => r.toLowerCase());

        if (!normalizedRoles.includes(currentRole)) {
            const correctPath = ROLE_REDIRECT_MAP[currentRole] || LOGIN_PATH;
            window.location.href = redirectUrl || correctPath;
            return false;
        }
        return true;
    }

    // ============================================
    // (H) REDIRECT HELPERS
    // ============================================

    function getRedirectPath(role) {
        const normalizedRole = (role || 'customer').toLowerCase();
        return ROLE_REDIRECT_MAP[normalizedRole] || ROLE_REDIRECT_MAP['customer'];
    }

    function redirectToDashboard() {
        const role = getRole() || 'customer';
        window.location.href = getRedirectPath(role);
    }

    // ============================================
    // (I) DEBUG HELPER
    // ============================================

    function debug() {
        console.log('[Auth] Current user:', getUser());
        console.log('[Auth] Is logged in:', isLoggedIn());
        console.log('[Auth] API Base:', getApiBase());
    }

    // ============================================
    // PUBLIC API
    // ============================================
    return {
        // Config
        getApiBase,
        getRedirectPath,
        LOGIN_PATH,
        ROLE_REDIRECT_MAP,

        // Storage
        save,
        getToken,
        getUserId,
        getRole,
        getEmail,
        getProviderId,
        getName,
        getProfile,
        getUser,
        getAuthHeader,

        // Check
        isLoggedIn,
        isAdmin,
        isProvider,
        isCustomer,

        // Guards
        requireAuth,
        requireRole,

        // Actions
        logout,
        clearAuth,
        redirectToDashboard,

        // Fetch
        fetchWithAuth,
        fetchJson,

        // Profile & UI
        loadProfile,
        renderHeaderUI,
        mount,

        // Debug
        debug
    };
})();

// Export cho ES modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Auth;
}

// Auto-attach to window
window.Auth = Auth;

// Debug log khi load
document.addEventListener('DOMContentLoaded', () => {
    if (Auth.isLoggedIn()) {
        console.log('[Auth] User logged in as:', Auth.getRole(), '| UserId:', Auth.getUserId());
    }
});
