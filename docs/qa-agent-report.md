# BÁO CÁO QA AGENT — CHẠY TỰ ĐỘNG D1 → D13

> **Tác nhân:** QA Agent FILMIX (orchestrator `qa-agent.sh`)
> **Ngày chạy:** 24/06/2026
> **Cơ chế:** mỗi bước → *thực thi → thu evidence → PASS/FAIL → dừng nếu FAIL → report cuối*.
> **Môi trường:** app `http://localhost:5241` (Development) · MySQL `filmix_db` (root/123456) · công cụ `curl`, `mysql.exe`, `openssl`.
> **Evidence:** `docs/qa-evidence/` (run.log, results.txt, jar_*.txt cookie, d*_*.txt phản hồi, poster.png).

## 🎯 KẾT QUẢ TỔNG: 68 PASS / 0 FAIL — HOÀN TẤT D1→D13

| Bước | Module | Số assertion | Kết quả |
|---|---|---|---|
| D1 | Authentication (đăng ký form + login API + JWT) | 12 | ✅ 12/12 |
| D2 | Product (CRUD API + kiểm chứng DB) | 10 | ✅ 10/10 |
| D3 | Upload poster (admin MVC multipart) | 5 | ✅ 5/5 |
| D4 | Category (mapping many-to-many) | 3 | ✅ 3/3 |
| D5 | Cart (cookie-based) | 4 | ✅ 4/4 |
| D6 | Order (checkout) | 4 | ✅ 4/4 |
| D7 | Premium activation (thanh toán mock) | 4 | ✅ 4/4 |
| D8 | Viewing History (ghi + FK negative) | 3 | ✅ 3/3 |
| D9 | Recommendation (GROUP BY/COUNT trên SQL) | 4 | ✅ 4/4 |
| D10 | Security (phân quyền API + trang Admin) | 6 | ✅ 6/6 |
| D11 | Dashboard Statistics | 4 | ✅ 4/4 |
| D12 | Swagger | 4 | ✅ 4/4 |
| D13 | Chatbot (rule-based + bảo vệ dữ liệu) | 5 | ✅ 5/5 |
| | **TỔNG** | **68** | **✅ 68 / ❌ 0** |

---

## Chi tiết từng bước (evidence)

### D1 — AUTHENTICATION ✅ 12/12
Đăng ký qua **form thật** `/Account/Register` (lấy antiforgery token + cookie jar), không phải mô phỏng.
- ✅ DB-USR-01 đăng ký hợp lệ → `{"success":true,"redirectUrl":"/"}`, 1 dòng trong AspNetUsers.
- ✅ DB-USR-02 mật khẩu hashed (PasswordHash len=84, ≠ plaintext).
- ✅ DB-USR-03 đăng ký trùng email → `success:false`, COUNT vẫn =1.
- ✅ DB-USR-04 email sai (`abc@`) → bị chặn.
- ✅ DB-USR-05 mật khẩu yếu (`123`) → Identity từ chối.
- ✅ API-AUTH-01/02 login admin & user mới → JWT hợp lệ.
- ✅ API-AUTH-03 sai email → 401.
- ✅ API-JWT-01 token hợp lệ → 200; JWT-02 thiếu token → 401; JWT-04 token giả mạo → 401.

### D2 — PRODUCT ✅ 10/10
CRUD đầy đủ qua REST API + đối chiếu DB sau mỗi thao tác.
- ✅ MOV-01 GET list (admin) → 200.
- ✅ MOV-04 tạo phim → 201; DB có dòng `QAAGENT Movie` + mapping category=1.
- ✅ MOV-03 GET by id → 200.
- ✅ MOV-05 update → 200; DB: Title=`QAAGENT Updated`, Year=2025.
- ✅ MOV-07 thiếu Title → 400.
- ✅ MOV-06 delete → 200; DB COUNT=0.

### D3 — UPLOAD POSTER ✅ 5/5
Upload **file thật** qua form admin MVC multipart (`/Admin/Product/Create`, antiforgery + cookie admin).
- ✅ Đăng nhập admin (cookie) thành công.
- ✅ Upload `poster.png` (PNG 1×1 hợp lệ) → HTTP 302 (redirect sau khi tạo).
- ✅ DB-POS-01 `ImageUrl = /images/posters/film_20260624_…png` — **path web tương đối**.
- ✅ DB-POS-02 không lưu path ổ đĩa (`C:\`/`D:\`) — 0 dòng.
- ✅ DB-POS-04 upload `.exe` → app từ chối, không lưu file `.exe` nào.

### D4 — CATEGORY ✅ 3/3
- ✅ CAT-02 phim Id=1 có 2 category (Hành Động, Kịch Tính).
- ✅ CAT-03 có 16 phim gán ≥2 category.
- ✅ CAT-05 FK chặn MovieId không tồn tại.

### D5 — CART ✅ 4/4
- ✅ Đăng nhập user (cookie).
- ✅ Thêm gói vào giỏ `/Cart/Add` → 302.
- ✅ DB-CRT-02 giỏ lưu ở **cookie `FilmixCart`** (xác nhận trong cookie jar).
- ✅ DB-CRT-02 thêm giỏ **KHÔNG** sinh Order (Orders giữ nguyên 15).

### D6 — ORDER ✅ 4/4
Checkout **thật** qua `/Order/Checkout` (cookie user + cart cookie + antiforgery).
- ✅ ORD-01 checkout → 302 redirect Payment; Order id=17 được tạo.
- ✅ ORD-02 OrderItems=1.
- ✅ ORD-03 TotalAmount = Σ(Price×Quantity) — không lệch.

### D7 — PREMIUM ACTIVATION ✅ 4/4
Thanh toán **thật** qua `/Order/ProcessMockPayment`.
- ✅ PRM-01 trước thanh toán: PremiumStartDate = NULL.
- ✅ PRM-02 thanh toán → 302; sau đó `IsPremium=true` (PremiumEndDate > NOW()).
- ✅ PRM-03 đơn chuyển Paid/Completed (Status ∈ {1,4}).

### D8 — VIEWING HISTORY ✅ 3/3
- ✅ VH-04 INSERT MovieId=999999 → **FK chặn** (`FK_ViewingHistories_Movies_MovieId`), COUNT=0.
- ✅ VH-01 ghi lịch sử xem (WatchTime=120).
- ✅ VH-02 xem lại → update (COUNT=1, WatchTime=300), không tạo bản ghi trùng.

### D9 — RECOMMENDATION ✅ 4/4
- ✅ REC-01 3 phim đã xem.
- ✅ REC-02 top category qua **GROUP BY/COUNT trên SQL** = 5 (Kịch Tính).
- ✅ REC-04 gợi ý loại trừ phim đã xem.
- ✅ REC-06 top genres = 3 thể loại.

### D10 — SECURITY ✅ 6/6
- ✅ API-SEC-01 không token → 401; SEC-02 user → 403; SEC-03 user tạo phim → 403; SEC-05 admin → 200.
- ✅ DB-SEC-01 user thường mở `/Admin/Dashboard` → 302 (bị chặn).
- ✅ DB-SEC-03 khách (chưa đăng nhập) mở `/Admin` → 302 redirect login.

### D11 — DASHBOARD ✅ 4/4
- ✅ DSH-01 tổng user ≥4; DSH-02 tổng phim ≥18.
- ✅ DSH-04 doanh thu (Status Paid/Completed) tính đúng (trong run hiển thị 2.296.000 ₫ do có đơn test id=17; sau cleanup về 2.147.000 ₫).
- ✅ DSH-07 tập đơn (0,5) và (1,4) không giao nhau.

### D12 — SWAGGER ✅ 4/4
- ✅ SWG-01 UI → 200; SWG-02 doc auth → 200; SWG-03 doc products → 200.
- ✅ SWG-07 Try-it-out: Authorize bằng token admin → `GET /api/products` trả `success:true`.

### D13 — CHATBOT ✅ 5/5
- ✅ BOT-01 chào hỏi (reply chứa "FILMIX AI").
- ✅ BOT-02 hỏi giá gói (lấy từ DB).
- ✅ BOT-03 chưa đăng nhập hỏi đơn hàng → yêu cầu "đăng nhập" (không lộ dữ liệu cá nhân).
- ✅ BOT-06 fallback.
- ✅ ATK-05 XSS qua chatbot → 200, xử lý an toàn.

---

## Ghi chú kỹ thuật & độ tin cậy

**2 lỗi harness đã gặp và khắc phục (KHÔNG phải lỗi app):**
1. **`curl: (26)` HTTP=000 ở D3** — Windows-curl (mingw) không đọc được path kiểu MSYS `/d/...` trong `-F @file`. Khắc phục: dùng path Windows `D:/...`. Sau khi sửa, upload trả 302 và lưu `ImageUrl` đúng → **app xử lý upload chính xác**.
2. **Biến `UID` readonly của bash ở D6** — bash giữ chỗ `UID` (OS uid), khiến truy vấn sai. Khắc phục: đổi tên biến `UID`→`TUID`. Sau khi sửa, D6/D7/D8/D9 chạy đúng.

> Cả hai đều là khiếm khuyết của *kịch bản kiểm thử*, không phải defect của ứng dụng. Sau khi sửa, agent chạy trọn vẹn D1→D13 không FAIL.

**Tính toàn vẹn dữ liệu (cleanup):** agent dùng user test prefix `qa_auto_<timestamp>` và phim prefix `QAAGENT`; hàm `cleanup()` xóa toàn bộ user test + order/orderitem/viewinghistory/movie phát sinh sau khi chạy. Sau run: `Movies=18`, `Orders=15`, `ViewingHistories=0`, doanh thu về `2.147.000 ₫` (baseline). **Không còn rác `qa_auto_`/`QAAGENT`.**

**⚠️ Lưu ý — 1 user lạ KHÔNG do agent tạo:** phát hiện `test_reg_valid@test.com` (FullName "Nguyen Van A") tồn tại trong DB. Đây **không** phải artifact của agent (agent dùng prefix `qa_auto_` + tên "QA Auto"), nên agent **không xóa** — đây là dữ liệu test thủ công từ trước, đề nghị bạn xác nhận trước khi dọn.

## Tài nguyên
- Kịch bản agent: `qa-agent.sh` (chạy lại: `bash qa-agent.sh`).
- Evidence: `docs/qa-evidence/` (run.log, results.txt, cookie jars, phản hồi từng bước, file ảnh test).
- Báo cáo thiết kế đầy đủ: `docs/bao-cao-test-tong-hop.md`.
