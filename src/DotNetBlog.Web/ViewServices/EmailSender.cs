using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace DotNetBlog.Web.ViewServices
{
    public class EmailSender : IEmailSender
    {
        private readonly Service.EmailService _emailService;

        public EmailSender(Service.EmailService emailService)
        {
            _emailService = emailService;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return _emailService.SendAsync(email, subject, htmlMessage);
        }
    }
}
