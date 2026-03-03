// Minimal auth-header.js for Admin dashboard
// Exposes loadAuthToHeader() which populates header user info and binds logout
async function loadAuthToHeader() {
  try {
    const auth = window.Auth;
    if (!auth) return;

    const usernameEl = document.getElementById('auth-username');
    const roleEl = document.getElementById('auth-role');
    const logoutBtn = document.getElementById('btn-logout');

    const user = auth.getUser();
    if (usernameEl) usernameEl.textContent = user?.email || user?.userId || 'Admin';
    if (roleEl) roleEl.textContent = (user?.role || '').toString();

    if (logoutBtn) {
      logoutBtn.addEventListener('click', function (e) {
        e.preventDefault();
        auth.logout();
      });
    }
  } catch (e) {
    console.error('[AUTH-HEADER] loadAuthToHeader error:', e);
  }
}

window.loadAuthToHeader = loadAuthToHeader;
