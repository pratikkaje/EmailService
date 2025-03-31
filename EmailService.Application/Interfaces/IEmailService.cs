using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailService.Domain.Models;

namespace EmailService.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailRequest emailRequest);
    }
}
