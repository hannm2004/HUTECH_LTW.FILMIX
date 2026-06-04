/* ═══════════════════════════════════════════════════════════════
   FILMIX — Continue Watching Engine
   • Reads/writes filmix_progress_{id} from localStorage
   • Renders CW slider row on Home page dynamically
   • Saves progress every 5s + on pause/close/beforeunload
   • Auto-resumes playback from saved timestamp
═══════════════════════════════════════════════════════════════ */

(function () {
    'use strict';

    const STORAGE_PREFIX = 'filmix_progress_';
    const STORAGE_CW_ORDER = 'filmix_cw_order';
    const MIN_PROGRESS_PCT = 2;    // Min % to appear in CW row
    const MAX_PROGRESS_PCT = 95;   // Max % — after this it's "finished"
    const SAVE_INTERVAL_MS = 5000; // Save every 5 seconds while watching

    // ─── Progress Data Helpers ────────────────────────────────────

    function getProgress(movieId) {
        try {
            const raw = localStorage.getItem(STORAGE_PREFIX + movieId);
            return raw ? JSON.parse(raw) : null;
        } catch { return null; }
    }

    function saveProgress(data) {
        try {
            localStorage.setItem(STORAGE_PREFIX + data.id, JSON.stringify(data));
            updateCWOrder(data.id);
        } catch (e) {
            console.warn('[CW] localStorage write failed:', e);
        }
    }

    function removeProgress(movieId) {
        localStorage.removeItem(STORAGE_PREFIX + movieId);
        const order = getCWOrder().filter(id => id !== String(movieId));
        localStorage.setItem(STORAGE_CW_ORDER, JSON.stringify(order));
    }

    function getCWOrder() {
        try {
            return JSON.parse(localStorage.getItem(STORAGE_CW_ORDER) || '[]');
        } catch { return []; }
    }

    function updateCWOrder(movieId) {
        const id = String(movieId);
        let order = getCWOrder().filter(x => x !== id);
        order.unshift(id); // Most recent first
        order = order.slice(0, 20); // Keep max 20
        localStorage.setItem(STORAGE_CW_ORDER, JSON.stringify(order));
    }

    function getAllCWItems() {
        const order = getCWOrder();
        return order
            .map(id => getProgress(id))
            .filter(p => {
                if (!p) return false;
                const pct = p.duration > 0 ? (p.time / p.duration) * 100 : 0;
                return pct >= MIN_PROGRESS_PCT && pct <= MAX_PROGRESS_PCT;
            });
    }

    // ─── Format Time Helper ────────────────────────────────────────

    function formatTimeLeft(currentTime, duration) {
        if (!duration) return '';
        const remaining = Math.max(0, duration - currentTime);
        const mins = Math.floor(remaining / 60);
        if (mins <= 0) return '';
        return mins + ' phút còn lại';
    }

    // ─── Render CW Row ─────────────────────────────────────────────

    function renderCWRow() {
        const section = document.getElementById('cwSection');
        if (!section) return;

        const track = document.getElementById('cwTrack');
        if (!track) return;

        const items = getAllCWItems();

        if (items.length === 0) {
            section.classList.remove('has-items');
            return;
        }

        section.classList.add('has-items');
        track.innerHTML = '';

        items.forEach(prog => {
            const pct = prog.duration > 0 ? Math.min((prog.time / prog.duration) * 100, 100) : 0;
            const almostDone = pct > 90;
            const timeLeft = formatTimeLeft(prog.time, prog.duration);

            const card = document.createElement('a');
            card.href = '/Product/Detail/' + prog.id;
            card.className = 'cw-card';
            card.setAttribute('data-movie-id', prog.id);
            card.setAttribute('aria-label', 'Tiếp tục xem: ' + prog.title);

            card.innerHTML = `
                <img class="cw-card__thumb"
                     src="${prog.imageUrl || '/images/movies/default.jpg'}"
                     onerror="this.src='/images/movies/default.jpg'"
                     alt="${prog.title}" loading="lazy" />
                <div class="cw-card__gradient"></div>
                <div class="cw-card__play-overlay">
                    <div class="cw-card__play-icon">
                        <svg viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
                    </div>
                </div>
                <div class="cw-card__title">${prog.title}</div>
                ${timeLeft ? `<div class="cw-card__time-left">${timeLeft}</div>` : ''}
                <div class="cw-card__progress-wrap">
                    <div class="cw-card__progress-bar ${almostDone ? 'almost-done' : ''}"
                         style="width: ${pct.toFixed(1)}%;"></div>
                </div>
                <button class="cw-card__remove" aria-label="Xóa khỏi Continue Watching" title="Xóa">
                    <svg viewBox="0 0 24 24"><path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/></svg>
                </button>
            `;

            // Prevent link navigation when clicking remove button
            const removeBtn = card.querySelector('.cw-card__remove');
            removeBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                removeCWCard(prog.id, card);
            });

            track.appendChild(card);
        });

        // Re-init slider arrows for CW row via FilmixSlider
        const row = document.getElementById('cwSliderRow');
        if (row && window.FilmixSlider) {
            window.FilmixSlider.init(row);
        }
    }

    function removeCWCard(movieId, cardEl) {
        cardEl.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
        cardEl.style.opacity = '0';
        cardEl.style.transform = 'scale(0.85)';
        setTimeout(() => {
            removeProgress(movieId);
            cardEl.remove();
            // Hide section if empty
            const track = document.getElementById('cwTrack');
            if (track && track.children.length === 0) {
                const section = document.getElementById('cwSection');
                if (section) section.classList.remove('has-items');
            }
        }, 300);
    }

    // ─── Progress Tracking (Video Player) ─────────────────────────
    // Called from Detail.cshtml with movie metadata

    let _saveTimer = null;
    let _videoEl = null;
    let _movieMeta = null;

    window.ContinueWatching = {

        /**
         * Initialize progress tracking on a video element.
         * @param {HTMLVideoElement} videoEl
         * @param {{ id, title, imageUrl, genre, year }} meta
         */
        track(videoEl, meta) {
            if (!videoEl || !meta) return;
            _videoEl = videoEl;
            _movieMeta = meta;

            // Restore saved position
            const saved = getProgress(meta.id);
            if (saved && saved.time > 5) {
                videoEl.addEventListener('loadedmetadata', () => {
                    videoEl.currentTime = saved.time;
                }, { once: true });
            }

            // Save on timeupdate (throttled)
            videoEl.addEventListener('timeupdate', _onTimeUpdate);
            videoEl.addEventListener('pause', _saveNow);
            videoEl.addEventListener('ended', _onEnded);
        },

        /**
         * Stop tracking and save final position.
         */
        stop() {
            if (_videoEl) {
                _videoEl.removeEventListener('timeupdate', _onTimeUpdate);
                _videoEl.removeEventListener('pause', _saveNow);
                _videoEl.removeEventListener('ended', _onEnded);
                _saveNow();
            }
            clearInterval(_saveTimer);
            _videoEl = null;
            _movieMeta = null;
        },

        getProgress,
        saveProgress,
        removeProgress,
        renderCWRow
    };

    let _lastSave = 0;

    function _onTimeUpdate() {
        const now = Date.now();
        if (now - _lastSave < SAVE_INTERVAL_MS) return;
        _lastSave = now;
        _saveNow();
    }

    function logToServerHistory(movieId, watchTime) {
        const params = new URLSearchParams();
        params.append('movieId', movieId);
        params.append('watchTime', Math.round(watchTime));

        fetch('/ViewingHistory/Log', {
            method: 'POST',
            body: params,
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('[History] Logged watch history to server.');
            }
        })
        .catch(err => console.warn('[History] Error logging history to server:', err));
    }

    function _saveNow() {
        if (!_videoEl || !_movieMeta) return;
        const time = _videoEl.currentTime;
        const duration = _videoEl.duration || 0;
        if (time < 3) return; // Don't save if barely started

        // Send watch history log to server
        logToServerHistory(_movieMeta.id, time);

        const pct = duration > 0 ? (time / duration) * 100 : 0;
        // Remove from CW if >95% (finished)
        if (pct > MAX_PROGRESS_PCT) {
            removeProgress(_movieMeta.id);
            return;
        }

        saveProgress({
            id:       _movieMeta.id,
            title:    _movieMeta.title,
            imageUrl: _movieMeta.imageUrl,
            genre:    _movieMeta.genre,
            year:     _movieMeta.year,
            time,
            duration,
            savedAt:  Date.now()
        });
    }

    function _onEnded() {
        // Finished: remove from CW
        if (_movieMeta) {
            removeProgress(_movieMeta.id);
            // Log final progress as complete
            if (_videoEl) {
                logToServerHistory(_movieMeta.id, _videoEl.duration || 0);
            }
        }
    }

    // Save on page unload
    window.addEventListener('beforeunload', () => {
        _saveNow();
    });

    // ─── Auto-render on Home page ──────────────────────────────────

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', renderCWRow);
    } else {
        renderCWRow();
    }

})();
