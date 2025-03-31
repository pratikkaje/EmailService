using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace EmailService.Application.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _sendGridApiKey;

        public SendGridEmailService(IConfiguration configuration)
        {
            _sendGridApiKey = configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException("SendGrid API Key is missing.");
        }

        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress("your-email@example.com", "App Support");
            var to = new EmailAddress(emailRequest.To);
            var msg = MailHelper.CreateSingleEmail(from, to, emailRequest.Subject, emailRequest.Body, emailRequest.IsHtml ? emailRequest.Body : null);

            var response = await client.SendEmailAsync(msg);
            return response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Accepted;
        }
    }
}
