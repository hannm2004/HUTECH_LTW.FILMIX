using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using untitled1.Models.Entities;
using untitled1.Models.Settings;

namespace untitled1.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendOrderConfirmationAsync(Order order)
        {
            var subject = $"[FILMIX] Xác nhận đơn hàng #{order.Id} – Cảm ơn bạn đã đặt hàng!";

            // Build order items HTML safely
            var itemsHtml = "";
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    var planName = item.Plan?.Name ?? $"Gói #{item.PlanId}";
                    var price = item.Price.ToString("N0");
                    itemsHtml += $@"
                    <tr>
                        <td style='padding:12px 16px; border-bottom:1px solid #2a2a2a; color:#fff; font-weight:600;'>{planName}</td>
                        <td style='padding:12px 16px; border-bottom:1px solid #2a2a2a; color:#aaa; text-align:center;'>{item.Quantity} tháng</td>
                        <td style='padding:12px 16px; border-bottom:1px solid #2a2a2a; color:#e50914; font-weight:700; text-align:right;'>{price} ₫</td>
                    </tr>";
                }
            }

            var statusColor = order.Status == OrderStatus.Paid ? "#22c55e" : "#f59e0b";
            var statusText  = order.Status == OrderStatus.Paid ? "Đã thanh toán" : "Chờ xử lý";
            var statusMsg   = order.Status == OrderStatus.Paid
                ? "🎉 Gói Premium của bạn đã được kích hoạt! Bắt đầu thưởng thức phim ngay nhé."
                : "⏳ Đơn hàng của bạn đang chờ xác nhận. Gói Premium sẽ được kích hoạt sau khi thanh toán được xác nhận.";

            var htmlBody = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='UTF-8'/>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
  <title>Xác nhận đơn hàng FILMIX</title>
</head>
<body style='margin:0;padding:0;background:#0a0a0a;font-family:""Segoe UI"",Arial,sans-serif;'>
  <div style='max-width:600px;margin:0 auto;background:#111;border-radius:16px;overflow:hidden;box-shadow:0 20px 60px rgba(0,0,0,0.8);'>

    <!-- Header -->
    <div style='background:linear-gradient(135deg,#1a0000,#2d0808);padding:36px 32px;text-align:center;border-bottom:2px solid #e50914;'>
      <div style='font-size:2.2rem;font-weight:900;letter-spacing:-1px;color:#fff;'>
        FILM<span style='color:#e50914;'>IX</span>
      </div>
      <p style='color:#aaa;font-size:0.9rem;margin:8px 0 0;letter-spacing:1px;text-transform:uppercase;'>
        Nền Tảng Phim Trực Tuyến Hàng Đầu
      </p>
    </div>

    <!-- Body -->
    <div style='padding:36px 32px;'>
      <h1 style='color:#fff;font-size:1.6rem;font-weight:800;margin:0 0 8px;'>
        ✅ Đơn Hàng Đã Được Ghi Nhận!
      </h1>
      <p style='color:#aaa;font-size:0.95rem;margin:0 0 28px;line-height:1.6;'>
        Xin chào <strong style='color:#fff;'>{order.FullName}</strong>, cảm ơn bạn đã tin dùng FILMIX.
        Dưới đây là thông tin chi tiết đơn hàng của bạn.
      </p>

      <!-- Order Info (Table format instead of Flexbox for mail client compatibility) -->
      <table style='width:100%;background:#1a1a1a;border-radius:12px;padding:20px;margin-bottom:24px;border:1px solid #2a2a2a;border-collapse:collapse;'>
        <tr>
          <td style='color:#777;font-size:0.88rem;padding:6px 0;text-align:left;width:50%;'>Mã đơn hàng</td>
          <td style='color:#fff;font-weight:700;font-size:0.95rem;padding:6px 0;text-align:right;'>#FX-ORD-{order.Id}</td>
        </tr>
        <tr>
          <td style='color:#777;font-size:0.88rem;padding:6px 0;text-align:left;'>Ngày đặt</td>
          <td style='color:#fff;font-size:0.9rem;padding:6px 0;text-align:right;'>{order.CreatedAt:dd/MM/yyyy HH:mm}</td>
        </tr>
        <tr>
          <td style='color:#777;font-size:0.88rem;padding:6px 0;text-align:left;'>Phương thức thanh toán</td>
          <td style='color:#fff;font-size:0.9rem;font-weight:600;padding:6px 0;text-align:right;'>{order.PaymentMethod}</td>
        </tr>
        <tr>
          <td style='color:#777;font-size:0.88rem;padding:6px 0;text-align:left;'>Trạng thái</td>
          <td style='padding:6px 0;text-align:right;'>
            <span style='color:{statusColor};font-weight:700;font-size:0.88rem;background:rgba(255,255,255,0.05);padding:3px 10px;border-radius:20px;'>
              {statusText}
            </span>
          </td>
        </tr>
      </table>

      <!-- Order Items Table -->
      <h3 style='color:#fff;font-size:1rem;font-weight:700;margin:0 0 12px;'>Chi Tiết Đơn Hàng</h3>
      <table style='width:100%;border-collapse:collapse;background:#1a1a1a;border-radius:12px;overflow:hidden;'>
        <thead>
          <tr style='background:#2a2a2a;'>
            <th style='padding:12px 16px;color:#777;font-size:0.82rem;text-transform:uppercase;letter-spacing:0.5px;text-align:left;'>Gói</th>
            <th style='padding:12px 16px;color:#777;font-size:0.82rem;text-transform:uppercase;letter-spacing:0.5px;text-align:center;'>Thời hạn</th>
            <th style='padding:12px 16px;color:#777;font-size:0.82rem;text-transform:uppercase;letter-spacing:0.5px;text-align:right;'>Giá</th>
          </tr>
        </thead>
        <tbody>
          {itemsHtml}
        </tbody>
        <tfoot>
          <tr style='background:#1f1f1f;'>
            <td colspan='2' style='padding:14px 16px;color:#fff;font-weight:700;font-size:1rem;'>Tổng Cộng</td>
            <td style='padding:14px 16px;color:#e50914;font-weight:800;font-size:1.1rem;text-align:right;'>
              {order.TotalAmount.ToString("N0")} ₫
            </td>
          </tr>
        </tfoot>
      </table>

      <!-- Status Message -->
      <div style='background:rgba(255,255,255,0.04);border-left:3px solid {statusColor};border-radius:8px;padding:16px 20px;margin-top:24px;'>
        <p style='color:#ddd;font-size:0.92rem;margin:0;line-height:1.6;'>{statusMsg}</p>
      </div>

      <!-- Customer Info -->
      <div style='margin-top:28px;padding-top:24px;border-top:1px solid #222;'>
        <h3 style='color:#fff;font-size:0.95rem;font-weight:700;margin:0 0 14px;'>Thông Tin Khách Hàng</h3>
        <table style='width:100%;'>
          <tr>
            <td style='color:#777;font-size:0.88rem;padding:4px 0;width:35%;'>Email:</td>
            <td style='color:#fff;font-size:0.88rem;padding:4px 0;'>{order.Email}</td>
          </tr>
          <tr>
            <td style='color:#777;font-size:0.88rem;padding:4px 0;'>Số điện thoại:</td>
            <td style='color:#fff;font-size:0.88rem;padding:4px 0;'>{order.PhoneNumber}</td>
          </tr>
          {(string.IsNullOrEmpty(order.Address) ? "" : $"<tr><td style='color:#777;font-size:0.88rem;padding:4px 0;'>Địa chỉ:</td><td style='color:#fff;font-size:0.88rem;padding:4px 0;'>{order.Address}</td></tr>")}
        </table>
      </div>

      <!-- CTA -->
      <div style='text-align:center;margin-top:32px;'>
        <a href='https://filmix.vn' style='display:inline-block;background:linear-gradient(135deg,#e50914,#b0000d);color:#fff;text-decoration:none;padding:14px 36px;border-radius:8px;font-weight:700;font-size:1rem;letter-spacing:0.3px;'>
          🎬 Xem Phim Ngay
        </a>
      </div>
    </div>

    <!-- Footer -->
    <div style='background:#0d0d0d;padding:24px 32px;text-align:center;border-top:1px solid #1f1f1f;'>
      <p style='color:#555;font-size:0.82rem;margin:0 0 8px;'>
        Nếu bạn không thực hiện đơn hàng này, hãy liên hệ ngay: 
        <a href='mailto:support@filmix.vn' style='color:#e50914;text-decoration:none;'>support@filmix.vn</a>
      </p>
      <p style='color:#333;font-size:0.78rem;margin:0;'>
        © 2026 FILMIX Vietnam · Bảo lưu mọi quyền.
      </p>
    </div>
  </div>
</body>
</html>";

            // Save preview locally in wwwroot/emails/ for local development testing & rendering verification
            try
            {
                var targetDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emails");
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                var filePath = Path.Combine(targetDir, $"order_confirmation_{order.Id}.html");
                await File.WriteAllTextAsync(filePath, htmlBody, System.Text.Encoding.UTF8);
                _logger.LogInformation("[EmailService] Order confirmation preview saved to: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EmailService] Failed to write local email preview file");
            }

            await SendEmailAsync(order.Email, order.FullName, subject, htmlBody);
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("[EmailService] Email disabled. Would send to {Email}: {Subject}", toEmail, subject);
                return;
            }

            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                    EnableSsl   = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var mail = new MailMessage
                {
                    From            = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject         = subject,
                    SubjectEncoding = System.Text.Encoding.UTF8,
                    Body            = htmlBody,
                    BodyEncoding    = System.Text.Encoding.UTF8,
                    IsBodyHtml      = true
                };
                mail.To.Add(new MailAddress(toEmail, toName));

                await client.SendMailAsync(mail);
                _logger.LogInformation("[EmailService] Email sent successfully to {Email}: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService] Failed to send email to {Email}", toEmail);
            }
        }
    }
}
