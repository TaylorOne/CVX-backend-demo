namespace CVX.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }

        public ProjectRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public InviteStatus InviteStatus { get; set; } = InviteStatus.Invited;

        public bool IsActive { get; set; } = false; // Member doesn't become active until they've approved the project

        public string? ReasonForProjectDecline { get; set; }

        public string? ReasonForProjectInviteDecline { get; set; }

        public required string UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}
