using untitled1.Models.Entities;

namespace untitled1.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(Order order);
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
    }
}
