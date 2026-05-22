/* ─────────────────────────────────────────────────────
   FILMIX — Hero Banner Video Autoplay
   • Auto-plays muted trailer after 2.5s
   • Mute/Unmute toggle button (bottom-right)
   • Play button → go to Detail page
   • More Info button → go to Detail page
   • Fades & shrinks banner on scroll (IntersectionObserver)
   • Pauses video when banner leaves viewport (saves CPU/GPU)
   ───────────────────────────────────────────────────── */

(function () {
  'use strict';

  const AUTOPLAY_DELAY = 2500; // ms before video starts

  // Sample trailer URL (BigBuckBunny as fallback demo)
  const DEMO_VIDEO = 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4';

  function initHeroBanner() {
    const banner  = document.getElementById('heroBanner');
    const videoEl = document.getElementById('heroBannerVideo');
    const muteBtn = document.getElementById('heroMuteBtn');
    const playBtn = document.getElementById('heroPlayBtn');
    const infoBtn = document.getElementById('heroInfoBtn');

    if (!banner || !videoEl) return;

    /* ── Autoplay after delay ── */
    let autoplayTimer = setTimeout(() => {
      videoEl.muted = true;
      videoEl.play().catch(() => {}); // ignore autoplay policy errors
      banner.classList.add('video-playing');
    }, AUTOPLAY_DELAY);

    /* ── Mute / Unmute toggle ── */
    if (muteBtn) {
      muteBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        videoEl.muted = !videoEl.muted;
        muteBtn.classList.toggle('unmuted', !videoEl.muted);
        muteBtn.setAttribute('aria-label', videoEl.muted ? 'Bật tiếng' : 'Tắt tiếng');
        muteBtn.querySelector('.icon-mute').style.display   =  videoEl.muted ? 'block' : 'none';
        muteBtn.querySelector('.icon-unmute').style.display = !videoEl.muted ? 'block' : 'none';
      });
    }

    /* ── Play / Info buttons ── */
    const heroLink = banner.dataset.detailUrl || '#';
    if (playBtn) {
      playBtn.addEventListener('click', () => {
        window.location.href = heroLink;
      });
    }
    if (infoBtn) {
      infoBtn.addEventListener('click', () => {
        window.location.href = heroLink;
      });
    }

    /* ── IntersectionObserver: pause when off-screen, fade on scroll ── */
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          if (banner.classList.contains('video-playing')) {
            videoEl.play().catch(() => {});
          }
          banner.classList.remove('hero-scrolled');
        } else {
          videoEl.pause();
          banner.classList.add('hero-scrolled');
        }
      });
    }, { threshold: 0.15 });

    observer.observe(banner);

    /* ── Parallax + fade on scroll ── */
    let ticking = false;
    window.addEventListener('scroll', () => {
      if (!ticking) {
        requestAnimationFrame(() => {
          const scrollY = window.scrollY;
          const bannerH = banner.offsetHeight;
          const progress = Math.min(scrollY / bannerH, 1);

          // Fade out content
          const content = banner.querySelector('.hero-banner__content');
          if (content) {
            content.style.opacity  = 1 - progress * 1.6;
            content.style.transform = `translateY(${progress * 40}px)`;
          }

          // Darken video overlay as user scrolls
          const overlay = banner.querySelector('.hero-banner__overlay');
          if (overlay) {
            overlay.style.opacity = 0.45 + progress * 0.55;
          }

          ticking = false;
        });
        ticking = true;
      }
    }, { passive: true });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initHeroBanner);
  } else {
    initHeroBanner();
  }
})();
