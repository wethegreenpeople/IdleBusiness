using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Helpers
{
    public class MailHelper : IEmailSender
    {
        private readonly IConfiguration _config;

        public MailHelper(IConfiguration configuration)
        {
            _config = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _config.GetValue<string>("SendGrid:ApiKey");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("DONOTREPLY@idlebusiness.com", "Idle Business"),
                Subject = subject,
                HtmlContent = htmlMessage
            };
            msg.AddTo(new EmailAddress(email));
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}
