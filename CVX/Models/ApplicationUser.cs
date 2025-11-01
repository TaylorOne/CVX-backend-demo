using Microsoft.AspNetCore.Identity;
using CVX.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; } = String.Empty;
    public string? LastName { get; set; } = String.Empty;
    public string? FullName => $"{FirstName} {LastName}".Trim();
    public string? Bio { get; set; } = String.Empty;
    public string? LinkedIn { get; set; } = String.Empty;
    public string? City { get; set; } = String.Empty;
    public string? State { get; set; } = String.Empty;
    public string? AvatarUrl { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public NetworkMemberStatus NetworkMemberStatus { get; set; } = NetworkMemberStatus.Applicant;

    public ICollection<Skills>? Skills { get; set; } = new List<Skills>();
    public ICollection<Experience>? Sectors { get; set; } = new List<Experience>();
}
