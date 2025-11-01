using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace CVX.Models
{
    public class LaunchTokenTransaction
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public int? MilestoneId { get; set; }
        public Milestone? Milestone { get; set; }
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
        public int CollaborativeId { get; set; }
        public Collaborative Collaborative { get; set; }
        public PaymentType PaymentType { get; set; }

        [Precision(18, 4)]
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum PaymentType
    {
        Milestone,

        [Display(Name = "Collab Admin")]
        CollabAdmin,

        [Display(Name = "Project Admin")]
        ProjectAdmin,

        Network
    }
}
