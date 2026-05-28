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
| ⏯ **Continue Watching** | Lưu tiến độ vào localStorage, auto-resume, thanh tiến độ đỏ |
| 📋 **Watchlist** | Thêm/xóa phim yêu thích, đồng bộ localStorage, filter Phim Lẻ / TV Series |
| 🔐 **Authentication** | Đăng nhập / Đăng ký với ASP.NET Identity, CSRF token |
| 👑 **Admin Dashboard** | CRUD phim đầy đủ, phân trang, thống kê, cascade delete |
| 🎭 **Detail Page** | Mô tả thực từ DB, diễn viên, đạo diễn, gallery lightbox, Similar Movies slider |
| 🌐 **i18n** | Hỗ trợ 8 ngôn ngữ (VI/EN/KO/JA/ZH/FR/ES/TH) |
| 📱 **Responsive** | Tương thích mobile, tablet, desktop |
| 🚫 **Custom 404** | Trang lỗi Netflix-style với animation glitch |

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
```
http://localhost:5241
```

---

## 🔑 Tài Khoản Mặc Định

| Role | Email | Mật khẩu |
|------|-------|-----------|
| Admin | `admin@filmix.vn` | `admin@123` |
| Admin | `superadmin@filmix.vn` | `admin@123` |

---

## 📁 Cấu Trúc Dự Án

```
HUTECH_LTW.FILMIX/
├── Areas/Admin/                  # Khu vực quản trị
│   ├── Controllers/
│   │   ├── DashboardController.cs
│   │   └── ProductController.cs  # CRUD phim
│   └── Views/Product/            # Create, Edit, Delete, Index
├── Controllers/
│   ├── AccountController.cs      # Login / Register / Logout
│   ├── ErrorController.cs        # Custom 404 / 500
│   ├── HomeController.cs
│   ├── MovieController.cs
│   ├── ProductController.cs      # Detail + Similar movies
│   ├── SearchController.cs       # Search + Autocomplete API
│   ├── TVShowsController.cs
│   └── WatchlistController.cs    # Watchlist + API
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs               # Seed roles & admin accounts
├── Models/Entities/
│   └── Entities.cs               # Movie, Episode, Category, MovieImage, ApplicationUser
├── ViewComponents/
│   └── HeroBannerViewComponent.cs
├── Views/
│   ├── Home/Index.cshtml         # Trang chủ + CW section
│   ├── Product/Detail.cshtml     # Chi tiết phim + Similar movies
│   ├── Search/Index.cshtml       # Kết quả tìm kiếm
│   ├── Watchlist/Index.cshtml    # Danh sách yêu thích
│   └── Shared/
│       ├── _Layout.cshtml        # Global nav + search + footer
│       ├── NotFound.cshtml       # 404 Netflix-style
│       ├── General.cshtml        # 500 error page
│       └── Components/HeroBanner/Default.cshtml
└── wwwroot/
    ├── css/
    │   ├── style.css             # Main stylesheet + skeleton
    │   ├── hero-banner.css
    │   ├── continue-watching.css
    │   ├── search.css
    │   ├── hover-preview.css
    │   └── watchlist.css
    └── js/
        ├── netflix-slider.js     # Slider + skeleton loading
        ├── hero-banner.js
        ├── continue-watching.js
        ├── hover-preview.js
        └── auth-state.js
```

---

## 🌿 Git Branches

| Branch | Mô tả |
|--------|-------|
| `main` | Stable production code |
| `feature/netflix-clone-phase4-interactive-ui` | Hero Banner + Slider + CW + Search |

---

## 🏗 Lộ Trình Phát Triển

- [x] Phase 1: Identity + Roles + Authentication
- [x] Phase 2: Admin Dashboard + CRUD
- [x] Phase 3: Hero Banner ViewComponent
- [x] Phase 4: Netflix Slider + Continue Watching + Search
- [x] Phase 5: Detail real data + Similar Movies + Custom 404 + Skeleton

---

## 📝 Ghi Chú

- Video phim dùng sample public domain: [Big Buck Bunny](https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4)
- Trailer URL nhận YouTube Embed format: `https://www.youtube.com/embed/XXXX`
- Watchlist & Continue Watching lưu trong `localStorage` (client-side only)

---

<div align="center">
  Made with ❤️ by HUTECH Students · 2026
</div>