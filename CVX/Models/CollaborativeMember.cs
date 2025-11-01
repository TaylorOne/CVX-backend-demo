using System.ComponentModel.DataAnnotations;

namespace CVX.Models
{
    public class CollaborativeMember
    {
        public int Id { get; set; }

        public CollaborativeRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public InviteStatus InviteStatus { get; set; } = InviteStatus.Invited; // Default status is Invited

        public bool IsActive { get; set; } = false; // Member doesn't become active until they've 1) accepted the invite and 2) accepted the CSA

        public CSAAcceptedStatus CSAAcceptedStatus { get; set; } = CSAAcceptedStatus.NotAccepted; // Default is NotAccepted, indicating the member has yet to accept the CSA

        public int CSAAcceptedId { get; set; }

        public DateTime? CSAAcceptedAt { get; set; }

        public string? ReasonForCollabDecline { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int CollaborativeId { get; set; }
        public Collaborative Collaborative { get; set; }
    }

    public enum CSAAcceptedStatus
    {
        NotAccepted,
        Accepted,
        Declined
    }

}