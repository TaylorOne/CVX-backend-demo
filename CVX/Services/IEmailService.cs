namespace CVX.Services
{
    public interface IEmailService
    {
        Task SendInvitationEmailAsync(string toEmail, string inviteLink, string Email, string FullName);
        Task SendDeniedApplicantEmailAsync(ApplicationUser user, string ReasonForDenial, string NetworkAdminName, string NetworkAdminEmail);
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
