using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using Serilog;

namespace CVX.Services
{
    public class IdentityEmailSender : IEmailSender
    {
        private readonly IEmailService _emailService;

        public IdentityEmailSender(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Log.Information("Sending email to {Email} with subject {Subject}", email, subject);
            Log.Information("Sending email to {Email} with subject {Subject}", email, subject);
            Log.Information("here's the htmlMessage: {HtmlMessage}", htmlMessage);

            // You may need to adjust this to match your IEmailService signature
            await _emailService.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
