/* ═══════════════════════════════════════════════════════════
   FILMIX — Hero Banner ViewComponent Single Logic
═══════════════════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', () => {
    const banner = document.getElementById('heroBanner');
    if (!banner) return;

    const video = document.getElementById('heroBannerVideo');
    const muteBtn = document.getElementById('heroMuteBtn');
    const iconMute = muteBtn ? muteBtn.querySelector('.icon-mute') : null;
    const iconUnmute = muteBtn ? muteBtn.querySelector('.icon-unmute') : null;

    let isVideoPlaying = false;
    let isVideoMuted = true;

    // --- VIDEO INITIALIZATION ---
    function initVideo() {
        if (!video) return;

        const trailerUrl = banner.dataset.videoUrl;
        if (!trailerUrl) {
            // No trailer, stay on background image
            return;
        }

        // We have a video, set src and prepare for playback
        video.src = trailerUrl;
        
        // Show mute button if we have video
        if (muteBtn) muteBtn.style.display = 'flex';

        // Autoplay logic (Netflix style: wait 3 seconds before playing)
        setTimeout(() => {
            playVideo();
        }, 3000);
    }

    function playVideo() {
        if (!video || !video.src) return;
        
        video.play().then(() => {
            banner.classList.add('video-playing');
            isVideoPlaying = true;
        }).catch(err => {
            console.warn("Autoplay prevented:", err);
        });
    }

    function pauseVideo() {
        if (!video) return;
        video.pause();
        isVideoPlaying = false;
    }

    // --- MUTE TOGGLE ---
    if (muteBtn) {
        muteBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            if (!video) return;

            isVideoMuted = !isVideoMuted;
            video.muted = isVideoMuted;

            if (isVideoMuted) {
                iconMute.style.display = 'block';
                iconUnmute.style.display = 'none';
            } else {
                iconMute.style.display = 'none';
                iconUnmute.style.display = 'block';
            }
        });
    }

    // --- PARALLAX & VISIBILITY ---
    function handleScroll() {
        const scrollY = window.scrollY;
        if (scrollY > window.innerHeight) return; // Optimize: don't process if way below

        // 1. Content Parallax
        const content = document.getElementById('heroBannerContent');
        if (content) {
            content.style.transform = `translateY(${scrollY * 0.4}px)`;
            content.style.opacity = Math.max(0, 1 - scrollY / 400);
        }

        // 2. Background darkening
        const bg = document.querySelector('.hero-banner__bg');
        if (bg) {
            bg.style.filter = `brightness(${Math.max(0.3, 1 - scrollY / 600)})`;
        }
    }
    
    // Use requestAnimationFrame for smooth scroll parallax
    let ticking = false;
    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                handleScroll();
                ticking = false;
            });
            ticking = true;
        }
    }, { passive: true });

    // --- INTERSECTION OBSERVER (Pause video when off-screen) ---
    if (window.IntersectionObserver && video) {
        const obs = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    if (!isVideoPlaying && banner.classList.contains('video-playing')) {
                        video.play();
                    }
                } else {
                    if (isVideoPlaying) {
                        video.pause();
                    }
                }
            });
        }, { threshold: 0.1 });
        obs.observe(banner);
    }

    // Start
    initVideo();
});
