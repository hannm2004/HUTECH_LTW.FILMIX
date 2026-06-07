# 🎬 FILMIX — Netflix Clone

> **Đồ án Lập Trình Web** — HUTECH  
> Xây dựng ứng dụng web xem phim phong cách Netflix với ASP.NET Core 9 MVC + SQL Server

---

## ✨ Tính Năng Chính

| Tính năng | Mô tả |
|-----------|-------|
| 🎬 **Hero Banner** | Video tự động phát, mute/unmute, Ken-Burns zoom, fallback ảnh |
| 🎠 **Netflix Slider** | Chevron arrows, pagination dashes, drag-to-scroll, touch swipe |
| 🔍 **Search & Autocomplete** | Full-text search (Title/Genre/Director/Cast/Description), glassmorphism dropdown, keyboard nav |
| ⏯ **Continue Watching** | Lưu tiến độ vào database & localStorage, tự động phát tiếp |
| 📋 **Watchlist** | Thêm/xóa phim yêu thích, đồng bộ localStorage, filter Phim Lẻ / TV Series |
| 🔐 **Authentication** | Đăng nhập / Đăng ký với ASP.NET Identity, CSRF token bảo mật |
| 👑 **Admin Dashboard v2** | Thống kê nâng cao, KPI tổng quan, biểu đồ trực quan, quản lý người dùng, đơn hàng, gói đăng ký |
| 🎭 **Detail Page** | Mô tả thực từ DB, diễn viên, đạo diễn, Similar Movies slider |
| 🛡️ **Swagger UI** | Phân nhóm tài liệu API (Auth/Cart/Products), hỗ trợ kiểm thử xác thực Bearer Token & Cookie |
| 🔑 **JWT Auth API** | Xác thực REST API bằng JSON Web Token, tách biệt hoàn toàn với Cookie của MVC |
| 🛒 **Cart API** | RESTful Cart API (Session-based) bảo mật bằng JWT |
| 🎬 **Products API** | RESTful CRUD API phim dành cho Admin bảo mật bằng JWT |
| 💳 **Payment System** | Quy trình thanh toán Premium 3D flip card, confetti thành công, quản lý gói cước |
| 🪵 **Audit Logging** | Centralized System Logs ghi lại 12+ hành động của người dùng/Admin |
| 🧠 **Recommendation** | Đề xuất phim cá nhân hóa dựa trên lịch sử xem của người dùng |
| 📱 **Responsive** | Tương thích hoàn toàn trên mobile, tablet, desktop |
| 🚫 **Custom Errors** | Trang lỗi 404/500 Netflix-style với hiệu ứng glitch |

---

## 🛠 Tech Stack

```
Frontend:  HTML5, CSS3 (Vanilla), JavaScript (ES2020+)
Backend:   ASP.NET Core 9 MVC, C# 13
Database:  SQL Server (hoặc MySQL)
ORM:       Entity Framework Core 9 (Code-First)
Auth:      ASP.NET Core Identity
```

---

## 🚀 Cài Đặt & Chạy

### Yêu cầu
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (Express / Developer) **hoặc** MySQL 8+

### Bước 1 — Clone
```bash
git clone https://github.com/hannm2004/HUTECH_LTW.FILMIX.git
cd HUTECH_LTW.FILMIX
```

### Bước 2 — Cấu hình Database
Mở `appsettings.json`, chỉnh chuỗi kết nối:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=filmix_db;Trusted_Connection=True;"
  },
  "DbProvider": "SqlServer"
}
```

> Dùng MySQL thì đổi `DbProvider` thành `"MySql"` và connection string tương ứng.

### Bước 3 — Chạy
```bash
dotnet run
```
Ứng dụng tự động:
- Tạo database nếu chưa có (`EnsureCreated`)
- Seed roles `Admin` / `User`
- Tạo tài khoản admin mặc định

### Bước 4 — Truy cập
* Giao diện chính: `http://localhost:5241`
* Tài liệu API Explorer: `http://localhost:5241/swagger`

---

## 🔑 Tài Khoản Mặc Định

| Role | Email | Mật khẩu | Mô tả |
|------|-------|-----------|-------|
| Admin | `admin1@filmix.com` | `admin@123` | Tài khoản Admin chính (Seed) |
| Admin | `admin2@filmix.com` | `admin@123` | Tài khoản Admin phụ (Seed) |

---

## 📁 Cấu Trúc Dự Án

```
HUTECH_LTW.FILMIX/
├── Areas/Admin/                  # Khu vực quản trị
│   ├── Controllers/
│   │   ├── DashboardController.cs # Thống kê tổng quan & Charts
│   │   ├── UserController.cs      # Quản lý người dùng, phân quyền Admin & Premium
│   │   ├── SubscriptionController.cs # Quản lý & hủy gói cước người dùng
│   │   └── ProductController.cs   # CRUD phim truyền thống
│   └── Views/                    # Dashboard, User, Subscription, Analytics views
├── Controllers/
│   ├── AccountController.cs      # Login / Register / Logout / Profile
│   ├── AuthApiController.cs      # API JWT xác thực (Login & Profile)
│   ├── CartApiController.cs      # API giỏ hàng (RESTful JWT-secured)
│   ├── ProductsApiController.cs  # API CRUD phim (RESTful Admin JWT-secured)
│   ├── ErrorController.cs        # Custom 404 / 500
│   ├── HomeController.cs         # Đề xuất phim cá nhân hóa & Trang chủ
│   ├── MovieController.cs
│   ├── ProductController.cs      # Giao diện Detail + Similar movies
│   ├── SearchController.cs       # Search & Autocomplete API
│   ├── SubscriptionController.cs # Chọn gói cước, Thanh toán 3D Card
│   ├── ViewingHistoryController.cs # Ghi nhận watch progress API
│   └── WatchlistController.cs    # Watchlist + API
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs               # Seed roles & admin accounts
├── Models/
│   ├── Entities/
│   │   ├── ApplicationUser.cs
│   │   └── Entities.cs           # Movie, Episode, Category, SubscriptionPlan, UserSubscription, SystemLog
│   ├── Settings/
│   │   └── JwtSettings.cs        # Cấu hình cài đặt JWT
│   ├── DTOs/
│   │   ├── AuthDtos.cs           # DTOs cho JWT auth
│   │   ├── CartDtos.cs           # Cart request DTOs & ApiResponse dùng chung
│   │   └── MovieDtos.cs          # Products request DTOs
│   └── ViewModels/
│       ├── AdminViewModels.cs    # ViewModels cho dashboard & quản trị
│       └── CartViewModels.cs     # ViewModels cho thanh toán
├── Services/
│   ├── CartService.cs            # Logic giỏ hàng Session
│   ├── IJwtService.cs            # Interface sinh Token JWT
│   ├── JwtService.cs             # Logic Claims & sinh Token JWT
│   ├── OrderService.cs           # Đơn hàng, thanh toán, kích hoạt Premium
│   ├── RecommendationService.cs  # Đề xuất cá nhân hóa & Top analytics
│   └── LogService.cs             # Ghi log hoạt động hệ thống (SystemLog)
├── ViewComponents/
│   └── HeroBannerViewComponent.cs
├── Views/
│   ├── Home/Index.cshtml         # Trang chủ + Recommendations
│   ├── Product/Detail.cshtml     # Chi tiết phim
│   ├── Search/Index.cshtml       # Kết quả tìm kiếm
│   ├── Watchlist/Index.cshtml    # Danh sách yêu thích
│   └── Shared/
│       ├── _Layout.cshtml        # Global nav + search + footer
│       ├── NotFound.cshtml       # 404 Netflix-style
│       └── General.cshtml        # 500 error page
└── wwwroot/
    ├── css/
    │   ├── style.css             # Main stylesheet + skeleton
    │   ├── hero-banner.css
    │   ├── continue-watching.css
    │   ├── search.css
    │   ├── subscription.css      # CSS thanh toán 3D Card
    │   └── admin.css             # Admin dashboard UI stylesheet
    └── js/
        ├── netflix-slider.js     # Slider + skeleton loading
        ├── hero-banner.js
        ├── continue-watching.js  # Tự động gửi tiến độ xem lên server
        └── auth-state.js
```

---

## 🌿 Git Branches

| Branch | Mô tả |
|--------|-------|
| `main` | Stable production code |
| `feature/api-swagger-integration` | RESTful Cart & Products APIs + Swagger UI integration |
| `feature/jwt-authentication` | JWT Bearer Authentication for REST APIs |

---

## 🏗 Lộ Trình Phát Triển

- [x] Phase 1: Identity + Roles + Authentication
- [x] Phase 2: Admin Dashboard + CRUD
- [x] Phase 3: Hero Banner ViewComponent
- [x] Phase 4: Netflix Slider + Continue Watching + Search
- [x] Phase 5: Detail real data + Similar Movies + Custom 404 + Skeleton
- [x] Phase 6: Premium Payment + Subscription Management
- [x] Phase 7: Analytics, System Audit Logs & Recommendation System
- [x] Phase 8: RESTful API Development (Cart & Products) & Swagger UI
- [x] Phase 9: RESTful API JWT Authentication & API Authorization

---

## 📝 Ghi Chú

- Video phim dùng sample public domain: [Big Buck Bunny](https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4)
- Trailer URL nhận YouTube Embed format: `https://www.youtube.com/embed/XXXX`
- Watchlist & Continue Watching lưu trong `localStorage` (client-side only)

---

<div align="center">
  Made with ❤️ by HUTECH Students · 2026
</div>