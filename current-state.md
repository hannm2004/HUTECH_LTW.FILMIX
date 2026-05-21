# Nhật Ký Phát Triển Dự Án FILMIX - Cập nhật ngày 21/05/2026

## 1. Các công việc và chỉnh sửa đã hoàn thành hôm nay

### 🔍 Tìm Kiếm Thời Gian Thực (Smart Live Search)
- **Hoàn thành**: Xây dựng API Controller (`Controllers/SearchController.cs`) để truy vấn và trả về kết quả tìm kiếm phim bằng JSON. Hỗ trợ tìm kiếm theo nhiều tiêu chí: Tên phim, Thể loại, Diễn viên và Đạo diễn.
- **Hoàn thành**: Tích hợp thanh tìm kiếm trực tiếp trên thanh điều hướng chính (`_Layout.cshtml`). Tự động hiển thị danh sách thả xuống (dropdown) với hiệu ứng mượt mà khi người dùng gõ từ khóa (có áp dụng kỹ thuật Debounce 250ms để tối ưu hiệu suất gọi API).
- **Hoàn thành**: Giao diện kết quả tìm kiếm được thiết kế chi tiết với ảnh bìa (thumbnail), tên phim, năm phát hành và thẻ thể loại nổi bật.

### ▶️ Trình Phát Video Nâng Cao & Tự Động Hóa (Advanced Video Player)
- **Hoàn thành**: Tính năng **Lưu Trạng Thái Xem (Resume Watching)** - Tự động theo dõi tiến trình video (cứ 1 giây) và lưu vào `localStorage`. Khi xem lại, tự động tua đến thời điểm lưu và hiển thị Overlay *Đang xem tiếp từ [thời gian]* chuyên nghiệp bên trong player.
- **Hoàn thành**: Tính năng **Tự Động Phát Tập Tiếp Theo (Autoplay Next Episode)** - Tích hợp bộ đếm ngược 5 giây khi video kết thúc (`onended`), hiển thị overlay mờ toàn phần thông báo tự động chuyển tập, kèm lựa chọn *Phát Ngay* hoặc *Hủy*.
- **Hoàn thành**: Nút **Bỏ Qua Giới Thiệu (Skip Intro)** - Tự động hiện nút bấm bo góc chuyên nghiệp chuẩn Netflix tại thời điểm mô phỏng intro (giây thứ 5 đến thứ 25).

### 🎬 Hệ thống Đề xuất Phim tương tự ("More Like This" Recommendations)
- **Hoàn thành**: Cấu trúc logic nghiệp vụ trong `ProductController.cs` để tự động tìm kiếm các bộ phim có cùng thể loại (Category) hoặc cùng đạo diễn (Director) với phim đang xem (tối đa hiển thị 6 phim, loại trừ phim hiện tại).
- **Hoàn thành**: Tích hợp khối giao diện **"Nội Dung Tương Tự"** ở cuối trang chi tiết phim (`Views/Product/Detail.cshtml`) dạng grid. Tái sử dụng các lớp CSS `.trending__grid` và `.t-card` để giữ nguyên các hiệu ứng chuẩn Netflix như hover phóng to (scale), tăng độ bão hòa màu, hiển thị nút play đỏ và phần metadata tóm tắt.

### 🌐 Thông tin Cast/Director động (Dynamic Cast/Director)
- **Hoàn thành**: Thêm trường `Director` và `Cast` vào thực thể `Movie` (`Models/Entities/Entities.cs`).
- **Hoàn thành**: Cập nhật dữ liệu hạt giống (Seed Data) trong `ApplicationDbContext.cs` với thông tin đạo diễn và dàn diễn viên thực tế cho cả 10 bộ phim mẫu.
- **Hoàn thành**: Liên kết dữ liệu động vào trang chi tiết phim (`Views/Product/Detail.cshtml`) thay cho các thông tin diễn viên/đạo diễn bị hard-code trước đó.
- **Hoàn thành**: Tự động hóa cập nhật Schema trong `Program.cs` - thêm kiểm tra trường `Director`/`Cast` khi khởi chạy, nếu phát hiện cấu trúc bảng cũ sẽ tự động xóa và tạo lại database cùng dữ liệu mẫu mới.

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

### 🐛 Fix Bugs
- **Lỗi Cartesian Explosion trong EF Core**: Khi fetch Movies kèm theo `Episodes` và `MovieCategories`, ứng dụng bị crash. Đã fix bằng cách thêm `.AsSplitQuery()` vào ProductController.
- **Lỗi LocalStorage "Danh Sách Của Tôi" (Watchlist)**: Sửa lại JavaScript trong `Detail.cshtml` để đọc/ghi vào `localStorage` chính xác.
- **Lỗi `toggleLang()` không tồn tại** (`i18n.js`): Thêm hàm vào `i18n.js` kèm logic highlight ngôn ngữ đang active và đóng dropdown khi click ra ngoài.
- **Nút "Lưu Xem Sau" ở Phim Lẻ không hoạt động** (`Movies/Index.cshtml`): Thêm handler cho phim Interstellar.
- **Nút "+ Danh Sách" ở Mới & Hot không hoạt động** (`NewHot/Index.cshtml`): Thêm handler cho phim Spider-Man: No Way Home.
- **Nút "Danh sách của tôi" ở TV Shows không hoạt động** (`TVShows/Index.cshtml`): Thêm handler cho phim Breaking Bad.
- **Title tab trình duyệt bị duplicate "FILMIX"** (`_Layout.cshtml`): Bỏ ` - FILMIX` khỏi layout.
- **EF Core warning `FirstOrDefault` không có `OrderBy`** (`Program.cs`): Thêm `.OrderBy` để tránh kết quả không xác định.

---

## 2. Tính năng đã hoàn thành thêm

### 🎬 Xem Trước Phim Khi Di Chuột (Hover Video Preview) — Tính năng 3
- **Hoàn thành**: Tạo file `wwwroot/css/hover-preview.css` — CSS cho popup card Netflix-style với hiệu ứng scale, shadow, animation xuất hiện mượt mà.
- **Hoàn thành**: Tạo file `wwwroot/js/hover-preview.js` — JavaScript engine toàn diện:
  - Popup xuất hiện sau 600ms hover (debounce), ẩn sau 250ms rời chuột
  - Video autoplay muted sau 300ms, có nút toggle Mute/Unmute
  - Hiển thị thumbnail ngay lập tức + spinner khi video đang load
  - Positioning thông minh: tính toán tránh tràn mép màn hình (trên/dưới/trái/phải)
  - Popup hiển thị: % phù hợp, rating, năm, thể loại, tiến trình xem (resume bar)
  - Nút **Phát Ngay**, **+ Danh Sách** (sync với localStorage watchlist), **Like**, **Xem Chi Tiết**
  - Hỗ trợ đóng bằng phím `Escape`
  - MutationObserver: tự gắn listener cho card mới được inject động ("Xem Tất Cả")
- **Hoàn thành**: Include `hover-preview.css` và `hover-preview.js` vào `_Layout.cshtml` → **tự động áp dụng cho TẤT CẢ trang** (Home, TVShows, Phim Lẻ, Mới & Hot, Danh Sách, Detail)
- **Hoàn thành**: Hỗ trợ đa dạng loại card:
  - `.t-card` dùng `href` (Home, TVShows, Movies, Product/List, Detail)
  - `.newhot-card` dùng `onclick` (trang Mới & Hot)
  - Card inject động từ JavaScript ("Xem Tất Cả" grid)

---

## 3. Lộ trình phát triển tiếp theo (Next Steps)
- **Tính năng 5:** Xây dựng màn hình **Hồ Sơ Người Dùng ("Who's Watching?")** - Cho phép người dùng chọn Profile (ví dụ: Người lớn, Trẻ em) trước khi vào trang chủ để cá nhân hóa trải nghiệm và danh sách yêu thích.

---
**Trạng thái hiện tại: 🟢 Ổn định (Stable)** — Toàn bộ tính năng chính hoạt động đúng. Không còn lỗi JavaScript runtime hay bug giao diện nghiêm trọng.
