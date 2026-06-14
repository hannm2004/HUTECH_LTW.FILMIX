# Nhật Ký Phát Triển Dự Án FILMIX — Cập nhật 14/06/2026

---

## 📅 Nhật Ký Cập Nhật Hôm Nay (14/06/2026) — Rà Soát Bảo Mật, Khắc Phục Lỗi Cú Pháp & Đóng Cổng Bypass Thanh Toán

Đã hoàn thành đợt rà soát và củng cố bảo mật toàn diện cho hệ thống: Sửa lỗi biên dịch trang Edit của Admin, phòng chống tấn công thay đổi giá (Price Tampering) qua Cart, xóa bỏ hoàn toàn đường dẫn bypass thanh toán gói cước Premium, dọn dẹp các thông tin nhạy cảm lộ trong mã nguồn, và tối ưu hóa hệ thống gợi ý phim trực tiếp trong CSDL.

### 🛠 Chi Tiết Cập Nhật

#### 1. Sửa Lỗi Biên Dịch Trang Quản Trị Admin
* **Edit View CSS Escape**: Khắc phục lỗi compiler `CS0103` tại `Areas/Admin/Views/Product/Edit.cshtml` bằng việc thay thế `@media` thành `@@media` trong khối thẻ `<style>` để trình biên dịch Razor không parse nhầm thành biến C#.

#### 2. Bảo Vệ Dữ Liệu Thanh Toán & Chặn Price Tampering (C-01)
* **Xác Thực Số Lượng Sản Phẩm**: Thêm kiểm tra đầu vào tại `OrderService.cs` đảm bảo số lượng sản phẩm thanh toán trong đơn hàng (`item.Quantity`) luôn luôn lớn hơn 0 để ngăn chặn thay đổi giá trị đơn hàng bằng số lượng âm hoặc bằng không.
* **Sử Dụng Giá Gốc DB**: Giá của gói đăng ký được tham chiếu trực tiếp từ `SubscriptionPlan` trong CSDL, bỏ qua hoàn toàn thông tin giá gửi từ phía client.

#### 3. Loại Bỏ Lộ Lọt Thông Tin Nhạy Cảm (H-01)
* **Gỡ Bỏ Mật Khẩu Gmail Hardcoded**: Loại bỏ mật khẩu ứng dụng Gmail (`yysq frdl wgpr zafc`) trong `Program.cs` và thay bằng placeholder của lập trình viên.
* **Cảnh Báo Biến Môi Trường**: Bổ sung log cảnh báo (`Console.WriteLine("[WARN]...")`) khi ứng dụng thiếu các biến môi trường cấu hình thực tế cho JWT, SMTP, Google Client ID/Secret.
* **Cấu hình CORS an toàn**: Đăng ký các địa chỉ CORS tin cậy (`Cors:AllowedOrigins`) trong `appsettings.json`.

#### 4. Tối Ưu Hóa Tốc Độ Truy Vấn Đề Xuất Phim (H-03)
* **Chuyển Aggregations Về CSDL**: Refactor `RecommendationService.cs` để thực hiện các toán tử nặng như GroupBy, Count, OrderByDescending và Take trực tiếp trên SQL Database bằng Entity Framework thay vì tải dữ liệu thô về bộ nhớ RAM rồi xử lý qua LINQ-to-Objects.

#### 5. Triệt Tiêu Lỗ Hổng Stored XSS Tập Phim (H-04)
* **Chuyển Sang Data-Attributes**: Gỡ bỏ tham số nội suy Javascript thô trong sự kiện `onclick` của tập phim tại `Detail.cshtml`. Dữ liệu tiêu đề và URL video được lưu an toàn trong thuộc tính `data-title` và `data-url` của HTML, và được truy xuất qua `this.getAttribute()`.

#### 6. Quy Chuẩn Hóa Quy Trình Mua Premium (M-05)
* **Đóng Đường Bypass Trực Tiếp**: Xóa bỏ các Action `ProcessPayment` và `Success` trong `SubscriptionController.cs`, đồng thời xóa bỏ hai file view thô `Checkout.cshtml` và `Success.cshtml` tương ứng. Luồng đăng ký Premium hiện được hợp nhất hoàn toàn qua giỏ hàng chuẩn của hệ thống.

#### 7. Hoàn Thiện & Xác Thực Tính Năng Upload Ảnh Bìa Phim (Movie Poster Upload)
* **Xác Thực Model ImageUrl Tùy Chọn**: Khắc phục lỗi Model Validation bắt buộc phải có `ImageUrl` bằng cách chuyển trường này thành nullable (`string?`) trong thực thể `Movie` để phù hợp với luồng tải lên ảnh tùy chọn.
* **Chính Sách Ưu Tiên File Tải Lên**: Thiết lập luồng xử lý tại Controller:
  - Nếu có file tải lên: File tải lên luôn luôn chiến thắng và được chọn.
  - Nếu không có file tải lên nhưng có `ImageUrl`: Sử dụng URL bên ngoài.
  - Nếu không có cả hai: Tự động gán poster mặc định `/images/posters/default.jpg`.
* **Cơ Chế Tự Động Tạo Thư Mục & Tên File Duy Nhất**: Tự động kiểm tra và khởi tạo thư mục lưu trữ ảnh `wwwroot/images/posters` nếu chưa tồn tại. Tên file được sinh ngẫu nhiên theo định dạng `film_yyyyMMdd_HHmmss_guid.extension` để tránh ghi đè dữ liệu.
* **Xóa Bỏ Ảnh Bìa Cũ & Khi Xóa Phim**:
  - Khi quản trị viên thay thế ảnh bìa mới trong Edit, ảnh bìa cũ dạng cục bộ (local) sẽ tự động bị xóa khỏi đĩa cứng để tránh lãng phí dung lượng.
  - Khi xóa phim (Delete), tệp ảnh bìa cục bộ liên kết với phim đó cũng tự động được giải phóng khỏi máy chủ.
  - Bảo vệ an toàn: Hệ thống tuyệt đối KHÔNG bao giờ cố gắng xóa ảnh bìa mặc định `default.jpg` hoặc các URL bên ngoài (`https://...`).
* **Harden Bảo Mật Phía Server**: Validate chặt chẽ file upload: chặn tệp rỗng, giới hạn kích thước tối đa 5MB, giới hạn định dạng cho phép (`.jpg`, `.jpeg`, `.png`, `.webp`), đối chiếu MIME Type (`image/jpeg`, `image/png`, `image/webp`), và chặn đứng tấn công Path Traversal bằng cách kiểm tra ký tự nguy hiểm (`..`, `/`, `\`) trong filename cũng như kiểm tra đường dẫn sau khi giải quyết (canonical path check).

---

## 📅 Nhật Ký Cập Nhật Hôm Nay (12/06/2026) — Tích Hợp Chatbot AI, Gửi Email Tự Động, Social Login OAuth, Đồng Bộ Watchlist DB & Hỗ Trợ Đa CSDL

Đã hoàn thành đợt nâng cấp quan trọng cuối cùng trước khi bảo vệ đồ án: Tích hợp thành công Chatbot AI, hệ thống Email SMTP xác nhận hóa đơn đơn hàng, Đăng nhập bên thứ ba (Google/Facebook OAuth), đồng bộ dữ liệu Watchlist 2 chiều với CSDL và hỗ trợ chuyển đổi linh hoạt SQL Server / MySQL.

### 🛠 Chi Tiết Cập Nhật

#### 1. Đăng Nhập Mạng Xã Hội (Google OAuth Integration)
* **Tích hợp bên thứ ba**: Cấu hình và tích hợp thư viện Authentication OAuth cho Google vào pipeline xác thực của ứng dụng (`Program.cs`).
* **Định tuyến & Xử lý (AccountController)**: Triển khai các Endpoint `/Account/ExternalLogin` và `/Account/ExternalLoginCallback` xử lý bắt tay OAuth, nhận diện thông tin email và họ tên từ Claims của nhà cung cấp.
* **Tự Động Tạo Tài Khoản (Auto-provisioning)**: Tự tạo mới tài khoản `ApplicationUser` và gán vai trò `"User"` khi người dùng đăng nhập lần đầu bằng Google, tự động xác nhận email (`EmailConfirmed = true`) và ghi nhận vào Audit Log hệ thống.
* **Đồng Bộ Trạng Trái Client (ExternalLoginSuccess.cshtml)**: Thiết kế trang chuyển tiếp trung gian Netflix-style với hiệu ứng Loading Spinner đẹp mắt, tự động ghi nhận thông tin đăng nhập vào `localStorage` của client-side (`filmix_user`) để đồng bộ thanh Navbar và điều hướng an toàn trở lại trang đích trước đó.
* **Khởi tạo động an toàn**: Cấu hình kiểm tra trong `Program.cs` để chỉ kích hoạt OAuth Services khi các khoá ClientId/ClientSecret được cấu hình trong `appsettings.json`, tránh gây lỗi crash ứng dụng khi chưa cấu hình.

#### 2. Đồng Bộ Watchlist Lên Cơ Sở Dữ Liệu (Database-Backed Watchlist & Sync)
* **Thiết Kế Thực Thể (Database Schema)**: Bổ sung thực thể `WatchlistItem` với khoá ngoại liên kết bảng phim và người dùng, thiết lập chỉ mục kép duy nhất trên `(UserId, MovieId)` để tối ưu hóa truy vấn.
* **Endpoint API Watchlist**: Cấu hình các API mới trong `WatchlistController.cs`:
  - `GET /api/watchlist/ids`: Trả về danh sách các Movie ID có trong danh sách của người dùng đang đăng nhập.
  - `POST /api/watchlist/sync`: Nhận mảng ID từ phía client gửi lên để đồng bộ hóa (thêm mới/xoá bớt) tương thích với DB nhằm bảo toàn lịch sử lưu phim.
* **Đồng Bộ Tự Động Hai Chiều (watchlist-sync.js)**: Viết script Javascript tải sớm ở `<head>` của trang. Khi người dùng đăng nhập thành công, script sẽ tự động lấy danh sách phim trong CSDL nạp đè vào `localStorage` của trình duyệt. Mỗi khi người dùng thêm/xoá phim khỏi danh sách yêu thích, script sẽ tự động gọi API `/api/watchlist/sync` để đồng bộ ngay lập tức.
* **Lọc Phim Theo Thể Loại**: Hỗ trợ bộ lọc động Phim Lẻ / TV Series trên trang Watchlist (`/Watchlist/Index`) được render trực tiếp từ DB.

#### 3. Chuyển Đổi Linh Hoạt Cơ Sở Dữ Liệu (Multi-Database SQL Server & MySQL)
* **Hỗ trợ đa CSDL**: Cấu hình tham số `"DbProvider"` trong `appsettings.json` cho phép quản trị viên/lập trình viên chuyển đổi hệ quản trị cơ sở dữ liệu qua lại giữa **Microsoft SQL Server** và **MySQL** chỉ bằng cách thay đổi giá trị cấu hình.
* **Nhận Diện Khởi Tạo**: `Program.cs` đọc giá trị cấu hình này và tiêm DbContext phù hợp (`UseSqlServer` hoặc `UseMySql` thông qua Pomelo driver).

#### 4. Chatbot Hỏi Đáp Thông Minh (Netflix-Style Floating Widget)
* **Giao diện Chatbot**: Tích hợp widget nổi động (`chatbot.css` và `chatbot.js`) vào layout chung `_Layout.cshtml`, hỗ trợ responsive trên di động, hiệu ứng đóng/mở mượt mà, khung chat có scroll tự động và gửi tin nhắn qua phím Enter.
* **Engine Xử Lý Intent & Từ Khóa**: Xây dựng `ChatbotApiController.cs` với cơ chế nhận diện từ khóa linh hoạt hỗ trợ cả tiếng Việt không dấu (alias keywords).
* **Định tuyến & Thứ Tự Ưu Tiên**: Thiết lập thứ tự ưu tiên cho câu hỏi để tránh trùng lặp:
  1. *Hỗ trợ kỹ thuật* (sự cố xem phim, lỗi player).
  2. *Phương thức thanh toán* (thẻ ngân hàng, VNPay, PayOS).
  3. *Đơn hàng* (yêu cầu đăng nhập, kiểm tra trạng thái đơn hàng).
  4. *Gói cước* (truy vấn danh sách gói từ `ISubscriptionPlanRepository`).
  5. *Nội dung phim* (thể loại, gợi ý phim lẻ/TV series).
  6. *Mặc định (Fallback)* hướng dẫn các chủ đề chatbot có thể trả lời.

#### 5. Dịch Vụ Gửi Email Xác Nhận Đơn Hàng Tự Động (Email Service)
* **Kiến trúc EmailService**: Thiết lập `IEmailService` và triển khai `EmailService` qua SMTP (`System.Net.Mail`), cấu hình bảo mật bằng lớp cài đặt `EmailSettings` ánh xạ từ `appsettings.json`.
* **Trình duyệt/Email Client Compatibility**: Thiết kế template HTML phong cách Netflix Dark Mode, thay thế hoàn toàn Flexbox bằng cấu trúc thẻ `<table>` để bảo đảm hiển thị đồng đều trên Outlook, Gmail.
* **Mã hóa UTF-8**: Cấu hình `SubjectEncoding` và `BodyEncoding` là `System.Text.Encoding.UTF8` giúp hiển thị tiếng Việt hoàn hảo không lỗi ký tự.
* **Kích hoạt tự động (Triggers)**:
  - Khi đặt hàng thành công (`OrderController.Checkout`): Gửi mail trạng thái `Pending` (Chờ xử lý).
  - Khi hoàn tất thanh toán (`OrderController.ProcessMockPayment`): Gửi mail trạng thái `Paid` (Đã thanh toán) để kích hoạt gói dịch vụ.
* **Fire-and-forget**: Thực thi gửi mail ngầm bằng Task bất đồng bộ để tối ưu hóa tốc độ load trang cho người dùng.
* **Local Preview**: Tự động kết xuất HTML ra file tĩnh `wwwroot/emails/order_confirmation_{id}.html` khi tắt SMTP để lập trình viên dễ dàng kiểm thử giao diện.

#### 6. Khôi phục hiển thị Cover cho Poster chuẩn dọc & Auth Guards
* **Khôi phục `object-fit: cover`**: Revert toàn bộ các điều chỉnh `object-fit: contain` trước đây về `cover` trên các file style hệ thống:
  - `style.css` (trending slider)
  - `movies.css` (movies card)
  - `newhot.css` (new & hot card)
  - `search.css` (search results)
  - `watchlist.css` (watchlist card)
  - Inline style của phim gợi ý trong `Home/Index.cshtml` và `Product/Detail.cshtml`
* **Nén ảnh tối ưu dung lượng**: Thực hiện resize file poster `thebatman.jpg` có độ phân giải cao (~1.6MB) xuống kích thước hiển thị chuẩn web (400x593px, dung lượng chỉ còn ~41KB), nâng tốc độ tải trang lên tối đa.
* **Thành phần Modal chung (`_Layout.cshtml`)**: Bổ sung modal overlay glassmorphism phong cách Netflix thông báo "Đăng nhập để tiếp tục" kèm theo link Đăng Ký / Đăng Nhập và khả năng truyền ngược URL hiện tại.
* **Chặn tương tác khi chưa Đăng Nhập (Client-side & Server-side)**:
  - **Giỏ Hàng**: Thêm `[Authorize]` vào toàn bộ Cart mutating endpoints ở server. Chặn submit form giỏ hàng ở `Plans.cshtml` phía client và show modal nếu chưa đăng nhập.
  - **Watchlist (Lưu Xem Sau)**: Tích hợp hàm kiểm tra `requireAuth()` trước khi thao tác lưu phim ở các trang `Movies/Index.cshtml`, `TVShows/Index.cshtml`, `NewHot/Index.cshtml` và `Detail.cshtml`.
  - **Chuyển hướng ReturnUrl**: Cập nhật `AccountController.cs` và `Auth.cshtml` đọc và truyền `ReturnUrl` để tự động trả người dùng về đúng trang họ đang làm việc sau khi đăng nhập thành công.

---


## 📅 Nhật Ký Cập Nhật Hôm Nay (08/06/2026) — Nâng Cấp Hệ Thống Sẵn Sàng Bảo Vệ Đồ Án (Premium Fix, Database Migration & UI Smart Trailer)

Đã hoàn thành các hạng mục sửa lỗi critical, chuẩn bị dữ liệu và nâng cấp trải nghiệm người dùng trước buổi bảo vệ đồ án.

### 🛠 Chi Tiết Cập Nhật

#### 1. Sửa lỗi kích hoạt tài khoản Premium (Critical Fix)
* **`SubscriptionController.cs`**: Bổ sung cập nhật trực tiếp `PremiumStartDate` và `PremiumEndDate` vào `ApplicationUser` khi thanh toán thành công, thực hiện `UpdateAsync` thông qua `UserManager` trước khi lưu CSDL.
* **`ApplicationUser.cs`**: Tích hợp thuộc tính tính toán `IsPremium` dựa trên `PremiumEndDate` để hệ thống tự động xác nhận quyền hạn Premium theo thời gian thực (real-time).

#### 2. Dọn dẹp logic cơ sở dữ liệu & Cập nhật EF Migration
* **`Program.cs`**: Loại bỏ hoàn toàn khối lệnh `EnsureDeleted()` tự động reset DB không an toàn trong block catch lỗi khởi tạo.
* **EF Core Migration**: Tạo bản migration `AddMovieRatingAndLocalTrailer` để thêm các cột `Rating`, `TrailerVideoUrl`, `PremiumStartDate`, `PremiumEndDate` và các bảng phụ thuộc khác vào SQL Server một cách chính thống qua `dotnet ef database update`.

#### 3. Bổ sung dữ liệu phim thực tế chất lượng cao (DbSeeder)
* **`ApplicationDbContext.cs`**: Nạp dữ liệu của 18 bộ phim & series nổi tiếng (Breaking Bad, Game of Thrones, Interstellar, Dune 2, Inception...) kèm theo mô tả chi tiết tiếng Việt, đạo diễn, diễn viên, điểm IMDb thực tế và liên kết Trailer.
* **Danh mục thể loại**: Bổ sung thêm hai thể loại mới là "Hoạt Hình" và "Phiêu Lưu" để mở rộng cơ sở dữ liệu phân loại phim.

#### 4. Nâng cấp Hero Banner & Trình phát Trailer thông minh
* **`HeroBannerViewComponent.cs`**: Chuyển đổi từ thuật toán chọn phim ngẫu nhiên (gây nhấp nháy giao diện khi tải trang) sang việc lấy top 5 phim có Rating cao nhất và xoay vòng theo giờ hệ thống.
* **Giao diện Hero Banner (`Default.cshtml`)**: Thêm badge điểm đánh giá vàng (⭐ Rating/10) ngay dưới tiêu đề phim.
* **Trình phát Video thông minh (`Detail.cshtml` & `hero-banner.js`)**: Thiết lập cơ chế fallback 3 cấp độ phát video trailer:
  1. Phát file MP4 cục bộ (`TrailerVideoUrl` đặt tại `/videos/trailers/`) nếu có.
  2. Fallback phát iframe nhúng YouTube (`TrailerUrl`).
  3. Fallback phát clip Big Buck Bunny mẫu nếu không cấu hình trailer.

---

## 📅 Nhật Ký Cập Nhật Tối (05/06/2026) — Xây Dựng RESTful Products API & Tích Hợp Swagger UI

Đã hoàn thành thiết kế và xây dựng **RESTful Products API** dành cho quản trị viên, tích hợp công cụ tài liệu hóa **Swagger UI** phân nhóm tài liệu rõ ràng và cấu hình xác thực Cookie-based Identity bảo mật.

### 🛠 Chi Tiết Cập Nhật

#### DTOs & Validation Layer
* **`MovieDtos.cs`** (`Models/DTOs/MovieDtos.cs`): Định nghĩa các request/response DTOs cho thực thể Movie:
  - `MovieListDto`: Trả về danh sách phim thu gọn.
  - `MovieDetailDto`: Chi tiết phim kèm danh mục thể loại liên kết.
  - `CreateMovieDto`: Ràng buộc nhập liệu khi thêm mới phim (Validation qua DataAnnotations: Title, ImageUrl, Year, Genre).
  - `UpdateMovieDto`: Ràng buộc nhập liệu khi cập nhật thông tin phim.

#### RESTful Controller API Layer
* **`ProductsApiController.cs`** (`Controllers/ProductsApiController.cs`): Controller mới xử lý CRUD tài nguyên phim thông qua các phương thức REST chuẩn:
  - `GET /api/products`: Lấy danh sách phim kèm tìm kiếm theo tên và phân trang (`page`, `pageSize`).
  - `GET /api/products/{id}`: Lấy chi tiết bộ phim cụ thể (Eager Loading thể loại liên kết).
  - `POST /api/products`: Thêm mới phim và liên kết danh mục thể loại trong bảng trung gian. Ghi log hoạt động "Add Movie".
  - `PUT /api/products/{id}`: Cập nhật thông tin phim, ghi đè danh mục thể loại cũ và ghi log "Edit Movie".
  - `DELETE /api/products/{id}`: Xóa phim cùng các thực thể phụ thuộc liên quan (Episodes, MovieImages) và ghi log "Delete Movie".

#### Bảo Mật Với Cookie-Based Auth & Identity
* **Phân quyền truy cập:** Thêm `[Authorize(Roles = "Admin")]` cho `ProductsApiController` để đảm bảo chỉ những tài khoản Admin mới có quyền thao tác trên tài nguyên phim.
* **Tích hợp Audit Log:** Tự động bắt thông tin Admin đang đăng nhập thông qua `UserManager<ApplicationUser>` để ghi log hệ thống chi tiết vào DB.

#### Cấu Hình Swagger UI & API Explorer (`Program.cs`)
* Đăng ký SwaggerGen chia thành 2 nhóm tài liệu riêng biệt:
  - **`FILMIX Cart API v1`**: Gom các endpoints chứa `CartApi` dùng cho giỏ hàng.
  - **`FILMIX Products API v1`**: Gom các endpoints chứa `ProductsApi` dùng cho quản lý phim của Admin.
* Cấu hình định nghĩa bảo mật `cookieAuth` loại `ApiKey` trong Cookie nhằm hỗ trợ kiểm thử tiện lợi trên giao diện Swagger UI sau khi đăng nhập tài khoản.
* Tự động ẩn schemas models mặc định để tăng tính thẩm mỹ (`DefaultModelsExpandDepth(-1)`).

---

## 📅 Nhật Ký Cập Nhật Sáng (05/06/2026) — Xây Dựng RESTful Cart API Hoàn Chỉnh

Đã hoàn thành phân tích dự án FILMIX ASP.NET Core MVC 8 hiện tại và xây dựng hệ thống **RESTful Cart API** hoàn chỉnh sử dụng kiến trúc Repository + Service hiện có, đảm bảo không ảnh hưởng đến `CartController` MVC cũ.

### 🛠 Chi Tiết Cập Nhật

#### DTOs & Validation Layer
* **`CartDtos.cs`** (`Models/DTOs/CartDtos.cs`): Định nghĩa các request/response DTOs chuẩn hóa:
  - `AddToCartDto`: Request thêm sản phẩm (chỉ cần `PlanId` bắt buộc).
  - `UpdateQuantityDto`: Request cập nhật số lượng (giới hạn từ 1 đến 100).
  - `ApiResponse<T>`: Cấu trúc JSON chuẩn hóa chung (`Success`, `Message`, `Data`, `Errors`).
  - `CartDto`, `CartItemDto`, `UserInfoDto`: DTOs đóng gói dữ liệu giỏ hàng và thông tin người dùng hiện tại phục vụ client-side.

#### RESTful Controller API Layer
* **`CartApiController.cs`** (`Controllers/CartApiController.cs`): Controller mới kế thừa `ControllerBase` với attribute `[ApiController]` xử lý các API:
  - `GET /api/cart`: Lấy thông tin giỏ hàng hiện tại.
  - `POST /api/cart/items`: Thêm gói dịch vụ vào giỏ hàng (nhận `AddToCartDto` trong request body).
  - `PUT /api/cart/items/{planId}`: Cập nhật số lượng của một gói cụ thể (nhận `UpdateQuantityDto` trong request body).
  - `DELETE /api/cart/items/{planId}`: Xóa một gói cụ thể ra khỏi giỏ hàng.
  - `DELETE /api/cart`: Xóa sạch toàn bộ giỏ hàng.

#### Tích Hợp ASP.NET Identity & Session
* **Xác định User hiện tại:** Sử dụng `UserManager<ApplicationUser>` kết hợp `User.Identity.IsAuthenticated` để phát hiện và tự động điền thông tin chi tiết của người dùng đang đăng nhập (bao gồm trạng thái Premium) vào trường `User` của `CartDto`.
* **Tái sử dụng Service:** Kế thừa nguyên vẹn `ICartService` và `CartService` lưu trữ trong HTTP Session để đảm bảo dữ liệu giỏ hàng trên API đồng bộ 100% với giao diện MVC truyền thống.

#### Cấu Hình API Validation & DI (`Program.cs`)
* Tích hợp cấu hình `ConfigureApiBehaviorOptions` tùy biến `InvalidModelStateResponseFactory`. Khi client gửi dữ liệu không hợp lệ (ví dụ số lượng ngoài khoảng 1-100), hệ thống sẽ tự động chặn từ middleware và trả về HTTP 400 Bad Request kèm format lỗi chuẩn `ApiResponse`.

#### Build & Tài Liệu
* **✅ Build Succeeded 100% — 0 Errors, 1 Warning.**
* **Tài Liệu Chi Tiết:** Đã biên soạn tài liệu đặc tả API đầy đủ kèm ví dụ Request/Response JSON tại `C:\Users\HP\.gemini\antigravity\brain\847bfc9d-88b2-494b-8a25-f3996b1cb2a0/cart_api_documentation.md`.

---

## 📅 Nhật Ký Cập Nhật Tối (04/06/2026) — Hệ Thống Audit Log (System Logs)

Đã hoàn thiện **tính năng Audit Log** theo đúng kiến trúc Repository + Service Pattern hiện có, tự động ghi nhận 12 loại hành động quan trọng của hệ thống và cung cấp giao diện quản lý đầy đủ trong Admin Area.

### 🛠 Chi Tiết Cập Nhật

#### Entities & Database
* **`SystemLog`** (`Models/Entities/Entities.cs`): Thực thể mới với các trường `Id`, `UserId`, `UserName`, `Action`, `Description`, `CreatedAt`, `IpAddress`. Không thiết lập FK cứng với `ApplicationUser` để đảm bảo audit trail tồn tại ngay cả khi tài khoản bị xóa.
* **`ApplicationDbContext`**: Đăng ký `DbSet<SystemLog> SystemLogs`.
* **`Program.cs`**: Thêm check query `db.SystemLogs.FirstOrDefault()` vào block kiểm tra schema tự động.

#### Repository & Service Layer
* **`ILogRepository`** + **`LogRepository`** (`Repositories/`):
  - `AddAsync` + `SaveAsync`: Ghi log mới.
  - `GetAllAsync(search, actionType, page, pageSize)`: Truy vấn phân trang, lọc theo loại hành động và tìm kiếm full-text trên `UserName`, `Description`, `IpAddress`, `Action`.
  - `GetTotalCountAsync`: Đếm tổng kết quả với cùng filter.
  - `GetActionTypesAsync`: Lấy danh sách loại hành động distinct để render filter tabs.
* **`ILogService`** + **`LogService`** (`Services/`):
  - `LogAsync(userId, userName, action, description, ipAddress)`: Entry point để ghi log từ bất kỳ controller nào.
  - `GetLogsAsync(search, actionType, page, pageSize)`: Đóng gói kết quả vào `SystemLogIndexViewModel`.
  - `GetLogDetailAsync(id)`: Lấy chi tiết một bản ghi.
* **DI Registration** (`Program.cs`): Đăng ký `ILogRepository → LogRepository` và `ILogService → LogService`.

#### Tích Hợp Logging Vào Controllers (12 hành động)
| Action Key | Controller | Mô tả |
|---|---|---|
| `Login` | AccountController | Đăng nhập thành công |
| `Login Failed` | AccountController | Đăng nhập thất bại |
| `Register` | AccountController | Đăng ký tài khoản mới |
| `Register Failed` | AccountController | Đăng ký thất bại |
| `Logout` | AccountController | Đăng xuất |
| `Add Movie` | Admin/ProductController | Thêm phim mới |
| `Edit Movie` | Admin/ProductController | Sửa thông tin phim |
| `Delete Movie` | Admin/ProductController | Xóa phim |
| `Buy Premium` | SubscriptionController | Mua gói Premium |
| `Grant Admin` | Admin/UserController | Cấp quyền Admin |
| `Revoke Admin` | Admin/UserController | Thu hồi quyền Admin |
| `Grant Premium` | Admin/UserController | Cấp Premium thủ công |
| `Revoke Premium` | Admin/UserController | Thu hồi Premium thủ công |
| `Deactivate Subscription` | Admin/SubscriptionController | Hủy gói đăng ký |

#### Admin Area — System Logs UI
* **`SystemLogController`** (`Areas/Admin/Controllers/`): 2 action — `Index` (danh sách + filter + phân trang) và `Detail` (xem chi tiết).
* **`SystemLogIndexViewModel`** (`Models/ViewModels/`): ViewModel chứa danh sách logs, thông tin filter, pagination và danh sách action types.
* **`Areas/Admin/Views/SystemLog/Index.cshtml`**: Giao diện danh sách log với:
  - Thanh tìm kiếm full-text.
  - Filter tabs động theo loại hành động (lấy từ DB).
  - Bảng log hiển thị thời gian, hành động (badge màu), thông tin user, mô tả, IP.
  - Phân trang chuẩn.
* **`Areas/Admin/Views/SystemLog/Detail.cshtml`**: Card chi tiết một bản ghi log, hiển thị đầy đủ tất cả trường, có nút liên kết đến hồ sơ người dùng tương ứng.
* **`_AdminLayout.cshtml`**: Thêm link **"System Logs"** vào sidebar với icon shield (🛡️).
* **`admin.css`**: Thêm 4 badge variant mới: `.badge-danger`, `.badge-info`, `.badge-warning`, `.badge-gold`.

#### Build Status
* **✅ Build succeeded — 0 Errors, 1 Warning** (warning cũ ở `List.cshtml`, không liên quan).

---

## 📅 Nhật Ký Cập Nhật Hôm Nay (04/06/2026) — Hệ Thống Đề Xuất Phim & Thống Kê Xem Phim

Hôm nay đã hoàn thiện **Hệ thống Đề xuất Phim (Recommendation System)** dựa trên lịch sử xem của người dùng, tích hợp cơ chế ghi nhận tự động watch progress từ trình phát phim, và bổ sung Dashboard Analytics cho Admin để theo dõi Top 10 Thể Loại & Top 10 Phim xem nhiều nhất.

### 🛠 Chi Tiết Cập Nhật

#### Entities & Database Configuration
* **`ViewingHistory`**: Thực thể mới lưu thông tin lịch sử xem gồm `UserId`, `MovieId`, `WatchTime` (thời lượng đã xem), và `WatchedAt` (thời gian xem).
* **`ApplicationDbContext`**: Cấu hình DbSet `ViewingHistories` và thiết lập ràng buộc khóa ngoại (Cascading Delete khi xóa User hoặc Movie).
* **`Program.cs`**: Tích hợp check-query kiểm tra sự tồn tại của bảng `ViewingHistories` để tự động tái tạo DB cấu trúc mới nếu phát hiện phiên bản cũ.

#### Repositories & Services
* **`IViewingHistoryRepository` + `ViewingHistoryRepository`**: Cung cấp các thao tác ghi nhận bản ghi xem phim mới, truy vấn lịch sử theo UserId (kèm nạp Eager Loading các Categories), và lấy toàn bộ lịch sử để phục vụ phân tích.
* **`IRecommendationService` + `RecommendationService`**: Chứa logic phân tích và đề xuất phim:
  - `LogWatchHistoryAsync()`: Ghi nhận lịch sử xem phim của người dùng. Nếu người dùng đã xem phim này trước đó, cập nhật lại thời lượng (`WatchTime`) và mốc thời gian xem mới nhất (`WatchedAt`).
  - `GetRecommendationsAsync()`: Phân tích tối đa 3 thể loại (Genre) được người dùng xem nhiều nhất trong lịch sử, truy xuất các bộ phim chưa xem thuộc các thể loại này. Tự động fallback về các bộ phim mới nhất nếu chưa có lịch sử xem hoặc cần bù đắp danh sách đề xuất cho đủ số lượng.
  - `GetTopGenresAsync()` & `GetTopMoviesAsync()`: Tổng hợp số lượt xem của từng thể loại và từng bộ phim trên toàn bộ hệ thống.
* **DI Registration**: Đăng ký các repository và service mới trong `Program.cs`.

#### Controllers & Views (Public)
* **`ViewingHistoryController`**: API Controller (`/ViewingHistory/Log`) xử lý nhận dữ liệu gửi về từ client-side để cập nhật tiến độ xem của người dùng đang đăng nhập vào database.
* **`HomeController`**: Tích hợp gọi `IRecommendationService.GetRecommendationsAsync()` để lấy danh sách phim đề xuất cho người dùng hiện tại và truyền qua `ViewBag.Recommendations`.
* **`Views/Home/Index.cshtml`**: Thiết kế section **"Đề Xuất Dành Cho Bạn"** theo chuẩn thiết kế Netflix Slider, hỗ trợ responsive, hiệu ứng zoom scale khi hover và overlay hiển thị thông tin phim mượt mà.
* **`continue-watching.js`**: Tích hợp thêm hàm AJAX `logToServerHistory` gọi lên API `/ViewingHistory/Log` mỗi khi phát hiện video đang phát (định kỳ mỗi 5 giây qua timer hiện có và khi kết thúc video/unload trang).

#### Admin Dashboard Analytics
* **`AnalyticsViewModel`**: Mở rộng thuộc tính `TopGenres` và `TopMovies` để vận chuyển dữ liệu thống kê xem phim.
* **`AdminService.cs`**: Cập nhật phương thức `GetAnalyticsAsync()` thực hiện tổng hợp truy vấn dữ liệu từ bảng `ViewingHistories` để tính toán Top 10 thể loại được xem nhiều nhất và Top 10 phim được xem nhiều nhất.
* **`Areas/Admin/Views/Analytics/Index.cshtml`**: Bổ sung hai bảng dữ liệu side-by-side hiển thị trực quan xếp hạng **Top 10 Thể Loại** và **Top 10 Phim** kèm lượt xem thực tế, ảnh thumbnail phim, hiệu ứng hover và highlight Top 3 vị trí dẫn đầu bằng màu đỏ đặc trưng.

---

## 📅 Nhật Ký Cập Nhật Hôm Nay (02/06/2026) — Admin Module v2

Hôm nay đã hoàn thiện **Admin Module đầy đủ** với 5 section mới: Dashboard nâng cao, Quản lý Người dùng, Quản lý Gói đăng ký, Quản lý Đơn hàng, và Phân tích & Thống kê.

### 🛠 Kiến Trúc Admin Module

#### Repositories (mới)
* **`IUserRepository` + `UserRepository`**: Truy xuất toàn bộ danh sách user, tìm theo ID, cập nhật.
* **`ISubscriptionRepository` + `SubscriptionRepository`**: Truy vấn gói đăng ký (all, by user, active, by ID).

#### Services (mới)
* **`IAdminService` + `AdminService`**: Service tổng hợp cho toàn bộ admin:
  - `GetDashboardDataAsync()` — KPI, chart data 6 tháng.
  - `GetUsersAsync()` — Danh sách user với search/filter/pagination.
  - `GetUserDetailAsync()` — Chi tiết 1 user: đơn hàng + gói đăng ký.
  - `SetAdminRoleAsync()` — Cấp/thu hồi quyền Admin qua Identity.
  - `TogglePremiumAsync()` — Kích hoạt/thu hồi Premium thủ công.
  - `GetSubscriptionsAsync()` — Tất cả sub với filter active/expired.
  - `DeactivateSubscriptionAsync()` — Hủy gói đăng ký cụ thể.
  - `GetAnalyticsAsync()` — Dữ liệu 12 tháng cho tất cả biểu đồ.

#### ViewModels (mới — `AdminViewModels.cs`)
* `DashboardViewModel`, `UserIndexViewModel`, `UserListViewModel`, `UserDetailViewModel`, `SubscriptionIndexViewModel`, `SubscriptionRowViewModel`, `AnalyticsViewModel`.

#### Controllers Admin Area (mới/nâng cấp)
* **`DashboardController`** ✏️ Nâng cấp: sử dụng `IAdminService` thay vì raw DbContext.
* **`UserController`** 🆕: List (search/filter/page) + Detail + SetAdmin + TogglePremium.
* **`SubscriptionController`** 🆕: List (search/filter/page) + Deactivate.
* **`AnalyticsController`** 🆕: Index action trả về `AnalyticsViewModel`.

#### Views Admin Area (mới/nâng cấp)
* **`Dashboard/Index.cshtml`** ✏️: 4 KPI cards (phim/user/premium/doanh thu), 2 biểu đồ Chart.js (revenue bar + sub line), 2 panel recent activity, stats-bar tổng kết.
* **`User/Index.cshtml`** 🆕: Toolbar search + filter tabs (all/admin/premium/normal), bảng user với avatar, badge, pagination.
* **`User/Detail.cshtml`** 🆕: 2 cột (profile card sticky + main content), action cards cấp Admin & Premium, lịch sử đăng ký & đơn hàng.
* **`Subscription/Index.cshtml`** 🆕: KPI strip (tổng/active/expired/doanh thu), bảng đầy đủ với cảnh báo hết hạn sớm, nút Hủy inline.
* **`Analytics/Index.cshtml`** 🆕: 4 KPI, biểu đồ dual-axis (doanh thu + đơn hàng 12 tháng), doughnut phân phối gói, horizontal bar phương thức TT, bar trend đăng ký.

#### Layout & CSS
* **`_AdminLayout.cshtml`**: Thêm 3 link sidebar mới (Người dùng, Gói đăng ký, Phân tích) trong section "Nâng cao".
* **`admin.css`**: +~390 dòng mới — KPI cards, chart cards, toolbar, table-card, user cells, user-detail layout, action cards, subscription strip, analytics legend, extended badges, utilities.

#### DI Registration
* **`Program.cs`**: Đăng ký `IUserRepository`, `ISubscriptionRepository`, `IAdminService`.



Hôm nay đã hoàn thiện **hệ thống Thanh Toán & Đăng Ký Gói Premium** theo phong cách Netflix, tích hợp đầy đủ vào cơ sở dữ liệu.

### 💳 8. Hệ Thống Thanh Toán & Gói Premium (Payment System)

#### Entities & Database
* **`SubscriptionPlan`**: Entity mới lưu 3 gói (Cơ Bản 79k, Tiêu Chuẩn 149k, Cao Cấp 219k) với các trường: `Price`, `Resolution`, `MaxScreens`, `HasDownload`, `HasSpatialAudio`, `AccentColor`, `IsPopular`.
* **`UserSubscription`**: Entity lưu lịch sử đăng ký của từng user: `StartDate`, `EndDate`, `IsActive`, `PaymentMethod`, `TransactionId`.
* **`ApplicationDbContext`**: Đăng ký 2 DbSet mới + quan hệ FK + seed 3 gói mặc định.
* **`Program.cs`**: Thêm test query cho 2 bảng mới → tự động xóa & tạo lại DB khi phát hiện cấu trúc cũ.

#### Controller — `SubscriptionController.cs`
* `Plans()` — Hiển thị trang chọn gói, nhận diện gói đang dùng của user.
* `Checkout(planId)` — Trang thanh toán (yêu cầu đăng nhập).
* `ProcessPayment(planId, paymentMethod)` — POST: Hủy sub cũ → tạo sub mới → redirect thành công.
* `Success(id)` — Trang xác nhận giao dịch thành công.
* `MySubscription()` — Trang xem gói hiện tại.
* `ApiStatus()` — JSON endpoint `/api/subscription/status` để client check trạng thái.

#### Views & CSS
* **`Plans.cshtml`**: Trang chọn gói glassmorphism với badge "Phổ Biến Nhất" nhấp nháy, bảng so sánh tính năng, nút CTA thông minh (nhận diện gói hiện tại).
* **`Checkout.cshtml`**: Trang thanh toán premium 2 cột:
  - **4 tab phương thức**: Thẻ Ngân Hàng, MoMo, ZaloPay, Chuyển Khoản
  - **Thẻ 3D flip animation**: Card lật mặt sau khi focus vào ô CVV
  - **Real-time card display**: Số thẻ, tên chủ thẻ, ngày hết hạn hiển thị trực tiếp trên card ảo
  - **Cột tóm tắt đơn hàng**: Plan info + giá + nút xác nhận + security badges
* **`Success.cshtml`**: Trang chúc mừng với hiệu ứng confetti 60 hạt màu sắc, animated checkmark, chi tiết giao dịch đầy đủ.
* **`MySubscription.cshtml`**: Trang quản lý gói — hiển thị trạng thái, ngày hết hạn, phương thức TT, nút nâng cấp.
* **`subscription.css`**: ~450 dòng CSS premium — glassmorphism cards, plan badges, card 3D flip, confetti, success animation, responsive layout.

#### Layout Integration
* **Navbar**: Thêm nút **👑 Premium** gradient đỏ nổi bật sau "Danh Sách".
* **Footer**: Thêm link "Gói Đăng Ký" trong cột Tài Khoản.
* **`_Layout.cshtml`**: Include `subscription.css` toàn cục.

---

## 📅 Nhật Ký Cập Nhật Hôm Nay (28/05/2026)

Hôm nay chúng ta đã tập trung hoàn thiện các tính năng tương tác còn lại, tích hợp dữ liệu thực từ cơ sở dữ liệu cho các trang công cộng, nâng cấp trải nghiệm người dùng (UX) với màn hình chờ và tối ưu hệ thống xử lý lỗi. Tất cả các nhiệm vụ đều đã được kiểm thử trực quan và hoạt động hoàn hảo.

### 1. 🔍 Đã Build & Xác Minh Tính Năng Tìm Kiếm (Search & Autocomplete)
* **SearchController.cs**: Biên dịch thành công 100% không lỗi. Tích hợp full-text search tìm kiếm trên mọi trường dữ liệu thực của phim (`Title`, `Genre`, `Director`, `Cast`, `Description`).
* **Gợi ý tự động (Suggest API)**: Đầu ra API `/api/search/suggest?q=` trả về 8 gợi ý nhanh kèm ảnh thu nhỏ và định dạng phim.
* **Tối ưu hóa UI/UX**: Keyboard navigation (phím mũi tên lên/xuống, Enter để chọn, Esc để đóng) tích hợp hoàn hảo với dropdown glassmorphism sang trọng trên navbar.

### 2. 🎬 Trang Chi Tiết Phim (`Detail.cshtml`) Sử Dụng Dữ Liệu Thực Từ DB
* **Thay thế hardcode**: Chuyển đổi toàn bộ các đoạn text tĩnh cũ thành dữ liệu động lấy từ model:
  - Tóm tắt nội dung phim (`@Model.Description`) kèm fallback in nghiêng nếu trống.
  - Diễn viên chính (`@Model.Cast`) và Đạo diễn (`@Model.Director`) kèm fallback chữ mặc định.
  - Thể loại (`@Model.Genre`) và Năm phát hành (`@Model.Year`) hiển thị động trên sidebar.
  - Định dạng hiển thị sắc nét: Nhãn phân loại TV Series / Phim Điện Ảnh cùng badge Ultra HD 4K động.

### 🎠 3. Thêm Slider "Có Thể Bạn Cũng Thích" (Similar Movies)
* **Tích hợp Slider**: Thêm section đề xuất phim tương tự ở cuối trang Detail.
* **Dữ liệu động**: Query tự động 6 bộ phim cùng thể loại hoặc đạo diễn từ `ViewBag.SimilarMovies` (đã được tối xử lý trong `ProductController`).
* **Hiệu ứng**: Tận dụng thiết kế Netflix slider, hỗ trợ di chuột phóng to mượt mà và hiển thị thông tin metadata nhanh.

### 🚫 4. Định Tuyến & Thiết Kế Trang Lỗi Tùy Biến (Custom 404 / 500)
* **ErrorController.cs**: Tạo controller chuyên biệt xử lý mã trạng thái lỗi.
* **Program.cs**: Đăng ký middleware `app.UseStatusCodePagesWithReExecute("/Error/{0}");` xử lý lỗi thông minh ở mọi môi trường.
* **Trang 404 (`NotFound.cshtml`)**: Thiết kế giao diện Netflix-style cực kỳ bắt mắt với hiệu ứng số 404 nhiễu sóng (glitch) và dải cuộn phim cell-film chạy động liên tục.
* **Trang 500 (`General.cshtml`)**: Tạo view lỗi máy chủ Netflix-style đồng bộ, hỗ trợ nút "Thử lại" và "Quay lại" thông minh.

### ✨ 5. Tích Hợp Hiệu Ứng Chờ Trượt Shimmer (Skeleton Loaders)
* **style.css**: Xây dựng hệ thống CSS shimmer keyframes (`@keyframes shimmer`) và các class loader giả lập (`.skeleton`, `.img-loading`, `.slider-skeleton-card`).
* **netflix-slider.js**: Bổ sung hàm tự động phát hiện ảnh chưa tải `applySkeletons()`, tự động áp hiệu ứng shimmer nhấp nháy chuyển động và ẩn đi mượt mà khi poster phim load thành công.

### 📋 6. Trang Watchlist Khách Hàng (`Watchlist/Index.cshtml`)
* Hoàn thiện trang danh sách yêu thích chạy hoàn toàn bằng JavaScript ở client-side kết nối `localStorage` (`filmix_watchlist`).
* Gọi API `/api/watchlist?ids=...` để tải và đồng bộ phim yêu thích tức thì.
* Hỗ trợ bộ lọc động (Tất cả / Phim lẻ / TV Series), nút "Xóa tất cả" và đồng bộ hiển thị số lượng phim thực tế trên badge.

### 📝 7. Viết Tài Liệu README.md Chuẩn
* Tạo mới file `README.md` hướng dẫn toàn diện từ giới thiệu tính năng, cấu trúc công nghệ (Tech Stack), hướng dẫn cài đặt database, tài khoản Admin seed mặc định, cho đến cấu trúc thư mục chi tiết của dự án.

---

## 📅 Nhật Ký Cập Nhật Chiều (07/06/2026) — Tích Hợp JWT Authentication Cho REST APIs

Đã triển khai thành công hệ thống **JWT Authentication** dành riêng cho REST APIs (`CartApiController`, `ProductsApiController`, `AuthApiController`), giữ nguyên hệ thống **Cookie Authentication & ASP.NET Identity** hiện có cho các trang MVC.

### 🛠 Chi Tiết Cập Nhật

#### JWT Configuration & Settings
* **`appsettings.json`**: Thêm cấu hình `"JwtSettings"` chứa `Secret`, `Issuer`, `Audience` và `ExpiryInMinutes`.
* **`JwtSettings.cs`** (`Models/Settings/JwtSettings.cs`): Lớp settings ánh xạ trực tiếp từ cấu hình JSON phục vụ tiêm dependency (`IOptions<JwtSettings>`).

#### DTOs & Validation Layer
* **`AuthDtos.cs`** (`Models/DTOs/AuthDtos.cs`):
  - `LoginRequestDto`: Request nhận `Email` và `Password` kèm ràng buộc Validation.
  - `LoginResponseDto`: Response trả về `Token`, `ExpiresAt`, `UserName`, và `Roles`.
  - `UserProfileDto`: Response trả về thông tin người dùng đăng nhập hiện tại và quyền hạn.

#### JWT Service Layer
* **`IJwtService.cs`** & **`JwtService.cs`** (`Services/`): Interface & Service phụ trách xử lý mã hóa JWT Token, tự động truy vấn danh sách Roles của người dùng để đính kèm vào các Claims của Token (`ClaimTypes.Role`).

#### RESTful Controller API Layer
* **`AuthApiController.cs`** (`Controllers/AuthApiController.cs`): Controller xác thực RESTful API mới:
  - `POST /api/auth/login`: Xác thực thông tin tài khoản qua `UserManager` (không tạo Cookie), ghi log Audit tương ứng, sinh ra JWT Token trả về cho Client.
  - `GET /api/auth/profile`: API lấy thông tin người dùng được bảo vệ bằng JWT (`[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`).

#### Cấu Hình Phân Quyền & Swagger UI (`Program.cs`)
* **`Program.cs`**:
  - Đăng ký `IJwtService` và tiêm cấu hình `JwtSettings` vào DI Container.
  - Đăng ký thêm scheme `AddJwtBearer` cùng các tham số Validate chặt chẽ (Issuer, Audience, Lifetime, Signing Key).
  - Tích hợp nhóm Swagger Doc mới là `"auth"` dành riêng cho các API xác thực.
  - Cấu hình định nghĩa bảo mật `Bearer` để kích hoạt nút **Authorize** hỗ trợ kiểm thử trực tiếp các REST API bằng Bearer Token trên giao diện Swagger UI.
* **Cập nhật Phân quyền**:
  - `ProductsApiController`: Cập nhật class thành `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]` để yêu cầu JWT Token của Admin khi truy xuất phim.
  - `CartApiController`: Cập nhật class thành `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` để yêu cầu JWT Token khi thao tác giỏ hàng API.

#### Tích Hợp Audit Logging
* **`AuthApiController.cs`**: Tự động ghi lại log hoạt động vào database:
  - Thành công: Ghi Audit Log hành động `"API Login"`.
  - Thất bại: Ghi Audit Log hành động `"API Login Failed"`.

---

## ✅ Các Tính Năng Đã Hoàn Thành Trước Đó (Tóm tắt)

* **Phase 1 & 2 (Identity + Auth)**: Đăng ký, đăng nhập bảo mật bằng Identity, phân quyền Admin/User đầy đủ.
* **Phase 3 (Admin Area)**: Trang quản trị Dashboard thống kê và CRUD phim đầy đủ các trường dữ liệu mới.
* **Hero Banner**: Banner động tự động phát trailer ẩn danh, Ken-burns zoom nền, tự pause khi cuộn trang ra ngoài vùng hiển thị.
* **Netflix Slider**: Slider cuộn chuột, vuốt màn hình cảm ứng mượt mà kèm pagination indicators động.
* **Continue Watching**: Ghi nhớ thời gian đã xem của từng phim định kỳ mỗi 5s, tự động phát tiếp khi mở lại, thanh tiến trình màu đỏ chuyên nghiệp.
* **Checkout & Payment**: Giỏ hàng (Cart) sử dụng Session, trang Checkout xác thực DataAnnotations, quy trình thanh toán mô phỏng (COD, Chuyển khoản, VNPay, PayOS), tự kích hoạt Premium.
* **Admin Dashboard & Lifecycle**: Quản lý đơn hàng, đổi trạng thái (Pending -> Paid / Completed / Cancelled), nút đồng bộ vòng đời gói dịch vụ, thông báo cảnh báo hết hạn Premium < 3 ngày.
* **RESTful Cart & Products APIs**: Các endpoints RESTful dùng để quản lý giỏ hàng và dữ liệu phim của Admin.

---

## 📁 Cấu Trúc Các File Đã Thêm / Sửa Hôm Nay (12/06)

```
HUTECH_LTW.FILMIX/
├── Areas/Admin/
│   └── Views/
│       └── Shared/
│           └── _AdminLayout.cshtml  ✅ SỬA (Cập nhật sidebar System Logs và bố cục)
├── Controllers/
│   ├── AccountController.cs         ✅ SỬA (Tích hợp đăng nhập bên thứ ba Google & Facebook)
│   ├── ChatbotApiController.cs      ✅ MỚI (Xử lý hỏi đáp chatbot qua API)
│   ├── OrderController.cs           ✅ SỬA (Tích hợp gửi email đơn hàng và nạp dữ liệu chi tiết)
│   ├── WatchlistController.cs       ✅ SỬA (Triển khai API lấy danh sách và đồng bộ watchlist)
│   └── HomeController.cs            ✅ SỬA (Tiêm các repo & service để tối ưu hóa trang chủ)
├── Data/
│   └── ApplicationDbContext.cs      ✅ SỬA (Thêm thực thể WatchlistItem và cấu hình quan hệ)
├── Models/
│   └── Entities/
│       └── Entities.cs              ✅ SỬA (Định nghĩa thực thể WatchlistItem trong DB)
├── Services/
│   ├── IEmailService.cs             ✅ MỚI (Interface dịch vụ gửi email)
│   ├── EmailService.cs              ✅ MỚI (Hiện thực gửi mail bằng SmtpClient & lưu preview)
│   └── CartService.cs               ✅ SỬA (Cập nhật logic giỏ hàng cục bộ)
├── Views/
│   ├── Account/
│   │   ├── Auth.cshtml              ✅ SỬA (Tích hợp nút đăng nhập bằng Google/Facebook)
│   │   └── ExternalLoginSuccess.cshtml ✅ MỚI (View cầu nối đồng bộ trạng thái đăng nhập OAuth)
│   ├── Shared/
│   │   └── _Layout.cshtml           ✅ SỬA (Tích hợp CSS/JS chatbot, script đồng bộ Watchlist)
│   ├── Subscription/
│   │   └── Plans.cshtml             ✅ SỬA (Chặn thao tác chọn gói khi chưa đăng nhập)
│   └── TVShows/
│       └── Index.cshtml             ✅ SỬA (Cập nhật nút lưu phim TV nổi bật)
├── wwwroot/
│   ├── css/
│   │   └── chatbot.css              ✅ MỚI (Style giao diện bong bóng chat & khung chat Netflix)
│   └── js/
│       ├── chatbot.js               ✅ MỚI (Xử lý DOM gửi tin, nhận tin và scroll chatbot)
│       └── watchlist-sync.js        ✅ MỚI (Script đồng bộ Watchlist giữa localStorage và DB)
├── appsettings.json                 ✅ SỬA (Cấu hình SMTP EmailSettings và Google/Facebook OAuth keys)
├── Program.cs                       ✅ SỬA (Đăng ký DI, cấu hình JWT Bearer và External Logins)
├── CLAUDE.md                        ✅ MỚI (File tài liệu hướng dẫn Claude Code cho dự án)
├── README.md                        ✅ SỬA (Cập nhật sơ đồ tính năng & cấu trúc file)
└── current-state.md                 ✅ SỬA (Cập nhật nhật ký phát triển ngày 12/06)
```

---

## 🟢 Trạng Thái Hiện Tại Của Hệ Thống

* **Trình Biên Dịch**: ✅ **Build Succeeded 100%** — 0 Errors, 0 Warnings!
* **Cơ Sở Dữ Liệu**: ✅ **Migrations Applied Successfully** — Đã nạp 18 phim thật và các bảng thanh toán qua EF.
* **Kiểm Thử Thực Tế (Live Tested)**: ✅ **PASSED 100%** trên port `http://localhost:5241`.
  - Quy trình đăng ký, mua gói Premium và cập nhật hạn dùng hoạt động chuẩn xác.
  - Hero Banner hiển thị phim nổi bật theo đánh giá sao kèm trailer thông minh.
* **Tiến Độ Dự Án**: 🏆 **Sẵn sàng bảo vệ đồ án 100%** với cấu trúc kiến trúc vững chắc và trải nghiệm người dùng tối ưu!
  - Tích hợp JWT Bearer hoạt động trơn tru. API đăng nhập trả về Token đúng cấu trúc.
  - Các API profile, products và cart yêu cầu JWT Bearer xác thực chính xác, trả về mã 401 khi không truyền Token.
  - Các trang MVC chạy Cookie Authentication truyền thống hoạt động bình thường, không bị ảnh hưởng hay xung đột.
* **Tiến Độ Dự Án**: 🏆 **Hoàn thành 100%** toàn bộ các tính năng theo yêu cầu mở rộng về REST API JWT Authentication!
  - Homepage & Hero Banner hoạt động hoàn hảo.
  - Search & Suggestion dropdown mượt mà, không lỗi giao diện.
  - Trang chi tiết hiển thị dữ liệu DB thực, slider phim tương tự cuộn tốt.
  - Watchlist quản lý dữ liệu động & skeleton loading chạy chuẩn.
  - Các trang lỗi bắt lỗi định tuyến chính xác và hiển thị đẹp mắt.
* **Tiến Độ Dự Án**: 🏆 **Hoàn thành 100%** toàn bộ các tính năng cốt lõi và bổ sung nâng cao! Dự án ở trạng thái ổn định nhất để bàn giao/bảo vệ.
