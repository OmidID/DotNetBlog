﻿using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace DotNetBlog.Web.ViewServices
{
    public class EmailSender : IEmailSender
    {
        private readonly Core.Service.EmailService _emailService;

        public EmailSender(Core.Service.EmailService emailService)
        {
            _emailService = emailService;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return _emailService.SendAsync(email, subject, htmlMessage);
        }
    }
}
