using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Api.Controllers
{
    [Route("api/email")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
        {
            var result = await _emailService.SendEmailAsync(emailRequest);
            if (result) return Ok("Email sent successfully.");
            return BadRequest("Failed to send email.");
        }
    }
}
