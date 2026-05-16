/* ─────────────────────────────────────────────────────
   FILMIX — Auth State
   Reads the current user from localStorage and wires up
   the navbar profile chip, welcome bar, and CTA hiding.
   Depends on i18n.js being loaded first.
   ───────────────────────────────────────────────────── */

function getUser() {
  const raw = localStorage.getItem('filmix_user');
  return raw ? JSON.parse(raw) : null;
}

function logout() {
  localStorage.removeItem('filmix_user');
  location.replace('/');
}

/* Watchlist helpers */
function getWatchlist() {
  return JSON.parse(localStorage.getItem('filmix_watchlist') || '[]');
}

function isInWatchlist(id) {
  return getWatchlist().some(m => m.id === id);
}

function toggleWatchlist(item) {
  const list = getWatchlist();
  const idx  = list.findIndex(m => m.id === item.id);
  if (idx === -1) {
    list.push(item);
    localStorage.setItem('filmix_watchlist', JSON.stringify(list));
    return true;  /* added */
  } else {
    list.splice(idx, 1);
    localStorage.setItem('filmix_watchlist', JSON.stringify(list));
    return false; /* removed */
  }
}

/* Apply nav + welcome bar state for the current user */
function initAuthUI() {
  const user = getUser();

  const signinEl  = document.getElementById('navSignin');
  const profileEl = document.getElementById('navProfile');

  if (!user) {
    if (signinEl)  signinEl.style.display  = '';
    if (profileEl) profileEl.style.display = 'none';
    return;
  }

  const displayName = user.name || user.email.split('@')[0];
  const initial     = displayName.charAt(0).toUpperCase();

  /* nav chip */
  if (signinEl)  signinEl.style.display  = 'none';
  if (profileEl) {
    const avatarEl = document.getElementById('navAvatar');
    const usernameEl = document.getElementById('navUsername');
    if(avatarEl) avatarEl.textContent = initial;
    if(usernameEl) usernameEl.textContent = displayName;
    profileEl.style.display = 'flex';
  }

  /* welcome bar */
  const welcomeBar   = document.getElementById('welcomeBar');
  const welcomeGreet = document.getElementById('welcomeGreet');
  const welcomeName  = document.getElementById('welcomeName');
  if (welcomeBar && welcomeGreet && welcomeName) {
    welcomeGreet.textContent = typeof t === 'function' ? t('welcome.greeting') : 'Chào mừng trở lại,';
    welcomeName.textContent  = displayName;
    welcomeBar.style.display = '';
  }

  /* hide signup-only CTAs */
  const signupCta = document.querySelector('.hero__actions a[href*="signup"]');
  if (signupCta) signupCta.style.display = 'none';

  const finalCta = document.getElementById('finalCta');
  if (finalCta) {
    finalCta.style.display = 'none';
    const next = finalCta.nextElementSibling;
    if (next && next.classList.contains('divider')) next.style.display = 'none';
  }
}

// Auto-init on load
document.addEventListener('DOMContentLoaded', initAuthUI);
