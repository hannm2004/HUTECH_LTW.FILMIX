/* ─────────────────────────────────────────────────────
   FILMIX — Netflix Slider (horizontal scroll rows)
   • Arrow buttons show on row hover
   • Drag-to-scroll (mouse & touch)
   • Works with any .slider-row wrapper
   ───────────────────────────────────────────────────── */

(function () {
  'use strict';

  /* Amount to scroll per arrow click (px) */
  const SCROLL_AMOUNT = 900;

  function initSlider(row) {
    const track = row.querySelector('.slider-track');
    if (!track) return;

    /* ── Build arrow buttons ── */
    const btnPrev = document.createElement('button');
    btnPrev.className = 'slider-arrow slider-arrow--prev';
    btnPrev.setAttribute('aria-label', 'Cuộn trái');
    btnPrev.innerHTML = `<svg viewBox="0 0 24 24" fill="currentColor"><path d="M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z"/></svg>`;

    const btnNext = document.createElement('button');
    btnNext.className = 'slider-arrow slider-arrow--next';
    btnNext.setAttribute('aria-label', 'Cuộn phải');
    btnNext.innerHTML = `<svg viewBox="0 0 24 24" fill="currentColor"><path d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z"/></svg>`;

    row.appendChild(btnPrev);
    row.appendChild(btnNext);

    /* ── Arrow visibility ── */
    function updateArrows() {
      const atStart = track.scrollLeft <= 10;
      const atEnd   = track.scrollLeft >= track.scrollWidth - track.clientWidth - 10;
      btnPrev.classList.toggle('hidden', atStart);
      btnNext.classList.toggle('hidden', atEnd);
    }
    updateArrows();
    track.addEventListener('scroll', updateArrows, { passive: true });

    /* ── Arrow click ── */
    btnPrev.addEventListener('click', () => {
      track.scrollBy({ left: -SCROLL_AMOUNT, behavior: 'smooth' });
    });
    btnNext.addEventListener('click', () => {
      track.scrollBy({ left: SCROLL_AMOUNT, behavior: 'smooth' });
    });

    /* ── Mouse drag-to-scroll ── */
    let isDragging = false;
    let startX = 0;
    let scrollLeft = 0;

    track.addEventListener('mousedown', (e) => {
      isDragging = true;
      startX     = e.pageX - track.offsetLeft;
      scrollLeft = track.scrollLeft;
      track.classList.add('grabbing');
    });

    document.addEventListener('mouseup', () => {
      isDragging = false;
      track.classList.remove('grabbing');
    });

    track.addEventListener('mousemove', (e) => {
      if (!isDragging) return;
      e.preventDefault();
      const x    = e.pageX - track.offsetLeft;
      const walk = (x - startX) * 1.5;
      track.scrollLeft = scrollLeft - walk;
    });

    track.addEventListener('mouseleave', () => {
      isDragging = false;
      track.classList.remove('grabbing');
    });

    /* ── Touch drag-to-scroll ── */
    let touchStartX = 0;
    let touchScrollLeft = 0;

    track.addEventListener('touchstart', (e) => {
      touchStartX    = e.touches[0].pageX;
      touchScrollLeft = track.scrollLeft;
    }, { passive: true });

    track.addEventListener('touchmove', (e) => {
      const diff = touchStartX - e.touches[0].pageX;
      track.scrollLeft = touchScrollLeft + diff;
    }, { passive: true });
  }

  /* ── Initialize all .slider-row on page ── */
  function initAll() {
    document.querySelectorAll('.slider-row').forEach(initSlider);
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAll);
  } else {
    initAll();
  }

  /* Expose so dynamically added rows can also be init'd */
  window.FilmixSlider = { init: initSlider, initAll };
})();
