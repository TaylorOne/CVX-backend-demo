using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Serilog;

namespace CVX.Services
{

    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SmtpSettings GetSmtpSettings()
        {
            return new SmtpSettings
            {
                Host = _configuration["Smtp:Host"],
                Port = int.Parse(_configuration["Smtp:Port"]),
                Username = _configuration["Smtp:Username"],
                Password = _configuration["Smtp:Password"],
                FromEmail = _configuration["Smtp:From"]
            };
        }

        public async Task SendInvitationEmailAsync(string toEmail, string inviteLink, string Email, string FullName)
        {
            var smtp = GetSmtpSettings();

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "You've been invited to join CVx: the Collaborative Value Exchange";
            message.Body = new TextPart("html")
            {
                Text = $@"
                    <img src=""https://c-vx-mantine-ui-knwx.vercel.app/assets/CVxLogoWhite.png"" alt=""CVx Logo""><br><br>
                    <h2>{FullName} ({Email}) is inviting you to join the Collaborative Value Exchange!</h2>
                    <p>Click the link below to get started:</p>
                    <p>
                        <a href=""{inviteLink}"">Accept your invitation</a>
                    </p>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{inviteLink}</p>
                    <hr>
                    <p style=""font-size:small;color:gray;"">This invitation will expire in 24 hours.</p>
                "
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtp.Host, smtp.Port, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(smtp.Username, smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendDeniedApplicantEmailAsync(ApplicationUser user, string ReasonForDenial, string NetworkAdminName, string NetworkAdminEmail)
        {
            var smtp = GetSmtpSettings();

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(user.UserName));
            message.Subject = "Your application to the Collaborative Value Exchange has been denied";
            message.Body = new TextPart("html")
            {
                Text = $@"
                    <img src=""https://c-vx-mantine-ui-knwx.vercel.app/assets/CVxLogoWhite.png"" alt=""CVx Logo""><br><br>
                    <p style=""font-size: 1.3em;"">Hi {user.FirstName}, we're sorry to inform you you've been denied access to the CVx platform.</p>
                    <p style=""font-size: 1.3em;"">
                        The reason for your denial: <b>{ReasonForDenial}</b>
                    </p>
                    <p style=""font-size: 1.3em;"">If you have any questions, please contact the Network Admin at <a href=""mailto:{NetworkAdminEmail}"">{NetworkAdminEmail}</a>.</p>
                "
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtp.Host, smtp.Port, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(smtp.Username, smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = GetSmtpSettings();

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = htmlMessage
            };
            using var client = new SmtpClient();
            await client.ConnectAsync(smtp.Host, smtp.Port, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(smtp.Username, smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        private record SmtpSettings
        {
            public string Host { get; init; }
            public int Port { get; init; }
            public string Username { get; init; }
            public string Password { get; init; }
            public string FromEmail { get; init; }
        }
    }
}
