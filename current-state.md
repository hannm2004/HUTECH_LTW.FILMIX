# Nhật Ký Phát Triển Dự Án FILMIX - Cập nhật ngày 18/05/2026

## 1. Các công việc và chỉnh sửa đã hoàn thành hôm nay

### 🆕 Thêm Danh mục mới ("Phim Lẻ" & "Mới & Hot")
- Tạo `MoviesController.cs` và `NewHotController.cs`.
- Thiết kế giao diện riêng biệt với `movies.css` và `newhot.css`.
- Xây dựng Razor Views (`Views/Movies/Index.cshtml`, `Views/NewHot/Index.cshtml`).
- Cập nhật liên kết trên thanh điều hướng (`_Layout.cshtml`).

### 🛠 Cấu trúc Database & Kiến trúc mới (Movies Entity)
- Sửa lỗi Namespace không đồng nhất gây lỗi biên dịch.
- Hỗ trợ đa nền tảng cơ sở dữ liệu: Thêm tùy chọn `DbProvider` trong `appsettings.json` để chuyển đổi dễ dàng giữa **MySQL** và **SQL Server** cho các thành viên trong team.
- Tái cấu trúc thực thể `Movie`:
  - Thêm quan hệ nhiều-nhiều (Many-to-Many) với `Category` thông qua bảng `MovieCategory`.
  - Thêm quan hệ một-nhiều (One-to-Many) với `Episode` để hỗ trợ hiển thị danh sách các tập phim cho TV Series.
- Đồng bộ dữ liệu hạt giống (Seed Data) của DB với các phim hiển thị ngoài trang chủ để không bị lỗi.

### 🎬 Nâng cấp giao diện & Tính năng (Detail Page & Watchlist)
- Tạo trang **Chi tiết phim chuẩn Netflix** (`/Product/Detail/{id}`) với hero banner lớn, danh sách thể loại động, bộ chọn Season và các tập phim.
- Khắc phục lỗi **không click được vào thẻ phim**: Đã bọc các thẻ phim ở Trang Chủ, trang TV Shows và Danh Sách bằng thẻ `<a>`.
- Thiết kế giao diện **Tab 2 cột chuyên nghiệp** cho trang `/Product/List`:
  - **Tab 1:** Khám Phá Phim (mặc định, lọc theo thể loại).
  - **Tab 2:** Danh Sách Của Tôi (hiển thị phim được lưu trong bộ nhớ máy khách).

### 🐛 Fix Bugs — Phiên trước
- **Lỗi Cartesian Explosion trong EF Core**: Khi fetch Movies kèm theo `Episodes` và `MovieCategories`, ứng dụng bị crash. Đã fix triệt để bằng cách thêm `.AsSplitQuery()` vào ProductController.
- **Lỗi LocalStorage "Danh Sách Của Tôi" (Watchlist)**: Nút "Lưu Danh Sách" trước đó chỉ có hiệu ứng UI mà không lưu ID vào bộ nhớ trình duyệt, khiến Danh Sách Của Tôi luôn trống rỗng. Đã sửa lại JavaScript trong `Detail.cshtml` để đọc/ghi vào `localStorage` chính xác.

### 🔧 Fix Bugs — Phiên kiểm tra code hôm nay
- **[BUG 1 — Nghiêm trọng] `toggleLang()` không tồn tại** (`i18n.js`): Nút navbar gọi `onclick="toggleLang()"` nhưng hàm chưa được định nghĩa → crash `ReferenceError` khi click. Đã thêm hàm vào `i18n.js` kèm logic highlight ngôn ngữ đang active và đóng dropdown khi click ra ngoài.
- **[BUG 2] Nút "Lưu Xem Sau" ở Phim Lẻ không hoạt động** (`Movies/Index.cshtml`): Chỉ là HTML tĩnh, không có handler. Đã thêm `onclick="toggleHeroWatchlist(7, ...)"` cho phim Interstellar với toast notification và đổi màu trạng thái.
- **[BUG 3] Nút "+ Danh Sách" ở Mới & Hot không hoạt động** (`NewHot/Index.cshtml`): Đã thêm handler cho phim Spider-Man: No Way Home (id=10).
- **[BUG 4] Nút "Danh sách của tôi" ở TV Shows không hoạt động** (`TVShows/Index.cshtml`): Đã thêm handler cho phim Breaking Bad (id=1) trong hero section.
- **[BUG 5] Title tab trình duyệt bị duplicate "FILMIX"** (`_Layout.cshtml`): Pattern `"[Title] - FILMIX"` nhưng các view đã tự nhúng "FILMIX" → hiển thị `"FILMIX — ... - FILMIX"`. Đã bỏ ` - FILMIX` khỏi layout.
- **[BUG 6] EF Core warning `FirstOrDefault` không có `OrderBy`** (`Program.cs`): Đã thêm `.OrderBy(e => e.Id)` để tránh warning và kết quả không xác định.
---

## 2. Các Bug / Yêu cầu tính năng còn tồn đọng cần xử lý tiếp

### 🌐 Thông tin Cast/Director bị hard-code
- **Vấn đề**: Sidebar trang chi tiết phim hiện cùng một diễn viên/đạo diễn cho mọi bộ phim.
- **Yêu cầu**: Thêm field `Director` và `Cast` vào model `Movie`, cập nhật seed data và Detail view để hiển thị đúng thông tin từng phim.

---
**Trạng thái hiện tại: 🟢 Ổn định (Stable)** — Toàn bộ tính năng chính hoạt động đúng. Không còn lỗi JavaScript runtime hay bug giao diện nghiêm trọng.
