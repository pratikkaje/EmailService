using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _sendGridApiKey;
        private readonly string _sendGridFromEmail;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _sendGridApiKey = configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException("SendGrid API Key is missing.");
            _sendGridFromEmail = configuration["SendGrid:FromEmail"] ?? throw new ArgumentNullException("From Email is missing.");
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress(_sendGridFromEmail, "App Support");
                var to = new EmailAddress(emailRequest.To);
                var msg = MailHelper.CreateSingleEmail(from, to, emailRequest.Subject, emailRequest.Body, emailRequest.IsHtml ? emailRequest.Body : null);
                var response = await client.SendEmailAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError($"SendGrid API Error: {response.StatusCode} - {responseBody}");
                    throw new ApplicationException($"Failed to send email. SendGrid returned {response.StatusCode}.");
                }

                return response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Accepted;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid email parameters.");
                throw new ApplicationException("Invalid email parameters provided.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email.");
                throw new ApplicationException("An unexpected error occurred while sending the email.", ex);
            }
        }
    }
}
