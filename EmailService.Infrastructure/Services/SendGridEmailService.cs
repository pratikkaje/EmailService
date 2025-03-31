using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _sendGridApiKey;
        private readonly string _sendGridFromEmail;

        public SendGridEmailService(IConfiguration configuration)
        {
            _sendGridApiKey = configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException("SendGrid API Key is missing.");
            _sendGridFromEmail = configuration["SendGrid:FromEmail"] ?? throw new ArgumentNullException("From Email is missing.");
        }

        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress(_sendGridFromEmail, "App Support");
            var to = new EmailAddress(emailRequest.To);
            var msg = MailHelper.CreateSingleEmail(from, to, emailRequest.Subject, emailRequest.Body, emailRequest.IsHtml ? emailRequest.Body : null);

            var response = await client.SendEmailAsync(msg);
            return response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Accepted;
        }
    }
}
