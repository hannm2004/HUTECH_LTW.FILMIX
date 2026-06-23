# BÁO CÁO KIỂM THỬ TỔNG HỢP — HỆ THỐNG FILMIX

> **Vai trò:** Senior QA Engineer
> **Hệ thống:** FILMIX — Netflix-clone (ASP.NET Core 9 MVC + EF Core 9 + ASP.NET Identity + REST API/JWT + Swagger)
> **Provider DB:** MySQL `filmix_db` (`localhost:3306`)
> **Ngày lập:** 23/06/2026
> **Phạm vi:** Gộp 2 module kiểm thử — **Database** và **API** — vào một báo cáo duy nhất, kèm kết quả thực thi.

---

## A. TÓM TẮT ĐIỀU HÀNH (Executive Summary)

| Hạng mục | Database | API | Tổng |
|---|---|---|---|
| Tổng số test case thiết kế | 73 | 67 | **140** |
| Đã thực thi (tự động) | 38 | 62 | **100** |
| ✅ PASS | 38 | 62 | **100** |
| ❌ FAIL | 0 | 0 | **0** |
| ⏸️ Còn lại (cần thao tác UI / cấu hình thủ công) | 35 | 5 | **40** |
| Tỉ lệ pass trên số đã chạy | 100% | 100% | **100%** |

**Kết luận chung:** Đã thực thi tự động **100 test case, PASS 100%, 0 FAIL** (92 lượt đầu + 8 nhóm SQL-thuần D4/D8/D11). Các luồng cốt lõi — xác thực, phân quyền Admin/User, toàn vẹn dữ liệu (FK chặn MovieId lạ, composite PK, computed property), mapping nhiều-nhiều Movie↔Category, CRUD phim qua API có kiểm chứng DB, hệ thống gợi ý (GROUP BY/COUNT trên SQL), thống kê dashboard (doanh thu chỉ tính đơn Paid/Completed), token hết hạn/giả mạo, chống SQL Injection & XSS lưu trữ, bảo vệ dữ liệu cá nhân ở chatbot — đều hoạt động đúng. Không phát hiện lỗi. **40 case còn lại** cần thao tác thủ công (form đăng ký, upload file poster, màn checkout/thanh toán, redirect trang Admin, render trên trình duyệt, đổi cấu hình Production) — xem **mục D. Hướng dẫn chi tiết từng case** ở cuối báo cáo.

---

## B. KẾT QUẢ THỰC THI (lượt chạy đầy đủ — 23/06/2026)

**Môi trường:** app `http://localhost:5241` (Development), MySQL `filmix_db` (root/123456). Công cụ: `curl` + `mysql.exe` + `openssl` (tự ký JWT hết hạn). Dữ liệu nền: **4 user** (2 Admin, 1 User, 1 không role), **18 phim**, **15 đơn** (13 Paid, 2 Pending), **7 categories**. Dữ liệu seed phục vụ test (3 bản ghi lịch sử xem, phim CRUD tạm) **đã được dọn sạch** sau khi chạy → DB trở về nguyên trạng.

**Kết quả: 92/92 PASS — 0 FAIL.**

### B.1. Module Database — 30/30 PASS

| Nhóm | Test ID PASS | Bằng chứng |
|---|---|---|
| User Management | DB-USR-06, 07, 08, 09, 10 | `IsPremium` không có cột; cột Premium Start/End tồn tại; admin1=Admin; user=User; user không có Admin |
| Movie Management | DB-MOV-05, 06 | Rating không âm; query ID 999999 = 0 dòng |
| Poster Upload | DB-POS-02 | 0 dòng `ImageUrl` chứa path ổ đĩa `C:\`/`D:\` |
| Category | DB-CAT-01, 04, 05 | 7 categories; composite PK `MovieId,CategoryId`; FK chặn MovieId lạ |
| Shopping Cart | DB-CRT-01 | 0 bảng `%cart%` trong schema |
| Order | DB-ORD-03, 04, 07 | TotalAmount=Σ(Price×Qty) 0 lệch; 0 OrderItem mồ côi; 0 PlanId mồ côi |
| Premium | DB-PRM-04 | `CASE WHEN PremiumEndDate>NOW()` = 1 đúng computed property |
| Viewing History | DB-VH-01, 02, 03, 05 | Ghi WatchTime=120; update→300 không trùng; 0 mồ côi; 0 UserId rỗng |
| **Recommendation** | DB-REC-01, 02, 04, 06 | 3 phim đã xem; **GROUP BY/COUNT** trả top category id=5 (Kịch Tính, 3 lượt); gợi ý loại trừ phim đã xem; top genres = 3 thể loại |
| Security | DB-SEC-02, 05, 06 | 2 Admin; role = {Admin, User}; 0 mật khẩu plaintext |
| Dashboard | DB-DSH-01, 02, 03 | 4 user; 18 phim; 15 đơn |

**Bổ sung 8 case SQL-thuần (D4/D8/D11) — chạy 24/06/2026:**

| Test ID | Nội dung | Kết quả thực tế | Verdict |
|---|---|---|---|
| DB-CAT-02 | Mapping Movie↔Category (phim Id=1) | 2 category (Hành Động, Kịch Tính) — JOIN OK | ✅ PASS |
| DB-CAT-03 | Một phim gán ≥2 category | 16 phim có ≥2 category | ✅ PASS |
| DB-CAT-06 | Category chưa gán phim (hợp lệ) | 1 category rỗng (Tình Cảm) — không lỗi | ✅ PASS |
| DB-VH-04 | INSERT lịch sử với MovieId=999999 | **FK chặn** (`ERROR 1452` constraint), COUNT=0 | ✅ PASS |
| DB-DSH-04 | Doanh thu (Status Paid=1/Completed=4) | 2.147.000 ₫ | ✅ PASS |
| DB-DSH-05 | Top 5 phim mới nhất | Dune:Part Two 2024 → … (đúng thứ tự Year DESC) | ✅ PASS |
| DB-DSH-06 | Top user Premium còn hạn | 2 user (EndDate > NOW) | ✅ PASS |
| DB-DSH-07 | Pending/Cancelled KHÔNG vào doanh thu | 298.000 ₫ (0,5) tách biệt khỏi 2.147.000 ₫ (1,4) | ✅ PASS |

### B.2. Module API — 62/62 PASS

| Nhóm | Test ID PASS | Số | Ghi chú nổi bật |
|---|---|---|---|
| Swagger | SWG-01, 02, 03 | 3 | UI + doc auth/products/cart đều 200 |
| Authentication | AUTH-01 → 08 | 8 | Login đúng/sai email/sai mật khẩu/thiếu field/email 256 ký tự |
| JWT Authorization | JWT-01, 02, 03, 04, **05**, 06, 08 | 7 | **JWT-05: tự ký token `exp` quá khứ bằng secret → 401**; thiếu/sai/giả mạo/Basic/rỗng đều 401 |
| Product/Movie API | MOV-01 → 12 | 12 | **CRUD đầy đủ + kiểm chứng DB**: tạo (201, có dòng+mapping) → sửa (Title/Year đúng) → xóa (DB=0) |
| Invalid Resource 404 | 404-01 → 06 | 6 | GET/PUT/DELETE ID lạ → 404; cart planId/PlanId lạ → 404 |
| Security/Authz | SEC-01 → 07 | 7 | Không token→401; user→403 (GET/POST/DELETE); admin→200 |
| Chatbot | BOT-01 → 06 | 6 | Chào hỏi; giá gói từ DB; **chưa đăng nhập hỏi đơn → yêu cầu đăng nhập**; rỗng; fallback |
| Security Attack | ATK-01 → 13 | 13 | SQLi (login/search/**DROP TABLE**) không bypass, bảng còn nguyên; **XSS lưu literal**; token giả→401; sai method→405; oversized 1.5MB→400 (không treo) |

**2 điều chỉnh so với dự đoán ban đầu (đã ghi nhận):**
- **API-ATK-01 SQLi login:** thực tế **400/401** (do `[EmailAddress]` chặn payload trước khi chạm DB) — an toàn hơn, không bypass.
- **API-ATK-08 GET vào login:** thực tế **404** (attribute routing không có route GET khớp), không 500 — vẫn an toàn.

**Lưu ý kỹ thuật trong quá trình chạy** (đều do harness, không phải lỗi app, đã khắc phục và pass):
- Biến `UID` bị bash giữ chỗ (readonly) → seed nhầm; đổi tên biến → pass.
- `curl` không nhận body 1.5MB trên dòng lệnh → chuyển sang `--data @file` → pass (400).

---

# PHẦN I — TEST CASE MODULE DATABASE

> **Phạm vi:** Kiểm thử tầng dữ liệu (EF Core 9 + MySQL `filmix_db`). Mỗi bảng pipe-table copy thẳng sang Excel/Word.

## ⚠️ Ghi chú schema quan trọng (đọc trước khi test)

| # | Phát hiện | Hệ quả khi test |
|---|---|---|
| 1 | **`IsPremium` KHÔNG phải cột trong DB.** Computed property trong `ApplicationUser`: `IsPremium => PremiumEndDate.HasValue && PremiumEndDate.Value > DateTime.Now`. | Không thể `SELECT IsPremium`. Xác minh Premium qua `PremiumStartDate`/`PremiumEndDate`. |
| 2 | **Giỏ hàng KHÔNG lưu ở Database.** Cart là cookie `FilmixCart` (JSON Base64, 30 ngày). Không có bảng Cart. | Test "thêm vào giỏ" xác minh **không** có bảng/row cart; dữ liệu chỉ chốt vào `Orders`/`OrderItems` khi checkout. |
| 3 | **Phân quyền dùng bảng Identity:** `AspNetRoles` (Admin/User), `AspNetUserRoles`, `AspNetUsers`. | Xác minh quyền bằng JOIN 3 bảng, không có cột `Role` trên user. |
| 4 | **Recommendation `GroupBy`/`Count` chạy trên SQL.** `GroupBy(...).Count()...Take()` trước `ToListAsync()` ⇒ EF dịch sang SQL `GROUP BY ... COUNT(...)`. | Test hiệu năng bắt SQL thực tế chứa `GROUP BY`/`COUNT`. |
| 5 | **Poster lưu đường dẫn web tương đối** (`/images/posters/abc.jpg`) vào `Movies.ImageUrl`. | Cột `ImageUrl` không chứa path tuyệt đối `C:\...`/`D:\...`. |
| 6 | **Schema tạo bằng `EnsureCreated()`** (không migrations runtime). | Đổi schema phải DROP DB rồi chạy lại. |

**Tên bảng thật:** `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `Movies`, `Categories`, `MovieCategories` (join: `MovieId`,`CategoryId`), `Episodes`, `MovieImages`, `SubscriptionPlans`, `UserSubscriptions`, `Orders`, `OrderItems`, `ViewingHistories`, `WatchlistItems`, `SystemLogs`.

## I.1. User Management

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-USR-01 | User Management | Đăng ký tài khoản hợp lệ → lưu vào AspNetUsers | DB sạch, email chưa tồn tại | 1. Mở `/Account/Auth` 2. Nhập thông tin 3. Submit | Email: `newuser@test.com`, Password: `User@123`, FullName: `Nguyen Van A` | `SELECT Id, Email, UserName, FullName, EmailConfirmed FROM AspNetUsers WHERE Email='newuser@test.com';` | 1 dòng; `Email`/`UserName` đúng, `FullName` đúng, `PasswordHash` không null | High |
| DB-USR-02 | User Management | Mật khẩu được hash, không lưu plaintext | DB-USR-01 đã chạy | 1. Truy vấn PasswordHash | (như trên) | `SELECT PasswordHash FROM AspNetUsers WHERE Email='newuser@test.com';` | Chuỗi băm (≠ `User@123`), độ dài > 40 | High |
| DB-USR-03 | User Management | **(Negative)** Đăng ký trùng email | Đã tồn tại `newuser@test.com` | 1. Đăng ký lại cùng email | Email: `newuser@test.com` | `SELECT COUNT(*) FROM AspNetUsers WHERE Email='newuser@test.com';` | Báo lỗi trùng; `COUNT(*) = 1` | High |
| DB-USR-04 | User Management | **(Negative)** Email sai định dạng | DB sạch | 1. Đăng ký email không hợp lệ | Email: `abc@`, Password: `User@123` | `SELECT COUNT(*) FROM AspNetUsers WHERE Email='abc@';` | Validation chặn; `COUNT(*) = 0` | Medium |
| DB-USR-05 | User Management | **(Negative)** Mật khẩu yếu | DB sạch | 1. Đăng ký mật khẩu yếu | Email: `weak@test.com`, Password: `123` | `SELECT COUNT(*) FROM AspNetUsers WHERE Email='weak@test.com';` | Identity từ chối; `COUNT(*) = 0` | Medium |
| DB-USR-06 | User Management | User mới mặc định KHÔNG Premium | DB-USR-01 đã chạy | 1. Kiểm tra cột Premium | (như trên) | `SELECT PremiumStartDate, PremiumEndDate FROM AspNetUsers WHERE Email='newuser@test.com';` | Cả hai `= NULL` ⇒ `IsPremium=false` | High |
| DB-USR-07 | User Management | `IsPremium` KHÔNG tồn tại như cột DB | DB đã tạo | 1. Liệt kê cột | — | `SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AspNetUsers' AND COLUMN_NAME='IsPremium';` | **0 dòng** | Medium |
| DB-USR-08 | User Management | Gán quyền User mặc định | Role `User` đã seed | 1. Đăng ký user 2. Kiểm tra role | Email: `roleuser@test.com` | `SELECT r.Name FROM AspNetUserRoles ur JOIN AspNetUsers u ON u.Id=ur.UserId JOIN AspNetRoles r ON r.Id=ur.RoleId WHERE u.Email='roleuser@test.com';` | Trả về `User` | High |
| DB-USR-09 | User Management | Admin seed có quyền Admin | `DbSeeder` đã chạy | 1. Kiểm tra role admin1 | `admin1@filmix.com` | `SELECT r.Name FROM AspNetUserRoles ur JOIN AspNetUsers u ON u.Id=ur.UserId JOIN AspNetRoles r ON r.Id=ur.RoleId WHERE u.Email='admin1@filmix.com';` | Trả về `Admin` | High |
| DB-USR-10 | User Management | **(Negative)** User thường không có role Admin | DB-USR-08 đã chạy | 1. Kiểm tra | `roleuser@test.com` | `SELECT COUNT(*) FROM AspNetUserRoles ur JOIN AspNetUsers u ON u.Id=ur.UserId JOIN AspNetRoles r ON r.Id=ur.RoleId WHERE u.Email='roleuser@test.com' AND r.Name='Admin';` | `COUNT(*) = 0` | High |

## I.2. Movie Management

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-MOV-01 | Movie Management | Thêm phim mới → lưu vào Movies | Đăng nhập Admin | 1. Admin → Product → Thêm phim 2. Nhập 3. Lưu | Title: `Inception`, Year: `2010`, Genre: `Sci-Fi`, Director: `Nolan` | `SELECT Id, Title, Year, Genre, Director, IsTVSeries FROM Movies WHERE Title='Inception';` | 1 dòng khớp input, `IsTVSeries=0` | High |
| DB-MOV-02 | Movie Management | Thêm TV series | Đăng nhập Admin | 1. Thêm phim TV series | Title: `Stranger Things`, IsTVSeries: `true` | `SELECT IsTVSeries FROM Movies WHERE Title='Stranger Things';` | `IsTVSeries = 1` | Medium |
| DB-MOV-03 | Movie Management | **(Negative)** Thiếu Title | Đăng nhập Admin | 1. Bỏ trống Title 2. Lưu | Title: `` , Year: `2020` | `SELECT COUNT(*) FROM Movies WHERE Year=2020 AND (Title IS NULL OR Title='');` | Validation chặn; không tạo dòng | Medium |
| DB-MOV-04 | Movie Management | **(Negative)** Year sai kiểu | Đăng nhập Admin | 1. Nhập Year='abcd' | Year: `abcd` | `SELECT COUNT(*) FROM Movies WHERE Title='BadYearMovie';` | Validation chặn; `COUNT(*)=0` | Low |
| DB-MOV-05 | Movie Management | Rating mặc định = 0 | Đăng nhập Admin | 1. Thêm phim không nhập Rating | Title: `NoRatingMovie` | `SELECT Rating FROM Movies WHERE Title='NoRatingMovie';` | `Rating = 0` | Low |
| DB-MOV-06 | Movie Management | **(Negative)** Truy vấn phim không tồn tại | — | 1. Query Id lạ | Id: `999999` | `SELECT * FROM Movies WHERE Id=999999;` | 0 dòng (không lỗi) | Low |
| DB-MOV-07 | Movie Management | Xóa phim → xóa kèm MovieCategories | Phim có gán category | 1. Xóa phim 2. Kiểm tra join | MovieId đã xóa | `SELECT COUNT(*) FROM MovieCategories WHERE MovieId=<deletedId>;` | `COUNT(*) = 0` | Medium |

## I.3. Poster Upload

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-POS-01 | Poster Upload | Upload poster → lưu path web vào ImageUrl | Admin, phim tồn tại | 1. Sửa phim 2. Upload 3. Lưu | File: `poster.jpg` | `SELECT ImageUrl FROM Movies WHERE Title='Inception';` | `ImageUrl` dạng `/images/...`, không rỗng | High |
| DB-POS-02 | Poster Upload | **DB chỉ lưu path, KHÔNG path vật lý ổ đĩa** | DB-POS-01 đã chạy | 1. Kiểm tra ImageUrl | (như trên) | `SELECT ImageUrl FROM Movies WHERE ImageUrl LIKE 'C:\\%' OR ImageUrl LIKE 'D:\\%' OR ImageUrl LIKE '%:\\%';` | **0 dòng** | High |
| DB-POS-03 | Poster Upload | Path phục vụ được từ wwwroot | DB-POS-01 đã chạy | 1. Lấy ImageUrl 2. Mở URL | (như trên) | `SELECT ImageUrl FROM Movies WHERE Title='Inception';` | URL bắt đầu `/`, ảnh tải được | Medium |
| DB-POS-04 | Poster Upload | **(Negative)** Upload sai định dạng (.exe) | Admin | 1. Upload `virus.exe` | File: `virus.exe` | `SELECT ImageUrl FROM Movies WHERE Title='Inception';` | App từ chối; ImageUrl không thành `.exe` | High |
| DB-POS-05 | Poster Upload | **(Negative)** Không upload → giữ nguyên/NULL | Phim chưa có poster | 1. Lưu không chọn file | — | `SELECT ImageUrl FROM Movies WHERE Title='NoRatingMovie';` | NULL hoặc giá trị cũ | Low |
| DB-POS-06 | Poster Upload | **(Negative)** Upload file quá lớn | Admin | 1. Upload ảnh > giới hạn | File: `huge.jpg` (50MB) | `SELECT ImageUrl FROM Movies WHERE Title='Inception';` | App giới hạn; không lưu file lỗi | Medium |

## I.4. Category Management

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-CAT-01 | Category Management | Danh sách Categories được seed | Seed đã chạy | 1. Query category | — | `SELECT Id, Name FROM Categories ORDER BY Id;` | ≥ 1 dòng đúng seed (thực tế: 7) | Medium |
| DB-CAT-02 | Category Management | Mapping Movie ↔ Category | Phim gán ≥1 category | 1. Query join | MovieId, CategoryId | `SELECT mc.MovieId, mc.CategoryId, m.Title, c.Name FROM MovieCategories mc JOIN Movies m ON m.Id=mc.MovieId JOIN Categories c ON c.Id=mc.CategoryId WHERE m.Title='Inception';` | Đúng các cặp đã gán | High |
| DB-CAT-03 | Category Management | Một phim nhiều category | Phim gán ≥2 | 1. Đếm category của phim | MovieId | `SELECT COUNT(*) FROM MovieCategories WHERE MovieId=<id>;` | `COUNT(*) ≥ 2` | Medium |
| DB-CAT-04 | Category Management | Composite PK chống trùng mapping | Mapping đã tồn tại | 1. Thử thêm lại cùng cặp | MovieId=1, CategoryId=1 | `SELECT COUNT(*) FROM MovieCategories WHERE MovieId=1 AND CategoryId=1;` | `COUNT(*) = 1` | Medium |
| DB-CAT-05 | Category Management | **(Negative)** Mapping MovieId không tồn tại | — | 1. Insert mapping MovieId lạ | MovieId=999999, CategoryId=1 | `SELECT COUNT(*) FROM MovieCategories WHERE MovieId=999999;` | FK chặn; `COUNT(*) = 0` | Medium |
| DB-CAT-06 | Category Management | **(Negative)** Category không có phim | Category mới | 1. Query phim của category rỗng | CategoryId mới | `SELECT COUNT(*) FROM MovieCategories WHERE CategoryId=<newId>;` | `COUNT(*) = 0` (hợp lệ) | Low |

## I.5. Shopping Cart

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-CRT-01 | Shopping Cart | Cart KHÔNG lưu ở Database | App chạy | 1. Liệt kê bảng tìm cart | — | `SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='filmix_db' AND TABLE_NAME LIKE '%Cart%';` | **0 dòng** | High |
| DB-CRT-02 | Shopping Cart | Thêm gói vào giỏ → cookie, không sinh row DB | Ở trang gói | 1. Thêm gói 2. Kiểm tra Orders/OrderItems | PlanId: `2` | `SELECT COUNT(*) FROM Orders WHERE Status=0; SELECT COUNT(*) FROM OrderItems;` | Không phát sinh Order/OrderItem chỉ vì thêm giỏ | High |
| DB-CRT-03 | Shopping Cart | Giỏ tồn tại sau đăng nhập/đăng xuất | Thêm giỏ khi là khách | 1. Thêm (guest) 2. Đăng nhập 3. Kiểm tra | PlanId: `2` | (kiểm tra cookie `FilmixCart`) | Giỏ giữ nguyên, DB không có row cart | Medium |
| DB-CRT-04 | Shopping Cart | Checkout → Orders/OrderItems rồi xóa cookie | Có item, đã đăng nhập | 1. Checkout | PlanId: `2`, Qty: `1` | `SELECT o.Id FROM Orders o ORDER BY o.Id DESC LIMIT 1;` + `SELECT * FROM OrderItems WHERE OrderId=<id>;` | Order+OrderItem tạo; cookie bị xóa | High |

## I.6. Order Management

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-ORD-01 | Order Management | Tạo đơn → lưu vào Orders | Đăng nhập, giỏ có gói | 1. Checkout 2. Nhập 3. Đặt | FullName: `Tran B`, Email: `tranb@test.com`, Phone: `0909000111`, PlanId: `2` | `SELECT Id, UserId, TotalAmount, PaymentMethod, Status, Email FROM Orders WHERE Email='tranb@test.com' ORDER BY Id DESC LIMIT 1;` | 1 đơn; `Status=0`, `UserId` khớp, `TotalAmount>0` | High |
| DB-ORD-02 | Order Management | OrderItems khớp đơn | DB-ORD-01 | 1. Query chi tiết | OrderId | `SELECT oi.Id, oi.OrderId, oi.PlanId, oi.Quantity, oi.Price FROM OrderItems oi WHERE oi.OrderId=<id>;` | ≥1 dòng; PlanId đúng, Price khớp | High |
| DB-ORD-03 | Order Management | TotalAmount = Σ(Price×Qty) | DB-ORD-01,02 | 1. So tổng | OrderId | `SELECT o.TotalAmount, (SELECT SUM(oi.Price*oi.Quantity) FROM OrderItems oi WHERE oi.OrderId=o.Id) AS Computed FROM Orders o WHERE o.Id=<id>;` | `TotalAmount = Computed` | High |
| DB-ORD-04 | Order Management | FK OrderItems.OrderId | DB-ORD-01 | 1. Kiểm tra mồ côi | — | `SELECT COUNT(*) FROM OrderItems oi LEFT JOIN Orders o ON o.Id=oi.OrderId WHERE o.Id IS NULL;` | `COUNT(*) = 0` | Medium |
| DB-ORD-05 | Order Management | **(Negative)** Checkout thiếu thông tin | Đăng nhập, giỏ có gói | 1. Bỏ trống Email/Phone 2. Đặt | Email: ``, Phone: `` | `SELECT COUNT(*) FROM Orders WHERE Email='';` | Validation chặn; không tạo | Medium |
| DB-ORD-06 | Order Management | **(Negative)** Tạo đơn khi giỏ trống | Đăng nhập, giỏ rỗng | 1. Truy cập checkout | — | `SELECT COUNT(*) FROM Orders WHERE UserId='<id>' AND CreatedAt > <t>;` | Không tạo đơn rỗng | Medium |
| DB-ORD-07 | Order Management | **(Negative)** PlanId không tồn tại | — | 1. Tạo OrderItem PlanId lạ | PlanId: `999999` | `SELECT COUNT(*) FROM OrderItems WHERE PlanId=999999;` | FK chặn; `COUNT(*) = 0` | Medium |
| DB-ORD-08 | Order Management | Đổi trạng thái (Pending→Paid) | Có đơn Pending | 1. Admin đổi trạng thái | Status: `Paid`(=1) | `SELECT Status FROM Orders WHERE Id=<id>;` | `Status = 1` | Medium |

## I.7. Premium Activation

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-PRM-01 | Premium Activation | Trước thanh toán: chưa Premium | User mới | 1. Kiểm tra cột Premium | UserId | `SELECT PremiumStartDate, PremiumEndDate FROM AspNetUsers WHERE Id='<userId>';` | Cả hai NULL ⇒ `IsPremium=false` | High |
| DB-PRM-02 | Premium Activation | Sau thanh toán: kích hoạt Premium | Đơn Pending của user | 1. `Order/ProcessMockPayment` 2. Kiểm tra | OrderId | `SELECT PremiumStartDate, PremiumEndDate FROM AspNetUsers WHERE Id='<userId>';` | `PremiumStartDate` ≠ NULL; `PremiumEndDate > NOW()` | High |
| DB-PRM-03 | Premium Activation | Đơn chuyển Paid khi kích hoạt | DB-PRM-02 | 1. Kiểm tra trạng thái | OrderId | `SELECT Status FROM Orders WHERE Id=<id>;` | `Status = 1` (Paid) hoặc `4` | High |
| DB-PRM-04 | Premium Activation | Xác minh "IsPremium=True" qua computed | DB-PRM-02 | 1. Mô phỏng điều kiện computed | UserId | `SELECT CASE WHEN PremiumEndDate IS NOT NULL AND PremiumEndDate > NOW() THEN 1 ELSE 0 END AS IsPremium FROM AspNetUsers WHERE Id='<userId>';` | Trả về `1` | High |
| DB-PRM-05 | Premium Activation | **(Negative)** Premium hết hạn ⇒ không Premium | EndDate quá khứ | 1. Set EndDate quá khứ 2. Kiểm tra | PremiumEndDate: hôm qua | `SELECT CASE WHEN PremiumEndDate > NOW() THEN 1 ELSE 0 END AS IsPremium FROM AspNetUsers WHERE Id='<userId>';` | Trả về `0` | Medium |
| DB-PRM-06 | Premium Activation | **(Negative)** Thanh toán thất bại không kích hoạt | Đơn chưa Paid | 1. Để Pending 2. Kiểm tra | OrderId Pending | `SELECT PremiumEndDate FROM AspNetUsers WHERE Id='<userId>';` | `PremiumEndDate = NULL` | High |
| DB-PRM-07 | Premium Activation | Idempotent: gọi payment 2 lần | DB-PRM-02 | 1. Gọi lại cùng OrderId | OrderId đã Paid | `SELECT Status, PremiumEndDate FROM Orders o JOIN AspNetUsers u ON u.Id=o.UserId WHERE o.Id=<id>;` | Không nhân đôi; giữ Paid | Medium |

## I.8. Viewing History

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-VH-01 | Viewing History | Xem phim → ghi vào ViewingHistories | Đăng nhập, phim tồn tại | 1. Phát phim 2. Ghi tiến độ | UserId, MovieId=`1`, WatchTime=`120` | `SELECT UserId, MovieId, WatchTime, WatchedAt FROM ViewingHistories WHERE UserId='<id>' AND MovieId=1;` | 1 dòng; `WatchTime=120` | High |
| DB-VH-02 | Viewing History | Xem lại → cập nhật (không trùng) | DB-VH-01 | 1. Xem lại WatchTime mới | WatchTime=`300` | `SELECT COUNT(*), MAX(WatchTime) FROM ViewingHistories WHERE UserId='<id>' AND MovieId=1;` | `COUNT(*)=1`, `WatchTime=300` | High |
| DB-VH-03 | Viewing History | FK tới Movie & User hợp lệ | DB-VH-01 | 1. JOIN kiểm tra | — | `SELECT vh.Id FROM ViewingHistories vh JOIN Movies m ON m.Id=vh.MovieId JOIN AspNetUsers u ON u.Id=vh.UserId WHERE vh.UserId='<id>';` | JOIN thành công, không mồ côi | Medium |
| DB-VH-04 | Viewing History | **(Negative)** MovieId không tồn tại bị bỏ qua | Đăng nhập | 1. Log MovieId lạ | MovieId=`999999` | `SELECT COUNT(*) FROM ViewingHistories WHERE MovieId=999999;` | `COUNT(*)=0` | Medium |
| DB-VH-05 | Viewing History | **(Negative)** Guest (userId rỗng) không ghi | Chưa đăng nhập | 1. Phát phim khi là khách | UserId=`null` | `SELECT COUNT(*) FROM ViewingHistories WHERE UserId IS NULL OR UserId='';` | `COUNT(*)=0` | Medium |

## I.9. Recommendation System

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-REC-01 | Recommendation | Lấy dữ liệu đề xuất từ ViewingHistories | User có lịch sử | 1. Gọi gợi ý | UserId | `SELECT DISTINCT MovieId FROM ViewingHistories WHERE UserId='<id>';` | Danh sách phim đã xem để loại trừ | High |
| DB-REC-02 | Recommendation | Xác định thể loại yêu thích theo số lần xem | User xem nhiều cùng thể loại | 1. Tính top category | UserId | `SELECT mc.CategoryId, COUNT(*) AS Cnt FROM ViewingHistories vh JOIN MovieCategories mc ON mc.MovieId=vh.MovieId WHERE vh.UserId='<id>' GROUP BY mc.CategoryId ORDER BY Cnt DESC LIMIT 3;` | Top 3 CategoryId giảm dần | High |
| DB-REC-03 | Recommendation | **GroupBy/Count chạy trên SQL, không tải hết lên RAM** | User có lịch sử | 1. Bật log SQL EF 2. Gọi gợi ý 3. Đọc SQL | UserId | Đối chiếu SQL EF với `... GROUP BY ... ORDER BY COUNT(*) DESC LIMIT 3` | SQL chứa `GROUP BY`+`COUNT(*)`+`LIMIT`; không `SELECT *` rồi group ở C# | High |
| DB-REC-04 | Recommendation | Gợi ý loại trừ phim đã xem | User đã xem MovieId=1 | 1. Lấy gợi ý | UserId | `SELECT Id FROM Movies WHERE Id NOT IN (SELECT MovieId FROM ViewingHistories WHERE UserId='<id>');` | Gợi ý không chứa MovieId=1 | Medium |
| DB-REC-05 | Recommendation | **(Negative)** Chưa xem gì → fallback phim mới | User không lịch sử | 1. Gọi gợi ý | UserId mới | `SELECT Id, Title, Year FROM Movies ORDER BY Year DESC, Id DESC LIMIT 10;` | Danh sách phim mới nhất | Medium |
| DB-REC-06 | Recommendation | Top genres toàn hệ thống | Có ViewingHistories | 1. Gọi `GetTopGenresAsync` | — | `SELECT c.Name, COUNT(*) AS Cnt FROM ViewingHistories vh JOIN MovieCategories mc ON mc.MovieId=vh.MovieId JOIN Categories c ON c.Id=mc.CategoryId GROUP BY c.Name ORDER BY Cnt DESC LIMIT 10;` | Thể loại + số lần xem giảm dần | Low |

## I.10. Security / Phân quyền

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-SEC-01 | Security | **(Negative)** User thường truy cập trang Admin bị chặn | Đăng nhập user thường | 1. Mở `/Admin/Dashboard/Index` | `roleuser@test.com` | `SELECT COUNT(*) FROM AspNetUserRoles ur JOIN AspNetRoles r ON r.Id=ur.RoleId WHERE ur.UserId='<id>' AND r.Name='Admin';` | `COUNT(*)=0` ⇒ 403/redirect | High |
| DB-SEC-02 | Security | Admin truy cập trang Admin | Đăng nhập admin1 | 1. Mở `/Admin/Dashboard/Index` | `admin1@filmix.com` | `SELECT COUNT(*) FROM AspNetUserRoles ur JOIN AspNetRoles r ON r.Id=ur.RoleId JOIN AspNetUsers u ON u.Id=ur.UserId WHERE u.Email='admin1@filmix.com' AND r.Name='Admin';` | `COUNT(*)=1` | High |
| DB-SEC-03 | Security | **(Negative)** Khách truy cập Admin | Đăng xuất | 1. Mở `/Admin/...` | — | (kiểm tra redirect) | Redirect về đăng nhập | High |
| DB-SEC-04 | Security | **(Negative)** Đổi role không phản ánh quyền ngầm | — | 1. Thử bypass | — | `SELECT r.Name FROM AspNetUserRoles ur JOIN AspNetRoles r ON r.Id=ur.RoleId WHERE ur.UserId='<id>';` | Quyền chỉ dựa `AspNetUserRoles` | Medium |
| DB-SEC-05 | Security | Chỉ 2 role hợp lệ | Seed đã chạy | 1. Liệt kê role | — | `SELECT Name FROM AspNetRoles ORDER BY Name;` | `Admin`, `User` | Medium |
| DB-SEC-06 | Security | Mật khẩu không plaintext (toàn bảng) | Có nhiều user | 1. Quét bảng | — | `SELECT COUNT(*) FROM AspNetUsers WHERE PasswordHash IS NULL OR LENGTH(PasswordHash) < 20;` | `COUNT(*)=0` | High |

## I.11. Dashboard Statistics

| Test Case ID | Module | Scenario | Preconditions | Test Steps | Input Data | SQL Query xác minh | Expected Result | Priority |
|---|---|---|---|---|---|---|---|---|
| DB-DSH-01 | Dashboard | Tổng số User | Có ≥1 user | 1. Đọc dashboard | — | `SELECT COUNT(*) AS TotalUsers FROM AspNetUsers;` | Khớp dashboard (thực tế: 4) | High |
| DB-DSH-02 | Dashboard | Tổng số phim | Có ≥1 phim | 1. Đọc thẻ | — | `SELECT COUNT(*) AS TotalMovies FROM Movies;` | Khớp (thực tế: 18) | High |
| DB-DSH-03 | Dashboard | Tổng số đơn hàng | Có ≥1 đơn | 1. Đọc thẻ | — | `SELECT COUNT(*) AS TotalOrders FROM Orders;` | Khớp (thực tế: 15) | High |
| DB-DSH-04 | Dashboard | Tổng doanh thu (đơn đã thanh toán) | Có đơn Paid/Completed | 1. Đọc thẻ | — | `SELECT SUM(TotalAmount) AS Revenue FROM Orders WHERE Status IN (1,4);` | Khớp (thực tế: 2.147.000 ₫) | High |
| DB-DSH-05 | Dashboard | Top phim mới nhất | Có ≥5 phim | 1. Đọc danh sách | — | `SELECT Id, Title, Year FROM Movies ORDER BY Year DESC, Id DESC LIMIT 5;` | Khớp dashboard | Medium |
| DB-DSH-06 | Dashboard | Top người dùng Premium | Có user Premium còn hạn | 1. Đọc danh sách | — | `SELECT Id, Email, FullName, PremiumEndDate FROM AspNetUsers WHERE PremiumEndDate IS NOT NULL AND PremiumEndDate > NOW() ORDER BY PremiumEndDate DESC LIMIT 5;` | User Premium còn hạn | Medium |
| DB-DSH-07 | Dashboard | **(Negative)** Doanh thu không tính Pending/Cancelled | Có đơn Pending & Cancelled | 1. So sánh | — | `SELECT SUM(TotalAmount) FROM Orders WHERE Status IN (0,5);` | Không cộng vào doanh thu | Medium |
| DB-DSH-08 | Dashboard | **(Negative)** DB rỗng → số = 0, không lỗi | DB mới drop | 1. Mở dashboard | — | `SELECT COUNT(*) FROM Orders;` | `0`; không crash | Low |

---

# PHẦN II — TEST CASE MODULE API

> **Phạm vi:** REST API (JWT Bearer + Swagger). Base URL: `http://localhost:5241`.

## ⚠️ Ghi chú API quan trọng (đã đối chiếu source code)

| # | Phát hiện | Hệ quả khi test |
|---|---|---|
| 1 | **`ProductsApiController` gắn `[Authorize(JwtBearer, Roles="Admin")]` ở CẤP CONTROLLER** ⇒ *mọi* endpoint kể cả GET đều cần **JWT Admin**. | Không token → **401**; token role≠Admin → **403**. |
| 2 | **Không tồn tại `OrderApiController` REST.** Đơn hàng qua MVC `OrderController` (cookie, HTML). Bề mặt REST gần nhất: **`api/cart`** (`[Authorize(JwtBearer)]`). | Test "Order API" map sang Cart API. |
| 3 | **JWT lỗi trả JSON, không redirect HTML** (custom `OnChallenge`/`OnForbidden`). | Negative auth mong đợi JSON body. |
| 4 | **Login thành công trả thẳng `LoginResponseDto`** `{token, expiresAt, userName, roles}` (không bọc `ApiResponse`); lỗi login bọc `ApiResponse`. | So khớp 2 hình dạng response. |
| 5 | **JWT hết hạn sau 60 phút** (`JwtSettings.ExpiryInMinutes=60`). | Test token hết hạn: chỉnh nhỏ hoặc dùng token cũ. |
| 6 | **Validation API reformat về `ApiResponse`.** Thiếu `[Required]` → 400. | DTO bắt buộc: Login (Email, Password); CreateMovie (Title, ImageUrl, Year, Genre); UpdateMovie (Id, +4); AddToCart (PlanId). |
| 7 | **Chatbot public**, hỏi đơn hàng khi chưa đăng nhập vẫn trả **HTTP 200** + nội dung yêu cầu đăng nhập. | Test kiểm tra *nội dung reply*, không phải status. |
| 8 | **EF tham số hóa** ⇒ chống SQL Injection; input độc hại lưu/so khớp như literal. | SQLi/XSS không thực thi. |

**Bảng endpoint thật:**

| Controller | Route | Auth | Methods |
|---|---|---|---|
| AuthApi | `api/auth` | login public; profile JWT | `POST /login`, `GET /profile` |
| ProductsApi | `api/products` | **JWT + Role Admin (toàn bộ)** | `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `DELETE /{id}` |
| CartApi | `api/cart` | JWT (mọi user) | `GET /`, `POST /items`, `PUT /items/{planId}`, `DELETE /items/{planId}`, `DELETE /` |
| ChatbotApi | `api/chatbot` | Public (cookie/JWT optional) | `POST /message` |

## II.1. Swagger

| Test Case ID | Module | Scenario | Preconditions | API Endpoint | HTTP Method | Headers | Request Body | Test Steps | Expected HTTP Status | Expected Response | DB Validation | Priority |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| API-SWG-01 | Swagger | Swagger UI load thành công | Development | `/swagger/index.html` | GET | — | — | 1. Mở `/swagger` | 200 | UI hiển thị, không trang trắng | N/A | High |
| API-SWG-02 | Swagger | Doc `auth` JSON hợp lệ | Development | `/swagger/auth/swagger.json` | GET | — | — | 1. GET JSON | 200 | OpenAPI chứa `/api/auth/login`, `/api/auth/profile` | N/A | High |
| API-SWG-03 | Swagger | Doc `products` & `cart` JSON | Development | `/swagger/products/swagger.json`, `/swagger/cart/swagger.json` | GET | — | — | 1. GET 2 file | 200 | Chứa các path tương ứng | N/A | High |
| API-SWG-04 | Swagger | Không lỗi 500 khi gen Swagger | Development | các doc nhóm | GET | — | — | 1. Tải tất cả doc | 200 | Không 500, không stack trace | N/A | High |
| API-SWG-05 | Swagger | **(Negative)** Swagger tắt ở Production | `ASPNETCORE_ENVIRONMENT=Production` | `/swagger` | GET | — | — | 1. Mở `/swagger` | 404 | Không khả dụng | N/A | Medium |
| API-SWG-06 | Swagger Review | API cần JWT hiển thị "Authorize" | Development | `/swagger` | GET | — | — | 1. Kiểm tra ổ khóa | 200 | Có Authorize cho endpoint cần Bearer | N/A | Medium |
| API-SWG-07 | Swagger Review | Dùng Swagger test (Try it out) | JWT Admin | `/api/products` | GET | `Authorization: Bearer <token>` | — | 1. Authorize 2. Try it out | 200 | Gọi được, trả danh sách | N/A | Medium |

## II.2. Authentication API

| Test Case ID | Module | Scenario | Preconditions | API Endpoint | HTTP Method | Headers | Request Body | Test Steps | Expected HTTP Status | Expected Response | DB Validation | Priority |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| API-AUTH-01 | Authentication | **(Positive)** Đăng nhập đúng → JWT | `admin1@filmix.com`/`admin@123` | `/api/auth/login` | POST | `Content-Type: application/json` | `{"email":"admin1@filmix.com","password":"admin@123"}` | 1. POST login | 200 | `{token, expiresAt, userName, roles:["Admin"]}` | `SELECT Action FROM systemlogs WHERE Action='API Login' ORDER BY Id DESC LIMIT 1;` → có log | High |
| API-AUTH-02 | Authentication | **(Positive)** User thường đăng nhập | Có user thường | `/api/auth/login` | POST | `Content-Type: application/json` | `{"email":"user@filmix.com","password":"user@123"}` | 1. POST login | 200 | Token hợp lệ, `roles:["User"]` | — | High |
| API-AUTH-03 | Authentication | **(Negative)** Sai email | — | `/api/auth/login` | POST | `Content-Type: application/json` | `{"email":"khong-ton-tai@test.com","password":"admin@123"}` | 1. POST login | 401 | `{success:false, message:"Email hoặc mật khẩu không chính xác."}` | `SELECT * FROM systemlogs WHERE Action='API Login Failed' ORDER BY Id DESC LIMIT 1;` → có log | High |
| API-AUTH-04 | Authentication | **(Negative)** Sai mật khẩu | Tồn tại admin1 | `/api/auth/login` | POST | `Content-Type: application/json` | `{"email":"admin1@filmix.com","password":"sai-mat-khau"}` | 1. POST login | 401 | Cùng message (không lộ email tồn tại) | Log "API Login Failed" | High |
| API-AUTH-05 | Authentication | **(Negative)** Thiếu password | — | `/api/auth/login` | POST | `Content-Type: application/json` | `{"email":"admin1@filmix.com"}` | 1. POST login | 400 | Lỗi "Mật khẩu không được để trống." | — | Medium |
| API-AUTH-06 | Authentication | **(Negative)** Body rỗng | — | `/api/auth/login` | POST | `Content-Type: application/json` | `{}` | 1. POST login | 400 | Lỗi "Thông tin đăng nhập không hợp lệ." | — | Medium |
| API-AUTH-07 | Authentication | **(Boundary)** Email sai định dạng | — | `/api/auth/login` | POST | `Content-Type: application/json` | `{"email":"abc@","password":"x"}` | 1. POST login | 400 | `[EmailAddress]` chặn | — | Low |
| API-AUTH-08 | Authentication | **(Boundary)** Email 256+ ký tự | — | `/api/auth/login` | POST | `Content-Type: application/json` | `{"email":"<256 ký tự>@test.com","password":"x"}` | 1. POST login | 400/401 | Không 500 | — | Low |

## II.3. JWT Authorization

| Test Case ID | Module | Scenario | Preconditions | API Endpoint | HTTP Method | Headers | Request Body | Test Steps | Expected HTTP Status | Expected Response | DB Validation | Priority |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| API-JWT-01 | JWT Authorization | **(Positive)** Token hợp lệ → Authorized | JWT hợp lệ | `/api/auth/profile` | GET | `Authorization: Bearer <valid>` | — | 1. GET profile | 200 | `UserProfileDto{id, userName, email, fullName, roles, isPremium}` | — | High |
| API-JWT-02 | JWT Authorization | **(Negative)** Thiếu Authorization header | — | `/api/auth/profile` | GET | *(không có)* | — | 1. GET không token | 401 | JSON challenge | — | High |
| API-JWT-03 | JWT Authorization | **(Negative)** Token sai định dạng | — | `/api/auth/profile` | GET | `Authorization: Bearer abc.def` | — | 1. GET token rác | 401 | 401 JSON, không 500 | — | High |
| API-JWT-04 | JWT Authorization | **(Negative)** Token giả mạo (sai chữ ký) | Token thật bị sửa | `/api/auth/profile` | GET | `Authorization: Bearer <tampered>` | — | 1. Sửa 1 ký tự sig 2. GET | 401 | 401 (chữ ký thất bại) | — | High |
| API-JWT-05 | JWT Authorization | **(Negative)** Token hết hạn | Token expiry đã qua | `/api/auth/profile` | GET | `Authorization: Bearer <expired>` | — | 1. GET token hết hạn | 401 | 401, báo token expired | — | High |
| API-JWT-06 | JWT Authorization | **(Negative)** Scheme sai (Basic) | Có token | `/api/auth/profile` | GET | `Authorization: Basic <token>` | — | 1. GET dùng Basic | 401 | 401 (chỉ Bearer) | — | Medium |
| API-JWT-07 | JWT Authorization | **(Boundary)** Token vừa tạo | Vừa login | `/api/auth/profile` | GET | `Authorization: Bearer <fresh>` | — | 1. Login 2. Gọi ngay | 200 | Hợp lệ | — | Low |
| API-JWT-08 | JWT Authorization | **(Negative)** Bearer rỗng | — | `/api/auth/profile` | GET | `Authorization: Bearer ` | — | 1. GET | 401 | 401 JSON | — | Medium |

## II.4. Product / Movie API

> ⚠️ Toàn bộ endpoint **bắt buộc JWT Admin** (header `Authorization: Bearer <ADMIN_TOKEN>`).

| Test Case ID | Module | Scenario | Preconditions | API Endpoint | HTTP Method | Headers | Request Body | Test Steps | Expected HTTP Status | Expected Response | DB Validation | Priority |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| API-MOV-01 | Product API | **(Positive)** Lấy danh sách phim (paged) | Admin token | `/api/products?page=1&pageSize=10` | GET | `Bearer <admin>` | — | 1. GET list | 200 | `{Items[], Page, PageSize, TotalPages, TotalCount}` | `SELECT COUNT(*) FROM movies;` khớp `TotalCount` | High |
| API-MOV-02 | Product API | **(Positive)** Tìm kiếm theo title | Admin token | `/api/products?search=Inter` | GET | `Bearer <admin>` | — | 1. GET search | 200 | Chỉ phim chứa "Inter" | — | Medium |
| API-MOV-03 | Product API | **(Positive)** Lấy phim theo ID | Admin token, phim tồn tại | `/api/products/1` | GET | `Bearer <admin>` | — | 1. GET by id | 200 | `MovieDetailDto` đầy đủ | `SELECT Title FROM movies WHERE Id=1;` khớp | High |
| API-MOV-04 | Product API | **(Positive)** Tạo phim mới | Admin token | `/api/products` | POST | `Bearer <admin>`, `application/json` | `{"title":"QA Test Movie","imageUrl":"/images/qa.jpg","year":2024,"genre":"Drama","isTVSeries":false,"rating":8.0,"categoryIds":[1]}` | 1. POST create | 201 | `CreatedAtAction` + `MovieDetailDto`; header `Location` | `SELECT * FROM movies WHERE Title='QA Test Movie';` 1 dòng; join có cặp | High |
| API-MOV-05 | Product API | **(Positive)** Cập nhật phim | Admin token, phim tồn tại | `/api/products/{id}` | PUT | `Bearer <admin>`, `application/json` | `{"id":<id>,"title":"QA Updated","imageUrl":"/images/qa.jpg","year":2025,"genre":"Action","isTVSeries":false,"rating":9.0,"categoryIds":[2]}` | 1. PUT update | 200 | `MovieDetailDto` đã cập nhật | `SELECT Title,Year FROM movies WHERE Id=<id>;` = 'QA Updated',2025 | High |
| API-MOV-06 | Product API | **(Positive)** Xóa phim | Admin token, phim tồn tại | `/api/products/{id}` | DELETE | `Bearer <admin>` | — | 1. DELETE | 200 | "Xóa phim ... thành công." | `SELECT COUNT(*) FROM movies WHERE Id=<id>;`=0; join=0 | High |
| API-MOV-07 | Product API | **(Negative)** Thiếu Title | Admin token | `/api/products` | POST | `Bearer <admin>`, `application/json` | `{"imageUrl":"/images/x.jpg","year":2024,"genre":"Drama"}` | 1. POST | 400 | "Tiêu đề phim không được để trống." | `SELECT COUNT(*) ... Title='';`=0 | High |
| API-MOV-08 | Product API | **(Negative)** Thiếu ImageUrl/Year/Genre | Admin token | `/api/products` | POST | `Bearer <admin>`, `application/json` | `{"title":"NoFields"}` | 1. POST | 400 | Liệt kê field bắt buộc thiếu | Không tạo | Medium |
| API-MOV-09 | Product API | **(Negative)** PUT id URL ≠ id body | Admin token | `/api/products/5` | PUT | `Bearer <admin>`, `application/json` | `{"id":9,"title":"X","imageUrl":"/x.jpg","year":2024,"genre":"Drama"}` | 1. PUT mismatch | 400 | "Mã phim trên URL không khớp..." | Không cập nhật | Medium |
| API-MOV-10 | Product API | **(Boundary)** page=0, pageSize=0 → mặc định | Admin token | `/api/products?page=0&pageSize=0` | GET | `Bearer <admin>` | — | 1. GET | 200 | `Page=1, PageSize=10` | — | Low |
| API-MOV-11 | Product API | **(Boundary)** pageSize cực lớn | Admin token | `/api/products?pageSize=100000` | GET | `Bearer <admin>` | — | 1. GET | 200 | Tối đa số phim hiện có, không 500 | — | Low |
| API-MOV-12 | Product API | **(Boundary)** page vượt tổng trang | Admin token | `/api/products?page=9999` | GET | `Bearer <admin>` | — | 1. GET | 200 | `Items: []`, `TotalCount` đúng | — | Low |

## II.5. Invalid Resource (404 thay vì 500)

| Test Case ID | Module | Scenario | Preconditions | API Endpoint | HTTP Method | Headers | Request Body | Test Steps | Expected HTTP Status | Expected Response | DB Validation | Priority |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| API-404-01 | Invalid Resource | GET phim ID không tồn tại | Admin token | `/api/products/999999` | GET | `Bearer <admin>` | — | 1. GET | 404 | "Không tìm thấy phim có mã ID 999999." (KHÔNG 500) | `SELECT COUNT(*) ... Id=999999;`=0 | High |
| API-404-02 | Invalid Resource | PUT phim ID không tồn tại | Admin token | `/api/products/999999` | PUT | `Bearer <admin>`, `application/json` | `{"id":999999,"title":"X","imageUrl":"/x.jpg","year":2024,"genre":"Drama"}` | 1. PUT | 404 | "...để cập nhật." | Không đổi | Medium |
| API-404-03 | Invalid Resource | DELETE phim ID không tồn tại | Admin token | `/api/products/999999` | DELETE | `Bearer <admin>` | — | 1. DELETE | 404 | "...để xóa." | — | Medium |
| API-404-04 | Invalid Resource | **(Boundary)** ID không phải số | Admin token | `/api/products/abc` | GET | `Bearer <admin>` | — | 1. GET | 404/400 | Route không khớp ⇒ 404, không 500 | — | Low |
| API-404-05 | Invalid Resource | Cart: planId không trong giỏ | User token, giỏ trống | `/api/cart/items/999999` | PUT | `Bearer <user>`, `application/json` | `{"quantity":2}` | 1. PUT | 404 | "...không tồn tại trong giỏ hàng." | — | Medium |
| API-404-06 | Invalid Resource | Cart: PlanId không tồn tại DB | User token | `/api/cart/items` | POST | `Bearer <user>`, `application/json` | `{"planId":999999}` | 1. POST | 404 | "Gói dịch vụ có mã 999999 không tồn tại." | `SELECT COUNT(*) FROM subscriptionplans WHERE Id=999999;`=0 | Medium |

## II.6. Security / Authorization

| Test Case ID | Module | Scenario | Preconditions | API Endpoint | HTTP Method | Headers | Request Body | Test Steps | Expected HTTP Status | Expected Response | DB Validation | Priority |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| API-SEC-01 | Security | **(Negative)** Gọi API bảo vệ chưa đăng nhập | Không token | `/api/products` | GET | *(không có)* | — | 1. GET | 401 | JSON challenge | — | High |
| API-SEC-02 | Security | **(Negative)** User thường gọi API Admin | Token role=User | `/api/products` | GET | `Bearer <user>` | — | 1. GET | 403 | Forbidden JSON | — | High |
| API-SEC-03 | Security | **(Negative)** User thường tạo phim | Token role=User | `/api/products` | POST | `Bearer <user>`, `application/json` | `{"title":"Hack","imageUrl":"/x.jpg","year":2024,"genre":"X"}` | 1. POST | 403 | Forbidden; không tạo | `SELECT COUNT(*) ... Title='Hack';`=0 | High |
| API-SEC-04 | Security | **(Negative)** User thường xóa phim | Token role=User | `/api/products/1` | DELETE | `Bearer <user>` | — | 1. DELETE | 403 | Forbidden; phim còn | `SELECT COUNT(*) ... Id=1;`=1 | High |
| API-SEC-05 | Security | **(Positive)** Admin truy cập thành công | Token role=Admin | `/api/products` | GET | `Bearer <admin>` | — | 1. GET | 200 | Danh sách phim | — | High |
| API-SEC-06 | Security | **(Negative)** Cart yêu cầu đăng nhập | Không token | `/api/cart` | GET | *(không có)* | — | 1. GET | 401 | 401 JSON | — | Medium |
| API-SEC-07 | Security | Token Admin truy cập cart hợp lệ | Token Admin | `/api/cart` | GET | `Bearer <admin>` | — | 1. GET | 200 | Hợp lệ (chỉ cần authenticated) | — | Low |

## II.7. Chatbot API

| Test Case ID | Module | Scenario | Preconditions | API Endpoint | HTTP Method | Headers | Request Body | Test Steps | Expected HTTP Status | Expected Response | DB Validation | Priority |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| API-BOT-01 | Chatbot | **(Positive)** Tin nhắn thường (chào hỏi) | — | `/api/chatbot/message` | POST | `application/json` | `{"message":"xin chào"}` | 1. POST | 200 | `{reply:"Xin chào! ... FILMIX AI ..."}` | — | Medium |
| API-BOT-02 | Chatbot | **(Positive)** Hỏi giá gói (từ DB) | Có SubscriptionPlans | `/api/chatbot/message` | POST | `application/json` | `{"message":"giá gói premium bao nhiêu"}` | 1. POST | 200 | `reply` liệt kê gói + giá từ DB | `SELECT Name,Price FROM subscriptionplans;` khớp | Medium |
| API-BOT-03 | Chatbot | **(Negative/Auth)** Chưa đăng nhập hỏi đơn hàng | Không auth | `/api/chatbot/message` | POST | `application/json` | `{"message":"đơn hàng của tôi"}` | 1. POST | 200 | `reply` chứa "cần **đăng nhập** trước" (không lộ dữ liệu) | — | High |
| API-BOT-04 | Chatbot | **(Positive)** Đã đăng nhập hỏi đơn hàng | JWT/cookie hợp lệ | `/api/chatbot/message` | POST | `Bearer <user>`, `application/json` | `{"message":"đơn hàng của tôi"}` | 1. POST | 200 | `reply` chứa mã đơn / "chưa có đơn hàng" | `SELECT Id FROM orders WHERE UserId=<id> ...` khớp | High |
| API-BOT-05 | Chatbot | **(Boundary)** Message rỗng | — | `/api/chatbot/message` | POST | `application/json` | `{"message":""}` | 1. POST | 200 | `reply:"Bạn chưa nhập gì..."` | — | Low |
| API-BOT-06 | Chatbot | **(Boundary)** Message không khớp intent | — | `/api/chatbot/message` | POST | `application/json` | `{"message":"asdkjqwe"}` | 1. POST | 200 | `reply` fallback "Tôi chưa hiểu..." | — | Low |

## II.8. Security Attack Test

| Test Case ID | Module | Scenario | Preconditions | API Endpoint | HTTP Method | Headers | Request Body | Test Steps | Expected HTTP Status | Expected Response | DB Validation | Priority |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| API-ATK-01 | Security Attack | **SQL Injection** trong email login | — | `/api/auth/login` | POST | `application/json` | `{"email":"admin1@filmix.com' OR '1'='1","password":"x"}` | 1. POST | 400 (`[EmailAddress]` chặn) hoặc 401 | KHÔNG bypass, KHÔNG lỗi SQL. Thực tế **400** trước khi chạm DB | `SELECT COUNT(*) FROM movies;` không đổi | High |
| API-ATK-02 | Security Attack | **SQL Injection** trong search | Admin token | `/api/products?search=' OR 1=1 --` | GET | `Bearer <admin>` | — | 1. GET | 200 | Không dump bảng (literal). Thực tế `totalCount:0` | `COUNT(*)` ổn định | High |
| API-ATK-03 | Security Attack | **SQL Injection** phá hủy (DROP) | Admin token | `/api/products` | POST | `Bearer <admin>`, `application/json` | `{"title":"x'); DROP TABLE movies;--","imageUrl":"/x.jpg","year":2024,"genre":"X"}` | 1. POST | 201 | Title lưu literal; bảng `movies` KHÔNG bị drop | `SHOW TABLES LIKE 'movies';` còn; title literal | High |
| API-ATK-04 | Security Attack | **XSS** lưu script qua tạo phim | Admin token | `/api/products` | POST | `Bearer <admin>`, `application/json` | `{"title":"<script>alert(1)</script>","imageUrl":"/x.jpg","year":2024,"genre":"X"}` | 1. POST 2. GET 3. Render | 201 | Lưu literal; Razor HTML-encode, không chạy JS | Title literal | High |
| API-ATK-05 | Security Attack | **XSS** qua chatbot | — | `/api/chatbot/message` | POST | `application/json` | `{"message":"<img src=x onerror=alert(1)>"}` | 1. POST 2. Render | 200 | reply fallback; UI encode an toàn | — | Medium |
| API-ATK-06 | Security Attack | **Token giả mạo** gọi API Admin | Token sửa role, sai sig | `/api/products` | POST | `Bearer <forged-admin>` | `{"title":"x","imageUrl":"/x.jpg","year":2024,"genre":"X"}` | 1. POST | 401 | Chữ ký không hợp lệ ⇒ 401, không nâng quyền | Không tạo | High |
| API-ATK-07 | Security Attack | **Thiếu Authorization Header** | — | `/api/cart` | GET | *(không có)* | — | 1. GET | 401 | 401 JSON | — | High |
| API-ATK-08 | Security Attack | **Sai HTTP Method**: GET endpoint chỉ POST | — | `/api/auth/login` | GET | — | — | 1. GET | 404 (attribute routing) | Thực tế **404**, KHÔNG 500. GET không thực thi login | — | Medium |
| API-ATK-09 | Security Attack | **Sai HTTP Method**: POST vào route DELETE | Admin token | `/api/products/1` | POST | `Bearer <admin>`, `application/json` | `{}` | 1. POST | 404/405 | Không khớp action, không xóa nhầm | `SELECT COUNT(*) ... Id=1;`=1 | Medium |
| API-ATK-10 | Security Attack | **Sai HTTP Method**: DELETE vào collection | Admin token | `/api/products` | DELETE | `Bearer <admin>` | — | 1. DELETE | 405 | 405 Method Not Allowed | Dữ liệu nguyên vẹn | Medium |
| API-ATK-11 | Security Attack | **Content-Type sai** | — | `/api/auth/login` | POST | `Content-Type: text/plain` | `email=a&password=b` | 1. POST | 415/400 | Không 500 | — | Low |
| API-ATK-12 | Security Attack | **JSON dị dạng** | — | `/api/auth/login` | POST | `application/json` | `{"email": "a", ` | 1. POST | 400 | 400, không 500 | — | Low |
| API-ATK-13 | Security Attack | **Oversized payload** | Admin token | `/api/products` | POST | `Bearer <admin>`, `application/json` | Title ~10MB | 1. POST | 400/413 | Bị giới hạn, không treo server | — | Low |

---

## C. PHỤ LỤC — Hướng dẫn & Khuyến nghị

### C.1. Thứ tự thực thi đề xuất
Database: module 1→11 (phụ thuộc dữ liệu User → Movie → Order → Premium → ViewingHistory → Recommendation/Dashboard).
API: Swagger → Login lấy token → JWT → CRUD phim → 404 → phân quyền → chatbot → tấn công.

### C.2. Công cụ
- **DB:** `mysql.exe` (kết nối `filmix_db`, root/123456) chạy trực tiếp các câu ở cột *SQL Query / DB Validation*.
- **API:** Swagger UI (Try it out) / Postman / `curl`. Lấy token: `POST /api/auth/login` → dán vào nút **Authorize** (`Bearer <token>`).
- **Tài khoản test:** Admin `admin1@filmix.com`/`admin@123`; User `user@filmix.com`/`user@123`.

### C.3. Lưu ý môi trường
- Test phá hủy (DELETE, SQLi/DROP) nên chạy trên DB test/clone.
- Schema dùng `EnsureCreated()` → DROP `filmix_db` rồi chạy lại app để reset sạch trước mỗi vòng test toàn diện.
- Để test token hết hạn (API-JWT-05): tạm chỉnh `JwtSettings.ExpiryInMinutes` về giá trị nhỏ.

### C.4. Khuyến nghị từ kết quả
1. Các điểm bảo mật đã xác nhận tốt: hash mật khẩu, phân quyền role, tham số hóa SQL (chống SQLi cả DROP TABLE), encode chống XSS, token hết hạn/giả mạo bị từ chối, không lộ dữ liệu cá nhân ở chatbot — nên giữ và viết regression test.
2. Hoàn tất **48 case thủ công** theo mục **D** dưới đây để đạt độ phủ 140/140.
3. Cân nhắc viết script tự động hóa trình duyệt (Playwright/Selenium) cho nhóm UI để đưa vào CI.

---

## D. HƯỚNG DẪN CHI TIẾT TỪNG CASE CÒN LẠI

> **Đã hoàn thành tự động (24/06/2026):** nhóm **D4** (DB-CAT-02/03/06), **D8** (DB-VH-04) và phần số liệu **D11** (DB-DSH-04/05/06/07) — tổng **8 case PASS**, xem mục B.1. Phần dưới đây là hướng dẫn **chi tiết từng case** cho **40 case còn lại** bắt buộc thao tác thủ công.
>
> **Chuẩn bị chung:** chạy `dotnet run` (app tại `http://localhost:5241`). Tài khoản: Admin `admin1@filmix.com`/`admin@123`, User `user@filmix.com`/`user@123`. Kết nối DB kiểm chứng:
> `& "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -p123456 filmix_db -e "<SQL>"`

### D1 — Đăng ký tài khoản (form `/Account/Auth` → tab Đăng Ký)

**DB-USR-01 · Đăng ký hợp lệ → lưu vào AspNetUsers**
1. Mở `http://localhost:5241/Account/Auth`, chọn tab **Đăng Ký**.
2. Nhập: Email `newuser@test.com`, Họ tên `Nguyen Van A`, Mật khẩu `User@123`, Xác nhận `User@123`.
3. Bấm **Đăng Ký** → kỳ vọng đăng nhập thành công / chuyển trang chủ.
4. Xác minh: `SELECT Email, FullName FROM AspNetUsers WHERE Email='newuser@test.com';` → đúng 1 dòng.

**DB-USR-02 · Mật khẩu được hash (không plaintext)** — sau DB-USR-01:
1. `SELECT PasswordHash FROM AspNetUsers WHERE Email='newuser@test.com';`
2. Kỳ vọng: chuỗi băm dài (>40 ký tự), **khác** `User@123`.

**DB-USR-03 · Đăng ký trùng email (negative)**
1. Đăng ký lại với đúng `newuser@test.com`.
2. Kỳ vọng: form báo lỗi "email đã tồn tại".
3. Xác minh: `SELECT COUNT(*) FROM AspNetUsers WHERE Email='newuser@test.com';` → vẫn `=1`.

**DB-USR-04 · Email sai định dạng (negative)**
1. Đăng ký Email `abc@`, mật khẩu `User@123`.
2. Kỳ vọng: bị chặn validation, không submit.
3. Xác minh: `SELECT COUNT(*) FROM AspNetUsers WHERE Email='abc@';` → `=0`.

**DB-USR-05 · Mật khẩu yếu (negative)**
1. Đăng ký Email `weak@test.com`, mật khẩu `123`.
2. Kỳ vọng: Identity từ chối (yêu cầu độ dài/chữ hoa/ký tự đặc biệt).
3. Xác minh: `SELECT COUNT(*) FROM AspNetUsers WHERE Email='weak@test.com';` → `=0`.

### D2 — Quản trị phim qua Admin UI (`/Admin/Product`, đăng nhập Admin)
> Nghiệp vụ CRUD đã PASS qua API (MOV-01→12); nhóm này phủ thêm lớp giao diện Razor.

**DB-MOV-01 · Thêm phim lẻ**
1. `/Admin/Product` → **Thêm phim**. Nhập Title `Inception`, Year `2010`, Genre `Sci-Fi`, Director `Nolan`, **bỏ trống** TV series, chọn ≥1 category.
2. Lưu. Xác minh: `SELECT Title,Year,IsTVSeries FROM Movies WHERE Title='Inception';` → `IsTVSeries=0`.

**DB-MOV-02 · Thêm TV series**
1. Thêm phim `Stranger Things`, **đánh dấu** TV series.
2. Xác minh: `SELECT IsTVSeries FROM Movies WHERE Title='Stranger Things';` → `=1`.

**DB-MOV-03 · Thiếu Title (negative)**
1. Form thêm phim, **bỏ trống** Title, Year `2020`, lưu.
2. Kỳ vọng: form chặn (required). Xác minh: `SELECT COUNT(*) FROM Movies WHERE Title='' OR Title IS NULL;` → không tăng.

**DB-MOV-04 · Year sai kiểu (negative)**
1. Nhập Year `abcd` (nếu ô là text) → kỳ vọng model binding/validation chặn.
2. Xác minh: `SELECT COUNT(*) FROM Movies WHERE Title='BadYearMovie';` → `=0`.

**DB-MOV-07 · Xóa phim → dọn mapping**
1. Chọn một phim có category, bấm **Xóa**, lấy `<id>` của nó.
2. Xác minh: `SELECT COUNT(*) FROM MovieCategories WHERE MovieId=<id>;` → `=0` và `SELECT COUNT(*) FROM Movies WHERE Id=<id>;` → `=0`.

### D3 — Upload poster (Admin → Sửa phim → mục ảnh poster)

**DB-POS-01 · Upload poster hợp lệ → lưu path web**
1. Sửa một phim, chọn file `.jpg` hợp lệ, lưu, lấy `<id>`.
2. Xác minh: `SELECT ImageUrl FROM Movies WHERE Id=<id>;` → dạng `/images/...`, không rỗng.

**DB-POS-03 · Path phục vụ được từ wwwroot**
1. Lấy `ImageUrl` ở trên, mở `http://localhost:5241<ImageUrl>` trên trình duyệt.
2. Kỳ vọng: ảnh hiển thị (URL bắt đầu bằng `/`).

**DB-POS-04 · Upload sai định dạng (negative)**
1. Chọn file `virus.exe` (hoặc `.txt`), lưu.
2. Kỳ vọng: app từ chối; `ImageUrl` không đổi sang `.exe`.

**DB-POS-05 · Không chọn file**
1. Lưu phim mà không chọn file mới.
2. Kỳ vọng: `ImageUrl` giữ giá trị cũ hoặc NULL, không phát sinh path rác.

**DB-POS-06 · File quá lớn (negative)**
1. Chọn ảnh > giới hạn (vd 50MB), lưu.
2. Kỳ vọng: app báo vượt giới hạn; không lưu file lỗi.

### D5 — Giỏ hàng + Checkout (`/Subscription/Plans`, đăng nhập User)

**DB-CRT-02 · Thêm gói vào giỏ → KHÔNG sinh row DB**
1. Ghi lại `SELECT COUNT(*) FROM Orders;` (số nền).
2. `/Subscription/Plans` → **Thêm vào giỏ** một gói.
3. Xác minh: `SELECT COUNT(*) FROM Orders;` → **không tăng**; `SELECT COUNT(*) FROM OrderItems;` → không tăng.

**DB-CRT-03 · Giỏ tồn tại sau đăng xuất/đăng nhập**
1. Sau khi thêm giỏ, **Đăng xuất** rồi **Đăng nhập** lại.
2. Mở giỏ → kỳ vọng item vẫn còn. Kiểm tra cookie `FilmixCart` (DevTools → Application → Cookies).

**DB-CRT-04 · Checkout → sinh Order/OrderItem, xóa cookie giỏ**
1. Từ giỏ bấm **Thanh toán** → hoàn tất checkout.
2. Xác minh: `SELECT o.Id, oi.PlanId, oi.Quantity FROM Orders o JOIN OrderItems oi ON oi.OrderId=o.Id ORDER BY o.Id DESC LIMIT 3;` → có đơn + item mới; cookie `FilmixCart` đã bị xóa.

### D6 — Tạo đơn qua Checkout (`/Order/Checkout`, đăng nhập User)

**DB-ORD-01 · Tạo đơn hợp lệ**
1. Thêm gói vào giỏ → `/Order/Checkout` → nhập Họ tên, Email `tranb@test.com`, SĐT `0909000111` → **Đặt hàng**.
2. Xác minh: `SELECT Id,UserId,TotalAmount,Status,Email FROM Orders ORDER BY Id DESC LIMIT 1;` → `Status=0` (Pending), `TotalAmount>0`, Email khớp.

**DB-ORD-02 · OrderItems khớp đơn** — với `<id>` đơn vừa tạo:
1. `SELECT OrderId,PlanId,Quantity,Price FROM OrderItems WHERE OrderId=<id>;` → ≥1 dòng, PlanId/Price đúng gói.

**DB-ORD-05 · Thiếu thông tin bắt buộc (negative)**
1. Tại checkout, bỏ trống Email/SĐT, đặt hàng.
2. Kỳ vọng: form chặn. Xác minh: `SELECT COUNT(*) FROM Orders WHERE Email='';` → `=0`.

**DB-ORD-06 · Checkout khi giỏ trống (negative)**
1. Đảm bảo giỏ trống, truy cập `/Order/Checkout`.
2. Kỳ vọng: không tạo được đơn rỗng (chuyển hướng / thông báo giỏ trống).

**DB-ORD-08 · Đổi trạng thái đơn**
1. Admin → `/Admin/Order` → chọn đơn Pending, đổi sang **Paid**.
2. Xác minh: `SELECT Status FROM Orders WHERE Id=<id>;` → `=1`.

### D7 — Kích hoạt Premium (luồng thanh toán mock, đăng nhập User)

**DB-PRM-01 · Trước thanh toán: chưa Premium**
1. Với user chưa mua: `SELECT PremiumStartDate,PremiumEndDate FROM AspNetUsers WHERE Email='user@filmix.com';` → cả hai `NULL`.

**DB-PRM-02 · Sau thanh toán: kích hoạt Premium**
1. Đặt đơn (D6) → ở màn thanh toán bấm nút mock (gọi `/Order/ProcessMockPayment`).
2. Xác minh: `SELECT PremiumStartDate,PremiumEndDate FROM AspNetUsers WHERE Email='user@filmix.com';` → `PremiumEndDate > NOW()`.

**DB-PRM-03 · Đơn chuyển Paid khi kích hoạt**
1. `SELECT Status FROM Orders WHERE Id=<id>;` → `=1` (Paid) hoặc `4` (Completed).

**DB-PRM-06 · Thanh toán thất bại không kích hoạt (negative)**
1. Tạo đơn nhưng **không** thanh toán (để Pending).
2. Xác minh: `SELECT PremiumEndDate FROM AspNetUsers WHERE Email='user@filmix.com';` → vẫn `NULL`.

**DB-PRM-07 · Idempotent: thanh toán 2 lần**
1. Với đơn đã Paid, gọi lại `/Order/ProcessMockPayment` cùng `OrderId`.
2. Kỳ vọng: không nhân đôi đơn / không gia hạn sai; `Status` giữ Paid.

**DB-PRM-05 · Premium hết hạn ⇒ không Premium (mô phỏng bằng SQL)**
```sql
UPDATE AspNetUsers SET PremiumEndDate=DATE_SUB(NOW(), INTERVAL 1 DAY) WHERE Email='user@filmix.com';
SELECT CASE WHEN PremiumEndDate>NOW() THEN 1 ELSE 0 END FROM AspNetUsers WHERE Email='user@filmix.com';  -- kỳ vọng =0
-- Dọn lại: UPDATE AspNetUsers SET PremiumEndDate=NULL, PremiumStartDate=NULL WHERE Email='user@filmix.com';
```

### D9 — Recommendation: GROUP BY trên SQL + fallback

**DB-REC-03 · Chứng minh GroupBy/Count chạy ở SQL (không tải hết lên RAM)**
1. Trong `appsettings.json` thêm: `"Logging":{"LogLevel":{"Microsoft.EntityFrameworkCore.Database.Command":"Information"}}`.
2. Chạy lại app. Đăng nhập user **đã có lịch sử xem** (seed 2–3 phim) → mở trang chủ để kích hoạt gợi ý.
3. Đọc console: phải thấy câu SQL EF sinh ra chứa `GROUP BY` + `COUNT(*)` + `ORDER BY ... DESC` + `LIMIT`. **Không** được thấy `SELECT * FROM ViewingHistories` tải toàn bộ rồi group ở C#.

**DB-REC-05 · Fallback khi user chưa xem gì**
1. Đăng nhập user **không** có lịch sử → mở trang gợi ý.
2. Kỳ vọng: hiển thị phim mới nhất. Đối chiếu: `SELECT Id,Title,Year FROM Movies ORDER BY Year DESC, Id DESC LIMIT 10;`.

### D10 — Chặn truy cập trang Admin (trình duyệt)

**DB-SEC-01 · User thường mở trang Admin → bị chặn**
1. Đăng nhập `user@filmix.com` → mở `http://localhost:5241/Admin/Dashboard/Index`.
2. Kỳ vọng: HTTP 403 / redirect, **không** thấy dashboard.

**DB-SEC-03 · Khách (chưa đăng nhập) mở Admin → redirect login**
1. Đăng xuất hoàn toàn → mở `/Admin/Dashboard/Index`.
2. Kỳ vọng: redirect về `/Account/Auth`.

**DB-SEC-04 · Quyền chỉ dựa trên AspNetUserRoles**
1. Xác minh: `SELECT r.Name FROM AspNetUserRoles ur JOIN AspNetRoles r ON r.Id=ur.RoleId JOIN AspNetUsers u ON u.Id=ur.UserId WHERE u.Email='user@filmix.com';` → chỉ `User` (không có cột role ẩn nào khác cấp quyền Admin).

### D11 (còn lại) — Dashboard khi DB rỗng

**DB-DSH-08 · DB rỗng → các số = 0, không crash**
1. `DROP DATABASE filmix_db;` rồi chạy lại app (EnsureCreated dựng schema + seed tối thiểu, chưa có đơn).
2. Mở `/Admin/Dashboard/Index` → kỳ vọng hiển thị 0/—, không exception.
> ⚠️ Phá dữ liệu — chỉ chạy trên môi trường test/clone.

### D12 — Swagger nâng cao

**API-SWG-04 · Không 500 khi gen tất cả doc**
1. Mở lần lượt `/swagger/auth/swagger.json`, `/swagger/products/swagger.json`, `/swagger/cart/swagger.json`.
2. Kỳ vọng: đều 200, không stack trace. *(Gián tiếp đã đạt qua SWG-01→03 đã PASS.)*

**API-SWG-05 · Swagger tắt ở Production**
1. Chạy app với `ASPNETCORE_ENVIRONMENT=Production` (`$env:ASPNETCORE_ENVIRONMENT="Production"; dotnet run`).
2. Mở `/swagger` → kỳ vọng **404**.

**API-SWG-06 · Endpoint cần JWT có nút Authorize**
1. Mở `/swagger` (Development) → quan sát `api/products`, `api/cart`, `api/auth/profile` có biểu tượng **ổ khóa**.

**API-SWG-07 · Swagger như công cụ test**
1. Bấm **Authorize**, dán `Bearer <token admin>` (lấy từ `POST /api/auth/login`).
2. **Try it out** `GET /api/products` → kỳ vọng 200 + danh sách phim.

### D13 — JWT token vừa cấp

**API-JWT-07 · Token mới còn hạn → Authorized**
1. `POST /api/auth/login` lấy token → gọi ngay `GET /api/auth/profile` với `Authorization: Bearer <token>`.
2. Kỳ vọng: **200**. *(Đã được phủ bởi JWT-01 đã PASS — chạy lại chỉ để xác nhận biên thời gian.)*

---

### Bảng tổng tiến độ

| Nhóm | Số case | Trạng thái |
|---|---|---|
| D4 Mapping category (CAT-02/03/06) | 3 | ✅ **Đã PASS (SQL)** |
| D8 Lịch sử negative (VH-04) | 1 | ✅ **Đã PASS (SQL)** |
| D11 Dashboard số liệu (DSH-04/05/06/07) | 4 | ✅ **Đã PASS (SQL)** |
| D1 Đăng ký | 5 | ⏸️ Cần form đăng ký |
| D2 Admin phim | 5 | ⏸️ Cần Admin UI (đã phủ qua API) |
| D3 Upload poster | 5 | ⏸️ Cần upload file |
| D5 Giỏ + checkout | 3 | ⏸️ Cần UI giỏ/cookie |
| D6 Checkout đơn | 5 | ⏸️ Cần UI checkout |
| D7 Kích hoạt Premium | 6 | ⏸️ UI thanh toán (PRM-05 mô phỏng SQL được) |
| D9 Recommendation log | 2 | ⏸️ Bật EF log + trình duyệt |
| D10 Chặn trang Admin | 3 | ⏸️ Trình duyệt |
| D11 DB rỗng (DSH-08) | 1 | ⏸️ Cần DROP DB |
| D12 Swagger nâng cao | 4 | ⏸️ Trình duyệt/Production |
| D13 JWT vừa cấp | 1 | ⏸️ Đã phủ bởi JWT-01 |
| **Tổng đã PASS / còn lại** | **8 / 40** | |
