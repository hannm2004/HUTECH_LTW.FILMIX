using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using untitled1.Models.Entities;
using untitled1.Repositories;

namespace untitled1.Controllers
{
    [Route("api/chatbot")]
    [ApiController]
    public class ChatbotApiController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISubscriptionPlanRepository _planRepository;

        public ChatbotApiController(
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            ISubscriptionPlanRepository planRepository)
        {
            _orderRepository  = orderRepository;
            _userManager      = userManager;
            _planRepository   = planRepository;
        }

        [HttpPost("message")]
        public async Task<IActionResult> Message([FromBody] ChatMessageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Message))
                return Ok(new { reply = "Bạn chưa nhập gì. Hãy hỏi tôi về phim, gói dịch vụ hoặc đơn hàng nhé! 😊" });

            // L-02: Support both Cookie and JWT auth — try each scheme to populate User principal
            if (HttpContext.User.Identity?.IsAuthenticated != true)
            {
                var cookieResult = await HttpContext.AuthenticateAsync("Identity.Application");
                if (cookieResult.Succeeded && cookieResult.Principal != null)
                    HttpContext.User = cookieResult.Principal;
                else
                {
                    var jwtResult = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                    if (jwtResult.Succeeded && jwtResult.Principal != null)
                        HttpContext.User = jwtResult.Principal;
                }
            }

            var msg   = req.Message.Trim().ToLower();
            var reply = await GenerateReply(msg);

            return Ok(new { reply });
        }

        // ── Rule-based reply engine ────────────────────────────────────────────
        private async Task<string> GenerateReply(string msg)
        {
            // ── Greetings ──────────────────────────────────────────────────────
            var trimmed = msg.Trim();
            if (ContainsAny(msg, "xin chào", "xin chao", "hello", "hey")
                || trimmed == "hi" || trimmed == "chào" || trimmed == "chao"
                || msg.StartsWith("chào ") || msg.StartsWith("chao "))
                return "Xin chào! 👋 Tôi là trợ lý FILMIX AI. Tôi có thể giúp bạn về:\n• 🎬 Thông tin phim & thể loại\n• 👑 Gói Premium & giá cả\n• 📦 Đơn hàng của bạn\n• 🔧 Hỗ trợ kỹ thuật\n\nBạn cần hỏi gì ạ?";

            if (ContainsAny(msg, "cảm ơn", "cam on", "thanks", "thank you", "tuyệt", "tuyet", "ok rồi", "ok roi"))
                return "Rất vui được giúp bạn! 😊 Nếu có thêm câu hỏi, cứ hỏi tôi nhé. Chúc bạn xem phim vui!";

            // ── Plans / Pricing ─────────────────────────────────────────────────
            if (ContainsAny(msg, "gói", "goi", "plan", "premium", "giá", "gia ", "phí", "phi ",
                                 "bao nhiêu", "bao nhieu", "đăng ký", "dang ky", "mua gói", "mua goi"))
            {
                try
                {
                    var plans = await _planRepository.GetAllAsync();
                    if (plans.Any())
                    {
                        var plansText = string.Join("\n", plans.Select(p =>
                            $"• **{p.Name}** – {p.Price:N0} ₫/tháng | {p.Resolution} | {p.MaxScreens} màn hình" +
                            (p.IsPopular ? " ⭐ Phổ Biến" : "")));
                        return $"🎬 **Các gói Premium của FILMIX:**\n\n{plansText}\n\n👉 Xem chi tiết tại trang [Đăng Ký Gói Premium](/Subscription/Plans)";
                    }
                }
                catch { /* fallback below */ }
                return "👑 FILMIX có 3 gói Premium:\n• **Cơ Bản** – 79.000 ₫/tháng | 480p | 1 màn hình\n• **Tiêu Chuẩn** – 129.000 ₫/tháng | 1080p | 2 màn hình ⭐\n• **Cao Cấp** – 199.000 ₫/tháng | 4K | 4 màn hình\n\n👉 Xem chi tiết: [/Subscription/Plans](/Subscription/Plans)";
            }

            // ── Technical support ───────────────────────────────────────────────
            if (ContainsAny(msg, "lỗi", "loi", "error", "không xem được", "khong xem duoc",
                                 "buffering", "giật lag", "giat lag", "không load", "khong load",
                                 "hỗ trợ", "ho tro", "support", "sự cố", "su co"))
                return "🔧 **Gặp sự cố kỹ thuật?** Thử các bước sau:\n1. Xóa cache trình duyệt và tải lại trang\n2. Kiểm tra kết nối internet (cần ≥5Mbps cho HD)\n3. Thử trình duyệt khác (Chrome/Firefox/Edge)\n4. Tắt VPN/proxy nếu đang dùng\n\nVẫn gặp lỗi? Liên hệ: **support@filmix.vn** 📧";

            // ── Payment methods ─────────────────────────────────────────────────
            if (ContainsAny(msg, "thanh toán", "thanh toan", "payment", "vnpay", "payos",
                                 "cod", "banking", "chuyển khoản", "chuyen khoan", "momo", "ngan hang", "ngân hàng"))
                return "💳 **Phương thức thanh toán FILMIX hỗ trợ:**\n• 💳 VNPay (tự động kích hoạt ngay)\n• 🔗 PayOS (tự động kích hoạt ngay)\n• 🏦 Chuyển khoản ngân hàng (chờ Admin duyệt)\n• 📦 COD – Thanh toán khi nhận (chờ Admin duyệt)";

            // ── Order query ─────────────────────────────────────────────────────
            if (ContainsAny(msg, "đơn hàng", "don hang", "order", "mã đơn", "ma don",
                                 "trạng thái", "trang thai", "đơn của tôi", "lich su don", "lịch sử đơn"))
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var orders = await _orderRepository.GetByUserIdAsync(user.Id);
                        if (!orders.Any())
                            return "📦 Bạn chưa có đơn hàng nào. Hãy khám phá các gói Premium tại [/Subscription/Plans](/Subscription/Plans)!";

                        var latest = orders.OrderByDescending(o => o.CreatedAt).First();
                        var statusText = latest.Status switch
                        {
                            OrderStatus.Pending    => "⏳ Chờ xử lý",
                            OrderStatus.Paid       => "✅ Đã thanh toán",
                            OrderStatus.Processing => "⚙️ Đang xử lý",
                            OrderStatus.Shipping   => "🚚 Đang giao",
                            OrderStatus.Completed  => "✅ Hoàn thành",
                            OrderStatus.Cancelled  => "❌ Đã hủy",
                            _ => latest.Status.ToString()
                        };

                        return $"📦 **Đơn hàng gần nhất của bạn:**\n• Mã: **#FX-ORD-{latest.Id}**\n• Ngày đặt: {latest.CreatedAt:dd/MM/yyyy HH:mm}\n• Tổng tiền: **{latest.TotalAmount:N0} ₫**\n• Trạng thái: {statusText}\n\n👉 Xem tất cả đơn hàng tại [/Order/History](/Order/History)";
                    }
                }
                return "📦 Để kiểm tra đơn hàng, bạn cần **đăng nhập** trước nhé. Sau đó hỏi lại tôi hoặc vào [Lịch Sử Đơn Hàng](/Order/History).";
            }

            // ── Subscription check ──────────────────────────────────────────────
            if (ContainsAny(msg, "subscription", "đăng ký của tôi", "gói của tôi", "hết hạn", "còn bao lâu", "gia hạn"))
                return "👑 Để kiểm tra gói đăng ký hiện tại, bạn vào [Gói Của Tôi](/Subscription/MySubscription).\nNếu gói sắp hết hạn, bạn có thể gia hạn tại [Đăng Ký Gói](/Subscription/Plans)!";

            // ── Cancellation ────────────────────────────────────────────────────
            if (ContainsAny(msg, "hủy", "cancel", "hoàn tiền", "refund"))
                return "❌ Để hủy đơn hàng hoặc yêu cầu hoàn tiền, vui lòng liên hệ:\n• 📧 Email: **support@filmix.vn**\n• 📞 Hotline: **1800-FILMIX** (miễn phí)\n\nChúng tôi sẽ xử lý trong vòng 24 giờ làm việc.";

            // ── Screens / Devices ───────────────────────────────────────────────
            if (ContainsAny(msg, "màn hình", "screen", "thiết bị", "device", "cùng lúc", "đồng thời"))
                return "📺 **Số màn hình đồng thời theo gói:**\n• Cơ Bản: 1 màn hình\n• Tiêu Chuẩn: 2 màn hình ⭐\n• Cao Cấp: 4 màn hình\n\nXem chi tiết tại [Gói Premium](/Subscription/Plans)";

            // ── Download ────────────────────────────────────────────────────────
            if (ContainsAny(msg, "tải xuống", "tai xuong", "download", "offline", "xem offline"))
                return "📥 Tính năng **tải xuống xem offline** chỉ có ở gói **Cao Cấp** (4K).\nNâng cấp ngay tại: [/Subscription/Plans](/Subscription/Plans)";

            // ── Quality / Resolution ────────────────────────────────────────────
            if (ContainsAny(msg, "4k", "1080p", "hd", "chất lượng", "resolution", "độ phân giải"))
                return "🖥️ **Chất lượng video theo gói:**\n• Cơ Bản: SD 480p\n• Tiêu Chuẩn: Full HD 1080p ⭐\n• Cao Cấp: Ultra HD 4K\n\nNâng cấp gói tại [/Subscription/Plans](/Subscription/Plans)";

            // ── Account / Login ─────────────────────────────────────────────────
            if (ContainsAny(msg, "đăng nhập", "login", "tài khoản", "account", "mật khẩu", "password", "quên mật khẩu"))
                return "🔐 **Hỗ trợ tài khoản:**\n• Đăng nhập / Đăng ký: [/Account/Auth](/Account/Auth)\n• Quên mật khẩu: Liên hệ **support@filmix.vn**\n• Quản lý tài khoản: [/Account/Profile](/Account/Profile)";

            // ── Movies / Content ────────────────────────────────────────────────
            if (ContainsAny(msg, "phim", "movie", "film", "series", "anime", "hoạt hình", "hoat hinh",
                                 "hành động", "hanh dong", "tình cảm", "tinh cam", "kinh dị", "kinh di", "hài", "hai "))
                return "🎬 FILMIX có hàng nghìn bộ phim và series hấp dẫn!\n• **Phim Lẻ**: [/Movies](/Movies)\n• **TV Series**: [/TVShows](/TVShows)\n• **Mới & Hot**: [/NewHot](/NewHot)\n\nBạn muốn tìm thể loại phim nào? Tôi có thể gợi ý thêm 😊";

            if (ContainsAny(msg, "tìm kiếm", "tìm phim", "search", "gợi ý", "đề xuất", "recommend"))
                return "🔍 Bạn có thể tìm kiếm phim ngay trên thanh tìm kiếm phía trên!\nHoặc ghé trang tìm kiếm tại [/Search](/Search).\n\nTôi cũng có thể gợi ý theo thể loại – bạn thích phim gì? (hành động, tình cảm, kinh dị, anime...)";

            // ── Default fallback ────────────────────────────────────────────────
            return "🤔 Tôi chưa hiểu câu hỏi của bạn. Bạn có thể hỏi về:\n• 🎬 **Phim**: thể loại, tìm kiếm, gợi ý\n• 👑 **Gói Premium**: giá, tính năng, so sánh\n• 📦 **Đơn hàng**: trạng thái, lịch sử\n• 💳 **Thanh toán**: phương thức, hoàn tiền\n• 🔧 **Hỗ trợ kỹ thuật**\n\nOr email us: **support@filmix.vn**";
        }

        private static bool ContainsAny(string text, params string[] keywords)
            => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    public class ChatMessageRequest
    {
        public string? Message { get; set; }
    }
}
