/* ─────────────────────────────────────────────────────────────
   FILMIX — Watchlist Sync
   Đồng bộ "Danh Sách Của Tôi" giữa localStorage (cache phía client)
   và DB (nguồn dữ liệu chuẩn theo từng user).

   - Khi đăng nhập: hydrate localStorage từ DB (render sẵn ở _Layout)
     ngay khi tải trang (đồng bộ, trước các script khác) → tránh sai
     trạng thái nút "Đã lưu".
   - Mọi thay đổi tới localStorage['filmix_watchlist'] (thêm/xoá/xoá hết
     từ bất kỳ trang nào) tự động được đẩy lên DB.
   Tải ở <head> nên chạy trước khi body được parse.
   ───────────────────────────────────────────────────────────── */
(function () {
    var KEY = 'filmix_watchlist';
    var authed = window.FILMIX_AUTH === true;

    // setItem gốc — dùng để ghi mà KHÔNG kích hoạt đẩy lên server (lúc hydrate)
    var origSetItem = localStorage.setItem.bind(localStorage);

    // 1) Hydrate: với user đã đăng nhập, localStorage = watchlist trong DB
    if (authed) {
        try {
            var serverIds = (window.FILMIX_WATCHLIST || []).map(String);
            origSetItem(KEY, JSON.stringify(serverIds));
        } catch (e) { /* ignore */ }
    }

    // 2) Đẩy toàn bộ danh sách hiện tại lên DB (chỉ khi đã đăng nhập)
    function pushToServer(list) {
        var ids = (list || [])
            .map(function (x) { return parseInt(x, 10); })
            .filter(function (n) { return !isNaN(n) && n > 0; });
        fetch('/api/watchlist/sync', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify(ids)
        }).catch(function () { /* offline / lỗi mạng: bỏ qua, localStorage vẫn giữ */ });
    }

    // 3) Ghi đè setItem: mọi thay đổi watchlist từ các trang đều được lưu xuống DB
    localStorage.setItem = function (k, v) {
        origSetItem(k, v);
        if (authed && k === KEY) {
            try { pushToServer(JSON.parse(v || '[]')); } catch (e) { /* ignore */ }
        }
    };
})();
