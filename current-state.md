# Nhật Ký Phát Triển Dự Án FILMIX - Cập nhật ngày 22/05/2026

## 2. Các công việc và chỉnh sửa đã hoàn thành hôm nay

### 🔐 Phase 1 & 2 — Identity + Roles (hoàn thành từ session trước)
> Ghi lại đầy đủ để tham chiếu. Các file này đã ổn định, không cần đụng thêm.

- **`untitled1.csproj`** — thêm `Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.0`.
- **`Models/Entities/Entities.cs`** — thêm class `ApplicationUser : IdentityUser` với property `FullName`.
- **`Data/ApplicationDbContext.cs`** — đổi base class thành `IdentityDbContext<ApplicationUser>`, gọi `base.OnModelCreating(modelBuilder)`.
- **`Data/DbSeeder.cs`** — seed 2 role (`Admin`, `User`) và 2 tài khoản admin (`admin1@filmix.com`, `admin2@filmix.com` / `admin@123`), idempotent.
- **`Program.cs`** — đăng ký Identity services (password policy nới lỏng), `ConfigureApplicationCookie` LoginPath = `/Account/Auth`, gọi `DbSeeder.SeedAsync`, thêm `UseAuthentication()`.
- **`Controllers/AccountController.cs`** — login/register/logout qua `SignInManager`, trả JSON cho AJAX fetch.
- **`Views/Account/Auth.cshtml`** — dùng CSRF token + fetch API thay cho form submit thủ công.
- **`Views/Shared/_Layout.cshtml`** — logout chuyển thành form POST; khi không authenticated thì xóa `localStorage['filmix_user']`.

### 🏗 Phase 3 — Admin Area (hoàn thành)

#### Backend
- **`Areas/Admin/Controllers/DashboardController.cs`** — trả về 4 stat: TotalMovies, TotalTVSeries, TotalFilms, TotalUsers + `ViewBag.RecentMovies` (5 phim mới nhất theo Id desc).
- **`Areas/Admin/Controllers/ProductController.cs`** — CRUD đầy đủ với:
  - Search theo title (`?search=`), pagination Skip/Take (PageSize = 10).
  - `TempData["Success"]` sau mỗi Create / Edit / Delete thành công.
  - Edit POST chỉ cập nhật scalar properties, không đụng Episodes/MovieImages.

#### Layout & Routing
- **`Areas/Admin/Views/_ViewStart.cshtml`** — trỏ tới `_AdminLayout.cshtml`.
- **`Areas/Admin/Views/_ViewImports.cshtml`** — import namespace + TagHelpers.
- **`Program.cs`** — area route default controller đổi thành `Dashboard`.

#### Admin Layout (`Areas/Admin/Views/Shared/_AdminLayout.cshtml`)
- Sidebar cố định (fixed) với logo FILMIX Admin, nav links (Dashboard, Quản Lý Phim), footer hiển thị avatar + tên user + nút logout.
- Topbar sticky với tiêu đề trang + link về trang chủ.
- TempData `Success`/`Error` alert hiển thị ngay trong layout.
- Hoàn toàn tách biệt khỏi public `_Layout.cshtml`.

#### Admin CSS (`wwwroot/css/admin.css`)
- Hệ thống design token riêng biệt (`--a-*` variables), không dùng biến từ `site.css`.
- Bao gồm: shell layout, sidebar, topbar, stat cards, toolbar/search, admin table, pagination, form card, delete confirm, badges, action buttons, alerts.
- **Hôm nay bổ sung thêm:** `.page-actions`, `.btn-clear`, `.thumb`, `.no-thumb`, `.title-cell`, `.title-cell__sub`, `.table-section-head`, `.dashboard-grid`.

#### Admin Views
- **`Dashboard/Index.cshtml`** — 4 stat cards + `dashboard-grid` 2 cột: bảng "Phim Mới Nhất" (có thumbnail) và bảng "Truy Cập Nhanh".
- **`Product/Index.cshtml`** — search + nút "+ Thêm" trên **cùng một hàng** (`page-actions`), bảng có cột ảnh thumbnail (`thumb`/`no-thumb`), title kèm genre subtitle (`title-cell`), inline delete confirm qua `window.confirm()`.
- **`Product/Create.cshtml`** — form card sạch với `form-row`, `form-control`, `cat-check-grid`, `check-row`.
- **`Product/Edit.cshtml`** — tương tự Create, pre-select category hiện có.
- **`Product/Delete.cshtml`** — `delete-card` với thông tin phim + nút xác nhận.

### 🔀 Global Layout Switch
- **`Views/_ViewStart.cshtml`** — logic chọn layout theo role:
  - Admin → `~/Areas/Admin/Views/Shared/_AdminLayout.cshtml`
  - Còn lại → `~/Views/Shared/_Layout.cshtml`
  - Area-level `_ViewStart` (trong `Areas/Admin/`) luôn ưu tiên hơn global, nên Admin Area không bị ảnh hưởng.

---

## 3. Các Bug / Rủi ro tiềm ẩn cần theo dõi

### 🔴 Bug có thể xảy ra ngay

#### B1 — Admin thấy Admin Layout trên trang công khai
- **Mô tả:** Do `Views/_ViewStart.cshtml` switch layout theo role, khi admin duyệt `/Product/Detail/1`, `/Movies`, `/TVShows`... sẽ thấy sidebar admin thay vì navbar công khai. Sidebar admin không có link phim, danh mục, v.v.
- **Ảnh hưởng:** Trải nghiệm duyệt phim của admin bị hỏng hoàn toàn.
- **Hướng fix gợi ý:** Chỉ áp dụng admin layout trong Admin Area (đã có `Areas/Admin/Views/_ViewStart.cshtml` lo việc này), bỏ logic switch trong `Views/_ViewStart.cshtml` hoặc thêm điều kiện kiểm tra Area:
  ```razor
  @if (ViewContext.RouteData.Values["area"]?.ToString() == "Admin")
  {
      // không cần set vì area _ViewStart đã xử lý
  }
  // Luôn dùng layout công khai ở đây
  Layout = "~/Views/Shared/_Layout.cshtml";
  ```

#### B2 — localStorage không đồng bộ khi Admin đăng nhập qua server
- **Mô tả:** Navbar công khai dùng `auth-state.js` đọc `localStorage['filmix_user']` để hiển thị trạng thái đăng nhập. Nếu admin login qua form POST thông thường (không qua AJAX fetch trong Auth.cshtml), localStorage sẽ không được ghi → navbar hiện nút "Đăng Nhập" dù đã login.
- **Ảnh hưởng:** Navbar công khai sai trạng thái với admin users.

#### B3 — Xóa phim không cascade Episodes/MovieImages
- **Mô tả:** `DeleteConfirmed` chỉ gọi `_context.Movies.Remove(movie)`. Nếu DB không có `ON DELETE CASCADE` (phụ thuộc vào cấu hình EF `EnsureCreated`), lệnh xóa sẽ throw foreign key violation exception.
- **Ảnh hưởng:** Xóa phim có tập phim hoặc ảnh phụ sẽ crash với HTTP 500.
- **Hướng fix gợi ý:** Kiểm tra EF có tự cascade không; nếu không, load kèm `.Include(m => m.Episodes).Include(m => m.MovieImages)` trước khi remove.

### 🟡 Rủi ro nhỏ / UX Issues

#### B4 — Trang Error hiển thị Admin Layout với admin user
- **Mô tả:** Trang `/Home/Error` cũng dùng `Views/_ViewStart.cshtml`, nên admin thấy trang lỗi với sidebar admin — trông lạ.

#### B5 — Confirm dialog xóa bị block trong một số môi trường
- **Mô tả:** `window.confirm()` trong onclick của nút Xóa có thể bị browser block khi trang chạy trong iframe hoặc một số security policy.
- **Ảnh hưởng:** Nút Xóa submit form ngay không qua confirm.

#### B6 — Build lỗi MSB3027 khi app đang chạy
- **Mô tả:** `dotnet build` fail với file lock error vì app đang giữ `untitled1.exe`. Đây không phải lỗi code — phải stop app trước khi build.
- **Lưu ý:** Razor views (`.cshtml`) không cần restart app — thay đổi view có hiệu lực ngay. Chỉ thay đổi `.cs` mới cần restart.

#### B7 — Pagination mất search term khi navigate
- **Mô tả:** Đã xử lý qua `asp-route-search="@search"` trong pagination links. Tuy nhiên nếu user submit form search mà không có `name="search"` param, trang sẽ về trang 1 với search rỗng.

---

## 4. Các tính năng còn tồn đọng từ trước (chưa xử lý)
- Các nút thêm vào danh sách ở **"Mới & Hot"** và **"Phim Lẻ"** chưa đồng nhất với TV Shows.
- ### 🌐 Thiếu tính năng Chuyển đổi Ngôn ngữ
- Chưa có cơ chế i18n động toàn cục.
- **Trạng thái hiện tại: 🟡 Hoạt động nhưng cần fix B1 trước khi demo** — Admin Area CRUD ổn định, Identity/Roles hoạt động, tuy nhiên bug B1 (admin layout đè lên trang công khai) cần xử lý ngay.