using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EmailService.Infrastructure.Services
{
    public class OutlookEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public OutlookEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Your Name", _configuration["Outlook:Email"]));
            message.To.Add(new MailboxAddress("", emailRequest.To));
            message.Subject = emailRequest.Subject;
            message.Body = new TextPart("html") { Text = emailRequest.Body };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.office365.com", 587, false);
            await client.AuthenticateAsync(_configuration["Outlook:Email"], _configuration["Outlook:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
    }
}
