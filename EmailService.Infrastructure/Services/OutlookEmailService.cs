using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MimeKit;

namespace EmailService.Infrastructure.Services
{
    public class OutlookEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IConfidentialClientApplication _confidentialClient;
        private readonly ILogger<OutlookEmailService> _logger;

        public OutlookEmailService(IConfiguration configuration, ILogger<OutlookEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _confidentialClient = ConfidentialClientApplicationBuilder
                .Create(_configuration["Outlook:ClientId"])
                .WithClientSecret(_configuration["Outlook:ClientSecret"])
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_configuration["Outlook:TenantId"]}"))
                .Build();
        }

        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("App Support", _configuration["Outlook:Email"]));
                message.To.Add(new MailboxAddress("", emailRequest.To));
                message.Subject = emailRequest.Subject;

                if (emailRequest.IsHtml)
                {
                    message.Body = new TextPart("html") { Text = emailRequest.Body };
                }
                else
                {
                    message.Body = new TextPart("plain") { Text = emailRequest.Body };
                }

                _logger.LogInformation("Acquiring OAuth2 token...");
                var authResult =
                    await _confidentialClient.AcquireTokenForClient(new[] { "https://outlook.office365.com/.default" }).ExecuteAsync();
                _logger.LogInformation("OAuth2 token acquired successfully.");

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                _logger.LogInformation("Connected to SMTP server.");

                _logger.LogInformation("Authenticating with OAuth2 token...");
                await client.AuthenticateAsync(new SaslMechanismOAuth2(_configuration["Outlook:Email"], authResult.AccessToken));
                _logger.LogInformation("Authenticated successfully.");

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully.");
                return true;
            }
            catch (MsalServiceException ex)
            {
                _logger.LogError($"MSAL Service Exception: {ex.Message}");
                return false;
            }
            catch (MsalClientException ex)
            {
                _logger.LogError($"MSAL Client Exception: {ex.Message}");
                return false;
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError($"Authentication Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Exception: {ex.Message}");
                return false;
            }
        }
    }
}
