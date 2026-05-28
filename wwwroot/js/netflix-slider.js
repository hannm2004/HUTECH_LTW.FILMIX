/* ─────────────────────────────────────────────────────────────────────────────
   FILMIX — Modern Netflix Slider with Dynamic Pagination & Fluid Controls
   • Dynamic Chevron controls (large, overlay, full height)
   • Responsive Pagination Indicators (dashes at top-right on hover)
   • Smooth sliding animation synchronized with indicators
   • Drag-to-scroll with auto-updating pagination
   • Resize observer for layout changes
   ───────────────────────────────────────────────────────────────────────────── */

(function () {
  'use strict';

  function initSlider(row) {
    const track = row.querySelector('.slider-track');
    if (!track) return;

    // --- Dynamic Arrow Creation ---
    const btnPrev = document.createElement('button');
    btnPrev.className = 'slider-arrow slider-arrow--prev hidden';
    btnPrev.setAttribute('aria-label', 'Cuộn trái');
    btnPrev.innerHTML = `<svg viewBox="0 0 24 24" fill="currentColor"><path d="M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z"/></svg>`;

    const btnNext = document.createElement('button');
    btnNext.className = 'slider-arrow slider-arrow--next';
    btnNext.setAttribute('aria-label', 'Cuộn phải');
    btnNext.innerHTML = `<svg viewBox="0 0 24 24" fill="currentColor"><path d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z"/></svg>`;

    row.appendChild(btnPrev);
    row.appendChild(btnNext);

    // --- Dynamic Indicators (Dashes) Container ---
    const indicatorsContainer = document.createElement('div');
    indicatorsContainer.className = 'slider-indicators';
    row.appendChild(indicatorsContainer);

    let dots = [];
    let pageCount = 0;

    // Calculate dynamic layout sizes
    function getLayout() {
      const trackWidth = track.clientWidth;
      const scrollWidth = track.scrollWidth;
      
      // Calculate pageSize (leaves a card overlap to make continuous scrolling readable)
      // If clientWidth is 1200px, slide by 1100px
      const pageSize = Math.max(200, trackWidth - 80);
      
      // Calculate total page count
      const computedPages = Math.ceil((scrollWidth - 10) / pageSize);
      return { trackWidth, scrollWidth, pageSize, computedPages };
    }

    // Rebuild pagination indicators (dots)
    function rebuildIndicators() {
      indicatorsContainer.innerHTML = '';
      dots = [];
      
      const { computedPages } = getLayout();
      pageCount = computedPages;

      if (pageCount <= 1) return; // No pagination needed if all fit on one screen

      for (let i = 0; i < pageCount; i++) {
        const dot = document.createElement('button');
        dot.className = `slider-indicator-dot ${i === 0 ? 'active' : ''}`;
        dot.setAttribute('aria-label', `Đến trang ${i + 1}`);
        dot.addEventListener('click', () => {
          const { pageSize } = getLayout();
          track.scrollTo({ left: i * pageSize, behavior: 'smooth' });
        });
        indicatorsContainer.appendChild(dot);
        dots.push(dot);
      }
    }

    // Sync active state of dot and controls visibility
    function syncState() {
      const { pageSize, scrollWidth, trackWidth } = getLayout();
      const scrollLeft = track.scrollLeft;

      // 1. Update active indicators
      if (dots.length > 0) {
        const activeIdx = Math.round(scrollLeft / pageSize);
        dots.forEach((dot, idx) => {
          dot.classList.toggle('active', idx === activeIdx);
        });
      }

      // 2. Show/Hide arrows
      const atStart = scrollLeft <= 10;
      const atEnd = scrollLeft >= (scrollWidth - trackWidth - 10);
      btnPrev.classList.toggle('hidden', atStart);
      btnNext.classList.toggle('hidden', atEnd);
    }

    // Build controls initial state
    rebuildIndicators();
    syncState();

    // Event listener for scroll to update indicators on scroll (supports mouse dragging / swipe too)
    let scrollTimeout;
    track.addEventListener('scroll', () => {
      // Small throttle/debounce for performant updating
      clearTimeout(scrollTimeout);
      scrollTimeout = setTimeout(syncState, 50);
    }, { passive: true });

    // --- Arrow Click Actions ---
    btnPrev.addEventListener('click', () => {
      const { pageSize } = getLayout();
      track.scrollBy({ left: -pageSize, behavior: 'smooth' });
    });

    btnNext.addEventListener('click', () => {
      const { pageSize } = getLayout();
      track.scrollBy({ left: pageSize, behavior: 'smooth' });
    });

    // --- Drag-To-Scroll (Mouse & Touch) ---
    let isDragging = false;
    let startX = 0;
    let scrollLeft = 0;
    let dragDistance = 0;

    track.addEventListener('mousedown', (e) => {
      // Only drag with left click
      if (e.button !== 0) return;
      isDragging = true;
      startX = e.pageX - track.offsetLeft;
      scrollLeft = track.scrollLeft;
      dragDistance = 0;
      track.classList.add('grabbing');
    });

    document.addEventListener('mouseup', () => {
      if (!isDragging) return;
      isDragging = false;
      track.classList.remove('grabbing');
    });

    track.addEventListener('mousemove', (e) => {
      if (!isDragging) return;
      e.preventDefault();
      const x = e.pageX - track.offsetLeft;
      const walk = (x - startX) * 1.5;
      dragDistance = Math.abs(walk);
      track.scrollLeft = scrollLeft - walk;
    });

    // Prevent navigation click when dragging
    track.addEventListener('click', (e) => {
      if (dragDistance > 10) {
        e.preventDefault();
        e.stopPropagation();
      }
    }, true);

    track.addEventListener('mouseleave', () => {
      if (isDragging) {
        isDragging = false;
        track.classList.remove('grabbing');
      }
    });

    // Mobile touch controls
    let touchStartX = 0;
    let touchScrollLeft = 0;

    track.addEventListener('touchstart', (e) => {
      touchStartX = e.touches[0].pageX;
      touchScrollLeft = track.scrollLeft;
    }, { passive: true });

    track.addEventListener('touchmove', (e) => {
      const diff = touchStartX - e.touches[0].pageX;
      track.scrollLeft = touchScrollLeft + diff;
    }, { passive: true });

    // --- Resize handler ---
    let resizeTimeout;
    window.addEventListener('resize', () => {
      clearTimeout(resizeTimeout);
      resizeTimeout = setTimeout(() => {
        rebuildIndicators();
        syncState();
      }, 150);
    });
  }

  // Init all elements
  function initAll() {
    document.querySelectorAll('.slider-row').forEach(initSlider);
    applySkeletons();
  }

  // Apply shimmer skeleton to images that haven't loaded yet
  function applySkeletons() {
    document.querySelectorAll('.slider-track img, .similar-movies-section img').forEach(img => {
      if (img.complete && img.naturalWidth > 0) return; // already loaded

      const wrap = img.parentElement;
      if (!wrap) return;

      wrap.classList.add('img-loading');

      const cleanup = () => wrap.classList.remove('img-loading');
      img.addEventListener('load',  cleanup, { once: true });
      img.addEventListener('error', cleanup, { once: true });
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAll);
  } else {
    initAll();
  }

  window.FilmixSlider = { init: initSlider, initAll, applySkeletons };
})();
