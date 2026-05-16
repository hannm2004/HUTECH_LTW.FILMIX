# Nhật Ký Phát Triển Dự Án FILMIX - 16/05/2026

## 1. Các công việc đã hoàn thành

### 🖼️ Địa phương hóa hình ảnh (Localizing Assets)
- Tải toàn bộ ảnh phim từ các nguồn ngoài (Picsum, Wikipedia) về thư mục `wwwroot/images/`.
- Phân chia thư mục khoa học: `/hero`, `/movies`, `/tvshows`, `/auth`.
- Cập nhật toàn bộ mã nguồn để sử dụng đường dẫn cục bộ, giúp trang web chạy nhanh và ổn định hơn.

### 📺 Trang TV Shows (Netflix Style)
- Xây dựng trang `/TVShows` với giao diện cuộn ngang đặc trưng của Netflix.
- Có Hero section nổi bật cho phim "Stranger Things".
- Đã kết nối với Database và lọc riêng thể loại Series (`IsTVSeries = true`).

### 🎬 Trang Phim Lẻ (Movie Collection)
- Xây dựng trang `/Movies` hiển thị danh sách phim điện ảnh.
- Thiết kế Hero section ấn tượng cho "Avengers: Endgame".
- Tích hợp logic lọc dữ liệu (`IsTVSeries = false`) từ SQL Server.
- Hỗ trợ cuộn ngang (Drag to scroll) mượt mà cho các danh sách phim.

### 🔥 Trang Mới & Hot (Trending & New Content)
- Xây dựng trang `/NewAndHot` hiển thị danh sách phim và series đang thịnh hành.
- Thiết kế giao diện danh sách xếp hạng (Ranking list) với hiệu ứng TOP rank ấn tượng.
- Tích hợp logic lọc dữ liệu (`IsTrending = true`) từ SQL Server.
- Cập nhật Database: Thêm cột `IsTrending` và `Description` vào bảng `Movies`.
- Bổ sung dữ liệu mẫu (Seeding) với mô tả chi tiết cho từng phim.

### 🗄️ Tích hợp Cơ sở dữ liệu SQL Server
- Kết nối thành công với SQL Server (Instance: `LAPTOP-PN800PJP`).
- Tự động tạo Database `filmix_db` và các bảng `Categories`, `Movies` khi khởi chạy ứng dụng.
- Đã nạp dữ liệu mẫu (Seeding) trực tiếp vào DB.
- Chuyển đổi logic từ dữ liệu "cứng" sang truy vấn SQL thực tế.

### 📁 Tổ chức lại thư mục (Refactoring)
- Sắp xếp lại mã nguồn theo chuẩn chuyên nghiệp:
    - `Models/Entities/`: Chứa các thực thể dữ liệu.
    - `Models/ViewModels/`: Chứa các Model phục vụ hiển thị.
- Cập nhật toàn bộ Namespace và các câu lệnh `using` để hệ thống không bị lỗi sau khi di chuyển file.

---

## 2. Các lỗi tiềm ẩn & Lưu ý (Potential Bugs)

### ⚠️ Khởi động ứng dụng (Startup Crash)
- **Vấn đề**: Nếu dịch vụ SQL Server chưa được bật hoặc sai chuỗi kết nối, ứng dụng sẽ báo lỗi ngay khi khởi động do lệnh `db.Database.EnsureCreated()` được gọi sớm.
- **Cách khắc phục**: Đảm bảo SQL Server đang chạy trước khi F5 project.

### ✅ Xung đột dữ liệu mẫu (Seeding Conflicts)
- **Trạng thái**: Đã chuyển sang dùng **Migrations** (`dotnet ef migrations`). Việc thay đổi cấu trúc bảng và dữ liệu mẫu hiện được quản lý an toàn qua các file migration, giúp đồng bộ hóa database SQL Server chính xác hơn.

### ⚠️ Đường dẫn ảnh (Image Paths)
- **Vấn đề**: Hiện tại ảnh được fix cứng đuôi `.jpg`. Nếu sau này bạn thay thế bằng các ảnh định dạng `.png` hoặc `.webp`, bạn cần cập nhật lại cột `ImageUrl` trong database.

### ⚠️ Hiển thị TV Shows & Phim Lẻ
- **Trạng thái**: Đã giải quyết. Đã thêm cột `IsTVSeries` vào database để phân loại rõ ràng giữa phim bộ và phim lẻ.

---

## 3. Kế hoạch phát triển tiếp theo (Roadmap)

### 🚀 Ưu tiên cao (High Priority)
- [ ] **Movie Details Modal**: Xây dựng cửa sổ xem nhanh thông tin phim (Trailer, Diễn viên, Nội dung chi tiết) mà không cần chuyển trang.
- [ ] **Authentication (Identity)**: Thay thế hệ thống login giả lập bằng ASP.NET Core Identity. Kết nối với Database để quản lý User thực tế.
- [ ] **My List (Watchlist)**: Lưu danh sách phim yêu thích vào Database theo từng User.

### 🛠️ Cải thiện tính năng (Features)
- [ ] **Search Engine**: Thêm thanh tìm kiếm thời gian thực trên Navbar.
- [ ] **Video Player**: Tích hợp trình phát video (hoặc nhúng Youtube) để xem phim/trailer.
- [ ] **User Profiles**: Giao diện "Ai đang xem?" (Who's watching) đặc trưng của Netflix.

### 🎨 Tối ưu trải nghiệm (UX/UI)
- [ ] **Video Hero Section**: Chuyển đổi Hero Section từ ảnh tĩnh sang video ngắn (muted loop).
- [ ] **Skeleton Loading**: Hiệu ứng chờ khi đang tải dữ liệu từ Database.
- [ ] **Admin Dashboard**: Trang quản lý dành cho Admin để thêm/sửa/xóa phim và thể loại.

---
**Trạng thái hiện tại: 🟢 Ổn định (Stable)**
Project đã sẵn sàng để bước vào giai đoạn nâng cấp chức năng tương tác.
