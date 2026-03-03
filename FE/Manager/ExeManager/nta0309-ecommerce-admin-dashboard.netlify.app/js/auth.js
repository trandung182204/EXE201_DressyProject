/**
 * ============================================
 * auth.js - Shared Authentication Utility
 * ============================================
 * Dùng chung cho tất cả FE pages: Customer, Admin, Manager
 * Copy file này vào các thư mục cần thiết hoặc import từ /FE/shared/
 */

const Auth = (function () {
    // ============================================
    // (A) CONFIGURATION - SỬA TẠI ĐÂY
    // ============================================

    const isLocal = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';

    /**
     * Lấy API Base URL dựa trên môi trường
     */
    function getApiBase() {
        return isLocal ? 'http://localhost:5135' : '';
    }

    /**
     * Tự động detect base path từ URL hiện tại
     * Thử nhiều markers để tìm đường dẫn gốc FE/
     */
    function getBasePath() {
        const path = location.pathname;

        // Thử các markers khác nhau theo thứ tự ưu tiên
        const markers = ["Manager", "Admin", "dress-rental-template"];

        for (const marker of markers) {
            const idx = path.indexOf(marker);
            if (idx > 0) {
                return path.substring(0, idx);
            }
        }

        // Fallback: trả về "/"
        return "/";
    }

    function getLoginPath() {
        const basePath = getBasePath();
        return `${basePath}dress-rental-template/wpdemo.redq.io/sites/dress-rental/html/login.html`;
    }

    /**
     * Lấy role redirect map
     */
    function getRoleRedirectMap() {
        const basePath = getBasePath();
        return {
            admin: `${basePath}Admin/admin-dashboard/index.html`,
            provider: `${basePath}Manager/ExeManager/nta0309-ecommerce-admin-dashboard.netlify.app/index.html`,
            customer: `${basePath}dress-rental-template/wpdemo.redq.io/sites/dress-rental/html/index.html`
        };
    }

    // ============================================
    // (B) STORAGE KEYS
    // ============================================
    const STORAGE_KEYS = {
        token: 'auth_token',
        userId: 'auth_userId',
        email: 'auth_email',
        role: 'auth_role',
        providerId: 'auth_providerId'
    };

    // ============================================
    // (C) CORE AUTH FUNCTIONS
    // ============================================

    /**
     * Lưu thông tin auth vào localStorage
     * @param {Object} data - { token, userId, role, email, providerId? }
     */
    function save(data) {
        if (!data) return;

        if (data.token) localStorage.setItem(STORAGE_KEYS.token, data.token);
        if (data.userId) localStorage.setItem(STORAGE_KEYS.userId, String(data.userId));
        if (data.email) localStorage.setItem(STORAGE_KEYS.email, data.email);
        if (data.role) localStorage.setItem(STORAGE_KEYS.role, data.role.toLowerCase());
        if (data.providerId) localStorage.setItem(STORAGE_KEYS.providerId, String(data.providerId));
    }

    /**
     * Lấy token từ localStorage
     */
    function getToken() {
        return localStorage.getItem(STORAGE_KEYS.token);
    }

    /**
     * Lấy userId từ localStorage
     */
    function getUserId() {
        return localStorage.getItem(STORAGE_KEYS.userId);
    }

    /**
     * Lấy role (đã lowercase)
     */
    function getRole() {
        return localStorage.getItem(STORAGE_KEYS.role)?.toLowerCase() || null;
    }

    /**
     * Lấy email
     */
    function getEmail() {
        return localStorage.getItem(STORAGE_KEYS.email);
    }

    /**
     * Lấy providerId (nếu có)
     */
    function getProviderId() {
        return localStorage.getItem(STORAGE_KEYS.providerId);
    }

    /**
     * Lấy tất cả user info
     */
    function getUser() {
        return {
            token: getToken(),
            userId: getUserId(),
            email: getEmail(),
            role: getRole(),
            providerId: getProviderId()
        };
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
     * Kiểm tra role cụ thể
     */
    function isAdmin() {
        return getRole() === 'admin';
    }

    function isProvider() {
        return getRole() === 'provider';
    }

    function isCustomer() {
        return getRole() === 'customer';
    }

    /**
     * Lấy Authorization header
     * @returns {Object} { Authorization: "Bearer xxx" } hoặc {}
     */
    function getAuthHeader() {
        const token = getToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    }

    /**
     * Xóa toàn bộ auth data và redirect về login
     */
    function logout() {
        Object.values(STORAGE_KEYS).forEach(key => {
            localStorage.removeItem(key);
        });
        window.location.href = getLoginPath();
    }

    /**
     * Xóa auth data mà không redirect (silent logout)
     */
    function clearAuth() {
        Object.values(STORAGE_KEYS).forEach(key => {
            localStorage.removeItem(key);
        });
    }

    // ============================================
    // (D) GUARD FUNCTIONS
    // ============================================

    /**
     * Yêu cầu phải đăng nhập, nếu không redirect về login
     * @returns {boolean} true nếu đã đăng nhập
     */
    function requireAuth() {
        if (!isLoggedIn()) {
            window.location.href = getLoginPath();
            return false;
        }
        return true;
    }

    /**
     * Yêu cầu role cụ thể, nếu không đủ quyền redirect về trang phù hợp
     * @param {string|string[]} allowedRoles - role hoặc array roles được phép
     * @param {string} [redirectUrl] - URL redirect nếu không đủ quyền (mặc định về trang của role hiện tại)
     * @returns {boolean} true nếu có quyền
     */
    function requireRole(allowedRoles, redirectUrl) {
        // Phải đăng nhập trước
        if (!requireAuth()) return false;

        const currentRole = getRole();
        const roles = Array.isArray(allowedRoles) ? allowedRoles : [allowedRoles];
        const normalizedRoles = roles.map(r => r.toLowerCase());

        if (!normalizedRoles.includes(currentRole)) {
            // Redirect về trang phù hợp với role của user
            const correctPath = getRoleRedirectMap()[currentRole] || getLoginPath();
            window.location.href = redirectUrl || correctPath;
            return false;
        }
        return true;
    }

    // ============================================
    // (E) REDIRECT HELPERS
    // ============================================

    /**
     * Lấy redirect path dựa trên role
     * @param {string} role 
     * @returns {string} path
     */
    function getRedirectPath(role) {
        const normalizedRole = (role || 'customer').toLowerCase();
        const map = getRoleRedirectMap();
        return map[normalizedRole] || map['customer'];
    }

    /**
     * Redirect về trang dashboard tương ứng với role hiện tại
     */
    function redirectToDashboard() {
        const role = getRole() || 'customer';
        window.location.href = getRedirectPath(role);
    }

    // ============================================
    // (F) FETCH WITH AUTH
    // ============================================

    /**
     * Fetch với auto-attach Authorization header + xử lý 401
     * @param {string} url - URL (có thể relative hoặc absolute)
     * @param {Object} options - fetch options
     * @returns {Promise<Response>}
     */
    async function fetchWithAuth(url, options = {}) {
        const token = getToken();

        // Build full URL nếu cần
        const fullUrl = url.startsWith('http') ? url : `${getApiBase()}${url}`;

        // Merge headers
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

            // Xử lý 401 Unauthorized
            if (response.status === 401) {
                console.warn('[Auth] Token expired or invalid, redirecting to login...');
                clearAuth();
                window.location.href = getLoginPath();
                // Return một rejected promise để caller biết
                return Promise.reject(new Error('Unauthorized - Session expired'));
            }

            return response;
        } catch (error) {
            console.error('[Auth] Fetch error:', error);
            throw error;
        }
    }

    /**
     * Helper: fetchWithAuth + auto parse JSON
     */
    async function fetchJson(url, options = {}) {
        const response = await fetchWithAuth(url, options);
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `HTTP ${response.status}`);
        }
        return response.json();
    }

    // ============================================
    // (G) DEBUG HELPER
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
        getLoginPath,
        getRoleRedirectMap,

        // Storage
        save,
        getToken,
        getUserId,
        getRole,
        getEmail,
        getProviderId,
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

        // Debug
        debug
    };
})();

// Export cho các module khác (nếu dùng ES modules)
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
