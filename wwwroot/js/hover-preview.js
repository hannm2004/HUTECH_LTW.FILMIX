/**
 * FILMIX — Hover Video Preview (Netflix-style)
 * Tính năng 3: Xem trước phim khi di chuột
 *
 * Flow:
 *  1. Hover vào .t-card → chờ 600ms (debounce)
 *  2. Tính vị trí popup thông minh (tránh tràn viền màn hình)
 *  3. Hiển thị thumbnail + metadata ngay lập tức
 *  4. Sau 800ms → inject <video> & autoplay muted
 *  5. Rời chuột → huỷ tất cả timer, ẩn popup mượt mà
 */

(function () {
    'use strict';

    // ─── Cấu hình ───────────────────────────────────────────────
    const CFG = {
        hoverDelay:   600,   // ms trước khi popup xuất hiện
        videoDelay:   900,   // ms trước khi video bắt đầu load
        hideDelay:    250,   // ms trước khi popup ẩn (cho phép chuột chuyển sang popup)
        popupWidth:   320,   // px — phải khớp với CSS
        popupHeight:  340,   // px ước tính tổng chiều cao popup
        edgePadding:  12,    // px cách mép màn hình
    };

    // Dữ liệu phim — khớp với seed data trong ApplicationDbContext.cs
    // trailerUrl: dùng sample video tự host hoặc YouTube embed
    // Vì là demo, dùng Big Buck Bunny clips (royalty-free)
    const MOVIE_DATA = {
        1:  { title: 'Breaking Bad',           year: 2008, genres: ['Crime', 'Drama'],     rating: '18+', match: 98, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        2:  { title: 'Game of Thrones',        year: 2011, genres: ['Fantasy', 'Action'],  rating: '18+', match: 96, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        3:  { title: 'Oppenheimer',            year: 2023, genres: ['Drama', 'History'],   rating: '16+', match: 94, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        4:  { title: 'Avengers: Infinity War', year: 2018, genres: ['Action', 'Sci-Fi'],   rating: 'T13', match: 97, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        5:  { title: 'Fight Club',             year: 1999, genres: ['Drama', 'Thriller'],  rating: '18+', match: 95, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        6:  { title: 'The Dark Knight',        year: 2008, genres: ['Action', 'Drama'],    rating: 'T13', match: 99, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        7:  { title: 'Interstellar',           year: 2014, genres: ['Sci-Fi', 'Drama'],    rating: 'T13', match: 97, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        8:  { title: 'Wednesday',              year: 2022, genres: ['Horror', 'Fantasy'],  rating: '16+', match: 92, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        9:  { title: 'Squid Game',             year: 2021, genres: ['Thriller', 'Action'], rating: '18+', match: 98, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
        10: { title: 'Spider-Man: NWH',        year: 2021, genres: ['Action', 'Sci-Fi'],   rating: 'T13', match: 96, trailer: 'https://www.w3schools.com/html/mov_bbb.mp4' },
    };

    // ─── State ──────────────────────────────────────────────────
    let popup        = null;
    let currentCard  = null;
    let hoverTimer   = null;
    let videoTimer   = null;
    let hideTimer    = null;
    let isMuted      = true;
    let currentVideo = null;

    // ─── Tạo DOM popup một lần ──────────────────────────────────
    function createPopup() {
        popup = document.createElement('div');
        popup.id = 'hover-preview-popup';
        popup.setAttribute('role', 'tooltip');
        popup.setAttribute('aria-live', 'polite');

        popup.innerHTML = `
            <div class="hp-media" id="hp-media">
                <img class="hp-thumb" id="hp-thumb" src="" alt="" />
                <div class="hp-loading" id="hp-loading"><div class="hp-spinner"></div></div>
                <div class="hp-progress-bar" id="hp-progress" style="width:0%"></div>
                <button class="hp-mute-badge" id="hp-mute-btn" title="Bật/Tắt âm thanh" aria-label="Bật/Tắt âm thanh">
                    <svg id="hp-mute-icon" width="16" height="16" viewBox="0 0 24 24" fill="white">
                        <path d="M16.5 12c0-1.77-1.02-3.29-2.5-4.03v2.21l2.45 2.45c.03-.2.05-.41.05-.63zm2.5 0c0 .94-.2 1.82-.54 2.64l1.51 1.51C20.63 14.91 21 13.5 21 12c0-4.28-2.99-7.86-7-8.77v2.06c2.89.86 5 3.54 5 6.71zM4.27 3L3 4.27 7.73 9H3v6h4l5 5v-6.73l4.25 4.25c-.67.52-1.42.93-2.25 1.18v2.06c1.38-.31 2.63-.95 3.69-1.81L19.73 21 21 19.73l-9-9L4.27 3zM12 4L9.91 6.09 12 8.18V4z"/>
                    </svg>
                </button>
            </div>
            <div class="hp-info">
                <div class="hp-actions">
                    <button class="hp-btn hp-btn-play" id="hp-play-btn" title="Xem ngay">
                        <svg width="20" height="20" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
                    </button>
                    <button class="hp-btn hp-btn-add" id="hp-add-btn" title="+ Danh Sách">
                        <svg width="18" height="18" viewBox="0 0 24 24"><path d="M19 13H13v6h-2v-6H5v-2h6V5h2v6h6v2z"/></svg>
                    </button>
                    <button class="hp-btn hp-btn-like" id="hp-like-btn" title="Thích">
                        <svg width="18" height="18" viewBox="0 0 24 24"><path d="M1 21h4V9H1v12zm22-11c0-1.1-.9-2-2-2h-6.31l.95-4.57.03-.32c0-.41-.17-.79-.44-1.06L14.17 1 7.59 7.59C7.22 7.95 7 8.45 7 9v10c0 1.1.9 2 2 2h9c.83 0 1.54-.5 1.84-1.22l3.02-7.05c.09-.23.14-.47.14-.73v-2z"/></svg>
                    </button>
                    <a class="hp-btn-detail" id="hp-detail-link" href="#" title="Thông tin thêm">
                        <svg width="18" height="18" viewBox="0 0 24 24"><path d="M7.41 8.59L12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z"/></svg>
                    </a>
                </div>
                <div class="hp-score-row">
                    <span class="hp-match" id="hp-match">98% Phù Hợp</span>
                    <span class="hp-rating" id="hp-rating">18+</span>
                    <span class="hp-year" id="hp-year">2008</span>
                    <span class="hp-hd">HD</span>
                </div>
                <div class="hp-title" id="hp-title">Breaking Bad</div>
                <div class="hp-genres" id="hp-genres"></div>
            </div>
        `;

        document.body.appendChild(popup);
        bindPopupEvents();
    }

    // ─── Gắn sự kiện cho popup ──────────────────────────────────
    function bindPopupEvents() {
        // Khi chuột vào popup → không ẩn
        popup.addEventListener('mouseenter', () => {
            clearTimeout(hideTimer);
        });

        // Khi chuột rời popup → ẩn popup
        popup.addEventListener('mouseleave', () => {
            scheduleHide();
        });

        // Nút Mute/Unmute
        document.getElementById('hp-mute-btn').addEventListener('click', (e) => {
            e.stopPropagation();
            toggleMute();
        });

        // Nút Play → navigate đến trang detail
        document.getElementById('hp-play-btn').addEventListener('click', () => {
            if (currentCard) {
                const href = currentCard.getAttribute('href') || currentCard.dataset.href;
                if (href) window.location.href = href;
            }
        });

        // Nút + Danh Sách
        document.getElementById('hp-add-btn').addEventListener('click', (e) => {
            e.stopPropagation();
            const movieId = popup.dataset.movieId;
            if (movieId) addToWatchlist(parseInt(movieId));
        });

        // Nút Like (toggle)
        document.getElementById('hp-like-btn').addEventListener('click', (e) => {
            e.stopPropagation();
            const btn = e.currentTarget;
            btn.classList.toggle('liked');
            const svg = btn.querySelector('svg');
            svg.style.fill = btn.classList.contains('liked') ? '#e50914' : '#fff';
        });
    }

    // ─── Cập nhật nội dung popup ─────────────────────────────────
    function updatePopupContent(movieId, thumbSrc) {
        const data = MOVIE_DATA[movieId];
        if (!data) return;

        popup.dataset.movieId = movieId;

        // Thumbnail
        const thumb = document.getElementById('hp-thumb');
        thumb.src = thumbSrc || `/images/movies/${movieId}.jpg`;
        thumb.alt = data.title;

        // Metadata
        document.getElementById('hp-match').textContent  = data.match + '% Phù Hợp';
        document.getElementById('hp-rating').textContent = data.rating;
        document.getElementById('hp-year').textContent   = data.year;
        document.getElementById('hp-title').textContent  = data.title;

        // Detail link
        document.getElementById('hp-detail-link').href = `/Product/Detail/${movieId}`;

        // Genre tags
        const genreEl = document.getElementById('hp-genres');
        genreEl.innerHTML = data.genres.map(g =>
            `<span class="hp-genre-tag">${g}</span>`
        ).join('');

        // Restore mute icon
        updateMuteIcon();

        // Progress bar (resume watching từ localStorage)
        const saved = localStorage.getItem(`filmix_progress_${movieId}`);
        const bar   = document.getElementById('hp-progress');
        if (saved) {
            try {
                const prog = JSON.parse(saved);
                const pct  = prog.duration > 0 ? Math.min((prog.time / prog.duration) * 100, 100) : 0;
                bar.style.width = pct + '%';
                bar.style.display = 'block';
            } catch (_) { bar.style.display = 'none'; }
        } else {
            bar.style.display = 'none';
        }

        // Watchlist check → đổi icon + nút Add nếu đã thêm
        const watchlist = JSON.parse(localStorage.getItem('filmix_watchlist') || '[]');
        const addBtn    = document.getElementById('hp-add-btn');
        if (watchlist.some(w => w.id == movieId)) {
            addBtn.innerHTML = `<svg width="18" height="18" viewBox="0 0 24 24" fill="#46d369"><path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/></svg>`;
            addBtn.title = 'Đã thêm vào Danh Sách';
        } else {
            addBtn.innerHTML = `<svg width="18" height="18" viewBox="0 0 24 24" fill="white"><path d="M19 13H13v6h-2v-6H5v-2h6V5h2v6h6v2z"/></svg>`;
            addBtn.title = '+ Danh Sách';
        }
    }

    // ─── Load video ──────────────────────────────────────────────
    function loadVideo(movieId) {
        const data = MOVIE_DATA[movieId];
        if (!data || !data.trailer) return;

        const mediaEl  = document.getElementById('hp-media');
        const loadingEl = document.getElementById('hp-loading');

        // Xoá video cũ nếu có
        if (currentVideo) {
            currentVideo.pause();
            currentVideo.remove();
            currentVideo = null;
        }

        // Hiển thị spinner
        loadingEl.classList.add('active');

        const video = document.createElement('video');
        video.muted    = isMuted;
        video.loop     = true;
        video.preload  = 'auto';
        video.playsInline = true;
        video.src      = data.trailer;
        video.style.cssText = 'position:absolute;inset:0;width:100%;height:100%;object-fit:cover;';

        video.addEventListener('canplay', () => {
            loadingEl.classList.remove('active');
            video.classList.add('playing');
            video.play().catch(() => {
                // Autoplay bị chặn — ẩn video, giữ thumbnail
                video.classList.remove('playing');
            });
        }, { once: true });

        video.addEventListener('error', () => {
            loadingEl.classList.remove('active');
        });

        mediaEl.appendChild(video);
        currentVideo = video;
    }

    // ─── Tính toán vị trí popup thông minh ──────────────────────
    function computePopupPosition(cardRect) {
        const vw = window.innerWidth;
        const vh = window.innerHeight;
        const pw = CFG.popupWidth;
        const ph = CFG.popupHeight;
        const pad = CFG.edgePadding;

        // Tâm ngang của card
        const cardCenterX = cardRect.left + cardRect.width / 2;
        let left = cardCenterX - pw / 2;
        let top  = cardRect.bottom + 8; // mặc định hiện phía dưới

        // Nếu tràn phải
        if (left + pw > vw - pad) left = vw - pw - pad;
        // Nếu tràn trái
        if (left < pad) left = pad;

        // Nếu phía dưới không đủ chỗ → hiện phía trên
        if (top + ph > vh - pad) {
            top = cardRect.top - ph - 8;
        }
        // Phòng trường hợp card ở trên cùng
        if (top < pad) top = cardRect.bottom + 8;

        return { left, top };
    }

    // ─── Hiển thị popup ──────────────────────────────────────────
    function showPopup(card, movieId, thumbSrc) {
        clearTimeout(hideTimer);

        updatePopupContent(movieId, thumbSrc);

        const cardRect = card.getBoundingClientRect();
        const { left, top } = computePopupPosition(cardRect);

        popup.style.left   = left + 'px';
        popup.style.top    = top  + 'px';
        popup.style.width  = CFG.popupWidth + 'px';

        popup.classList.add('visible');

        // Load video sau thêm 300ms (ưu tiên animation trước)
        videoTimer = setTimeout(() => loadVideo(movieId), 300);
    }

    // ─── Ẩn popup ────────────────────────────────────────────────
    function hidePopup() {
        popup.classList.remove('visible');
        if (currentCard) currentCard.classList.remove('hp-hovered');
        currentCard = null;

        // Dừng video
        if (currentVideo) {
            currentVideo.pause();
            setTimeout(() => {
                if (currentVideo) {
                    currentVideo.remove();
                    currentVideo = null;
                }
                document.getElementById('hp-loading').classList.remove('active');
            }, 250);
        }

        clearTimeout(hoverTimer);
        clearTimeout(videoTimer);
    }

    function scheduleHide() {
        clearTimeout(hideTimer);
        hideTimer = setTimeout(hidePopup, CFG.hideDelay);
    }

    // ─── Mute/Unmute ─────────────────────────────────────────────
    function toggleMute() {
        isMuted = !isMuted;
        if (currentVideo) currentVideo.muted = isMuted;
        updateMuteIcon();
    }

    function updateMuteIcon() {
        const icon = document.getElementById('hp-mute-icon');
        if (!icon) return;
        if (isMuted) {
            icon.innerHTML = '<path d="M16.5 12c0-1.77-1.02-3.29-2.5-4.03v2.21l2.45 2.45c.03-.2.05-.41.05-.63zm2.5 0c0 .94-.2 1.82-.54 2.64l1.51 1.51C20.63 14.91 21 13.5 21 12c0-4.28-2.99-7.86-7-8.77v2.06c2.89.86 5 3.54 5 6.71zM4.27 3L3 4.27 7.73 9H3v6h4l5 5v-6.73l4.25 4.25c-.67.52-1.42.93-2.25 1.18v2.06c1.38-.31 2.63-.95 3.69-1.81L19.73 21 21 19.73l-9-9L4.27 3zM12 4L9.91 6.09 12 8.18V4z"/>';
        } else {
            icon.innerHTML = '<path d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z"/>';
        }
    }

    // ─── Thêm vào Watchlist ──────────────────────────────────────
    function addToWatchlist(movieId) {
        const data = MOVIE_DATA[movieId];
        if (!data) return;

        let watchlist = JSON.parse(localStorage.getItem('filmix_watchlist') || '[]');
        const exists  = watchlist.some(w => w.id == movieId);

        if (exists) {
            watchlist = watchlist.filter(w => w.id != movieId);
            showToast('Đã xoá khỏi Danh Sách của bạn', false);
        } else {
            watchlist.push({
                id:       movieId,
                title:    data.title,
                imageUrl: `/images/movies/${movieId}.jpg`,
                year:     data.year,
                genre:    data.genres.join('/'),
            });
            showToast('Đã thêm vào Danh Sách của bạn ✓', true);
        }

        localStorage.setItem('filmix_watchlist', JSON.stringify(watchlist));

        // Cập nhật icon nút Add
        const addBtn = document.getElementById('hp-add-btn');
        if (!exists) {
            addBtn.innerHTML = `<svg width="18" height="18" viewBox="0 0 24 24" fill="#46d369"><path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/></svg>`;
            addBtn.title = 'Đã thêm vào Danh Sách';
        } else {
            addBtn.innerHTML = `<svg width="18" height="18" viewBox="0 0 24 24" fill="white"><path d="M19 13H13v6h-2v-6H5v-2h6V5h2v6h6v2z"/></svg>`;
            addBtn.title = '+ Danh Sách';
        }
    }

    // ─── Toast notification ──────────────────────────────────────
    function showToast(msg, success) {
        let toast = document.getElementById('hp-toast');
        if (!toast) {
            toast = document.createElement('div');
            toast.id = 'hp-toast';
            toast.style.cssText = `
                position: fixed; bottom: 24px; left: 50%; transform: translateX(-50%) translateY(20px);
                background: #1f1f1f; color: #fff; font-size: 0.88rem; font-weight: 600;
                padding: 12px 22px; border-radius: 6px; z-index: 99999;
                box-shadow: 0 8px 30px rgba(0,0,0,0.7); pointer-events: none;
                opacity: 0; transition: all 0.3s cubic-bezier(0.34,1.26,0.64,1);
                border-left: 4px solid #e50914; white-space: nowrap;
            `;
            document.body.appendChild(toast);
        }
        toast.textContent = msg;
        toast.style.borderLeftColor = success ? '#46d369' : '#e50914';
        toast.style.opacity = '1';
        toast.style.transform = 'translateX(-50%) translateY(0)';

        clearTimeout(toast._timer);
        toast._timer = setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(-50%) translateY(10px)';
        }, 2800);
    }

    // ─── Trích xuất movie ID từ nhiều loại card ─────────────────
    function extractMovieId(el) {
        // Ưu tiên data attribute nếu đã được set
        if (el.dataset.hpId) return parseInt(el.dataset.hpId);

        // Từ href (thẻ <a> với /Product/Detail/{id})
        const href = el.getAttribute('href') || '';
        let match  = href.match(/\/Product\/Detail\/(\d+)/i);
        if (match) return parseInt(match[1]);

        // Từ onclick (newhot-card dùng onclick="window.location.href='...'")
        const onclick = el.getAttribute('onclick') || '';
        match = onclick.match(/\/Product\/Detail\/(\d+)/i);
        if (match) return parseInt(match[1]);

        // Từ asp-route-id rendered thành href
        const aspHref = el.getAttribute('href') || '';
        match = aspHref.match(/\/(\d+)(?:\?|$|#)/);
        if (match) return parseInt(match[1]);

        return null;
    }

    // ─── Gắn listener vào một card element ──────────────────────
    function attachListenerToCard(card) {
        // Tránh gắn trùng
        if (card.dataset.hpBound === '1') return;
        card.dataset.hpBound = '1';

        const movieId = extractMovieId(card);
        if (!movieId || !MOVIE_DATA[movieId]) return;

        // Lấy thumbnail
        const imgEl    = card.querySelector('img');
        const thumbSrc = imgEl ? imgEl.getAttribute('src') : `/images/movies/${movieId}.jpg`;

        // Ghi nhớ movieId để dùng ở event handler
        card.dataset.hpId = movieId;

        card.addEventListener('mouseenter', () => {
            clearTimeout(hideTimer);
            clearTimeout(hoverTimer);

            hoverTimer = setTimeout(() => {
                if (currentCard && currentCard !== card) {
                    currentCard.classList.remove('hp-hovered');
                }
                currentCard = card;
                card.classList.add('hp-hovered');
                showPopup(card, movieId, thumbSrc);
            }, CFG.hoverDelay);
        });

        card.addEventListener('mouseleave', () => {
            clearTimeout(hoverTimer);
            scheduleHide();
        });
    }

    // ─── Attach listeners lên tất cả card (t-card + newhot-card) ─
    function attachCardListeners() {
        // Hỗ trợ t-card (trang Home, TVShows, Movies, Product/List, Detail)
        document.querySelectorAll('.t-card').forEach(attachListenerToCard);

        // Hỗ trợ newhot-card (trang Mới & Hot dùng onclick thay href)
        document.querySelectorAll('.newhot-card').forEach(attachListenerToCard);
    }

    // ─── MutationObserver — theo dõi card mới được inject ────────
    function observeNewCards() {
        // Theo dõi container allMoviesGrid (Home page "Xem Tất Cả")
        const gridEl = document.getElementById('allMoviesGrid');
        if (gridEl) {
            new MutationObserver(() => attachCardListeners())
                .observe(gridEl, { childList: true, subtree: false });
        }

        // Theo dõi toàn bộ body để bắt mọi card được inject động
        new MutationObserver((mutations) => {
            let needsScan = false;
            mutations.forEach(m => {
                m.addedNodes.forEach(node => {
                    if (node.nodeType === 1) {
                        if (node.classList?.contains('t-card') ||
                            node.classList?.contains('newhot-card') ||
                            node.querySelector?.('.t-card, .newhot-card')) {
                            needsScan = true;
                        }
                    }
                });
            });
            if (needsScan) attachCardListeners();
        }).observe(document.body, { childList: true, subtree: true });
    }

    // ─── Đóng popup khi nhấn Escape ─────────────────────────────
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && popup && popup.classList.contains('visible')) {
            hidePopup();
        }
    });

    // ─── Khởi tạo ────────────────────────────────────────────────
    function init() {
        createPopup();
        attachCardListeners();
        observeNewCards();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
