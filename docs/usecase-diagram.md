# Sơ Đồ Use Case Tổng Quát — FILMIX

Bản rút gọn (vừa khổ A4), đúng chuẩn UML:

- **Actor ↔ use case**: chỉ nối **association** (đường liền, không có quan hệ include/extend).
- **`«include»` / `«extend»`**: chỉ nối **use case với use case**.
- Google/Facebook, Cổng thanh toán, SMTP là **actor phụ** (hệ thống ngoài) đặt bên phải, nối association tới đúng use case dùng chúng.

```mermaid
%%{init: {'flowchart': {'curve': 'basis', 'nodeSpacing': 25, 'rankSpacing': 55}, 'theme': 'neutral'}}%%
graph LR
    %% ---------- ACTOR CON NGƯỜI (TRÁI) ----------
    KHACH["👤 Khách"]
    USER["👤 Người dùng"]
    ADMIN["🛡️ Quản trị viên"]
    USER -. kế thừa .-> KHACH
    ADMIN -. kế thừa .-> USER

    %% ---------- RANH GIỚI HỆ THỐNG (GIỮA) ----------
    subgraph SYSTEM["🎬  HỆ THỐNG FILMIX"]
        direction TB
        U1(("Xác thực &<br/>tài khoản"))
        MXH(("Đăng nhập<br/>mạng xã hội"))
        U2(("Duyệt &<br/>xem phim"))
        U3(("Tìm kiếm phim"))
        U4(("Danh sách<br/>của tôi"))
        U5(("Mua gói<br/>Premium"))
        PAY(("Thanh toán"))
        MAIL(("Gửi email<br/>xác nhận"))
        U6(("Chatbot<br/>hỗ trợ"))
        U7(("Quản trị<br/>phim"))
        U8(("Quản trị người<br/>dùng & gói"))
        U9(("Quản trị<br/>đơn hàng"))
        U10(("Thống kê &<br/>phân tích"))
        LOG(("Ghi nhật ký<br/>hệ thống"))

        %% ----- «include» / «extend»: CHỈ giữa các use case -----
        MXH -. «extend» .-> U1
        U5 -. «include» .-> PAY
        U5 -. «include» .-> MAIL
        U7 -. «include» .-> LOG
        U8 -. «include» .-> LOG
        U9 -. «include» .-> LOG
    end

    %% ---------- ACTOR HỆ THỐNG NGOÀI (PHẢI) ----------
    OAUTH["🔌 Google /<br/>Facebook"]
    PAYGATE["💳 Cổng<br/>thanh toán"]
    SMTP["📧 SMTP<br/>Gmail"]

    %% ---------- Association: actor người — use case ----------
    KHACH --- U1
    KHACH --- U2
    KHACH --- U3
    KHACH --- U6
    USER --- U4
    USER --- U5
    ADMIN --- U7
    ADMIN --- U8
    ADMIN --- U9
    ADMIN --- U10

    %% ---------- Association: use case — actor hệ thống ngoài ----------
    MXH --- OAUTH
    PAY --- PAYGATE
    MAIL --- SMTP

    %% ---------- STYLE ----------
    classDef human   fill:#1e3a8a,stroke:#93c5fd,color:#fff,font-weight:bold;
    classDef machine fill:#7c2d12,stroke:#fdba74,color:#fff,font-weight:bold;
    class KHACH,USER,ADMIN human;
    class OAUTH,PAYGATE,SMTP machine;
    style SYSTEM fill:#0f172a,stroke:#e50914,stroke-width:3px,color:#fff;
```

## Quan hệ trong sơ đồ

| Loại quan hệ | Cặp | Ý nghĩa |
|---|---|---|
| `«extend»` | Đăng nhập MXH → Xác thực & tài khoản | Đăng nhập Google/Facebook là luồng mở rộng tùy chọn của xác thực |
| `«include»` | Mua gói Premium → Thanh toán | Mọi đơn mua đều bắt buộc qua bước thanh toán |
| `«include»` | Mua gói Premium → Gửi email xác nhận | Sau thanh toán luôn gửi email xác nhận |
| `«include»` | Quản trị phim / người dùng / đơn hàng → Ghi nhật ký | Thao tác quản trị luôn ghi audit log |
| association | Đăng nhập MXH — Google/Facebook | Actor phụ cung cấp OAuth |
| association | Thanh toán — Cổng thanh toán | Actor phụ xử lý giao dịch (mock) |
| association | Gửi email xác nhận — SMTP Gmail | Actor phụ gửi thư đi |

## Diễn giải gói chức năng

| Use case | Gồm các chức năng | Controller chính |
|---|---|---|
| **Xác thực & tài khoản** | Đăng ký, đăng nhập, đăng xuất, hồ sơ | `AccountController` |
| **Đăng nhập mạng xã hội** | OAuth Google/Facebook | `AccountController.ExternalLogin` |
| **Duyệt & xem phim** | Trang chủ, phim lẻ, TV series, mới & nổi bật, chi tiết, xem phim/teaser, đề xuất, ghi tiến độ xem | `Home/Movies/TVShows/NewHot/ProductController`, `ViewingHistoryController` |
| **Tìm kiếm phim** | Tìm kiếm + gợi ý tự động | `SearchController` |
| **Danh sách của tôi** | Xem / thêm / xóa watchlist, đồng bộ CSDL | `WatchlistController` |
| **Mua gói Premium** | Xem gói, giỏ hàng, checkout, kích hoạt Premium, lịch sử đơn | `Subscription/Cart/OrderController` |
| **Thanh toán** | Xử lý thanh toán mock | `OrderController.ProcessMockPayment` |
| **Gửi email xác nhận** | Gửi mail hóa đơn qua SMTP | `EmailService` |
| **Chatbot hỗ trợ** | Hỏi đáp theo intent | `ChatbotApiController` |
| **Quản trị phim** | CRUD phim + upload poster | Admin `ProductController` |
| **Quản trị người dùng & gói** | Cấp/thu quyền Admin, Premium, quản lý gói | Admin `UserController`, `SubscriptionController` |
| **Quản trị đơn hàng** | Danh sách, đổi trạng thái, đồng bộ vòng đời | Admin `OrderController` |
| **Thống kê & phân tích** | Dashboard, phân tích top phim/thể loại | Admin `Dashboard/AnalyticsController` |
| **Ghi nhật ký hệ thống** | Audit log mọi thao tác quan trọng | `LogService`, Admin `SystemLogController` |

> Admin và Người dùng kế thừa toàn bộ quyền của Khách (generalization). Xuất ra A4 nên để **khổ ngang (landscape)** cho cân đối.
