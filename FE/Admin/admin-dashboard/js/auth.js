/**
 * ============================================
 * auth.js - Admin Dashboard Authentication
 * ============================================
 * Copy của FE/shared/auth.js - dùng cho Admin
 */

const Auth = (function () {
    const isLocal = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';

    function getApiBase() {
        return isLocal ? 'http://localhost:5135' : '';
    }

    /**
     * Tự động detect base path từ URL hiện tại
     */
    function getBasePath() {
        const path = location.pathname;

        // Tìm vị trí của "Admin" trong path
        const marker = "Admin";
        const idx = path.indexOf(marker);

        if (idx > 0) {
            return path.substring(0, idx);
        }
        return "/";
    }

    function getLoginPath() {
        const basePath = getBasePath();
        return `${basePath}dress-rental-template/wpdemo.redq.io/sites/dress-rental/html/login.html`;
    }

    function getRoleRedirectMap() {
        const basePath = getBasePath();
        return {
            admin: `${basePath}Admin/admin-dashboard/index.html`,
            provider: `${basePath}Manager/ExeManager/nta0309-ecommerce-admin-dashboard.netlify.app/index.html`,
            customer: `${basePath}dress-rental-template/wpdemo.redq.io/sites/dress-rental/html/index.html`
        };
    }

    const STORAGE_KEYS = {
        token: 'auth_token',
        userId: 'auth_userId',
        email: 'auth_email',
        role: 'auth_role',
        providerId: 'auth_providerId'
    };

    function save(data) {
        if (!data) return;
        if (data.token) localStorage.setItem(STORAGE_KEYS.token, data.token);
        if (data.userId) localStorage.setItem(STORAGE_KEYS.userId, String(data.userId));
        if (data.email) localStorage.setItem(STORAGE_KEYS.email, data.email);
        if (data.role) localStorage.setItem(STORAGE_KEYS.role, data.role.toLowerCase());
        if (data.providerId) localStorage.setItem(STORAGE_KEYS.providerId, String(data.providerId));
    }

    function getToken() { return localStorage.getItem(STORAGE_KEYS.token); }
    function getUserId() { return localStorage.getItem(STORAGE_KEYS.userId); }
    function getRole() { return localStorage.getItem(STORAGE_KEYS.role)?.toLowerCase() || null; }
    function getEmail() { return localStorage.getItem(STORAGE_KEYS.email); }
    function getProviderId() { return localStorage.getItem(STORAGE_KEYS.providerId); }

    function getUser() {
        return {
            token: getToken(),
            userId: getUserId(),
            email: getEmail(),
            role: getRole(),
            providerId: getProviderId()
        };
    }

    function isLoggedIn() {
        return !!(getToken() && getUserId());
    }

    function isAdmin() { return getRole() === 'admin'; }
    function isProvider() { return getRole() === 'provider'; }
    function isCustomer() { return getRole() === 'customer'; }

    function getAuthHeader() {
        const token = getToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    }

    function logout() {
        Object.values(STORAGE_KEYS).forEach(key => localStorage.removeItem(key));
        window.location.href = getLoginPath();
    }

    function clearAuth() {
        Object.values(STORAGE_KEYS).forEach(key => localStorage.removeItem(key));
    }

    function requireAuth() {
        if (!isLoggedIn()) {
            window.location.href = getLoginPath();
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
            const correctPath = getRoleRedirectMap()[currentRole] || getLoginPath();
            window.location.href = redirectUrl || correctPath;
            return false;
        }
        return true;
    }

    function getRedirectPath(role) {
        const normalizedRole = (role || 'customer').toLowerCase();
        const map = getRoleRedirectMap();
        return map[normalizedRole] || map['customer'];
    }

    function redirectToDashboard() {
        const role = getRole() || 'customer';
        window.location.href = getRedirectPath(role);
    }

    async function fetchWithAuth(url, options = {}) {
        const token = getToken();
        const fullUrl = url.startsWith('http') ? url : `${getApiBase()}${url}`;
        const headers = {
            'Content-Type': 'application/json',
            ...(options.headers || {}),
            ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        };

        try {
            const response = await fetch(fullUrl, { ...options, headers });
            if (response.status === 401) {
                console.warn('[Auth] Token expired, redirecting to login...');
                clearAuth();
                window.location.href = getLoginPath();
                return Promise.reject(new Error('Unauthorized'));
            }
            return response;
        } catch (error) {
            console.error('[Auth] Fetch error:', error);
            throw error;
        }
    }

    async function fetchJson(url, options = {}) {
        const response = await fetchWithAuth(url, options);
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `HTTP ${response.status}`);
        }
        return response.json();
    }

    return {
        getApiBase, getRedirectPath, getLoginPath, getRoleRedirectMap,
        save, getToken, getUserId, getRole, getEmail, getProviderId, getUser, getAuthHeader,
        isLoggedIn, isAdmin, isProvider, isCustomer,
        requireAuth, requireRole,
        logout, clearAuth, redirectToDashboard,
        fetchWithAuth, fetchJson
    };
})();

window.Auth = Auth;

// Auto check auth on page load for admin pages
document.addEventListener('DOMContentLoaded', () => {
    if (Auth.isLoggedIn()) {
        console.log('[Auth] Admin user:', Auth.getRole(), '| UserId:', Auth.getUserId());
    }
});
