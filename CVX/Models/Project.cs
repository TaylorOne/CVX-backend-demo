using Microsoft.EntityFrameworkCore;

namespace CVX.Models
{
    public class Project
    {
        public int Id { get; set; }

        public required string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Draft;

        [Precision(18, 4)]
        public decimal LaunchTokenBudget { get; set; }

        [Precision(18,4)]
        public decimal LaunchTokenBalance { get; set; }

        [Precision(18, 4)]
        public decimal FiatBudget { get; set; }

        [Precision(18, 4)]
        public decimal ProjectAdminCompensationLaunchTokens { get; set; }

        [Precision(18, 4)]
        public decimal NonTeamCostsLaunchTokens { get; set; }

        [Precision(18, 4)]
        public decimal ProjectAdminCompensationFiat { get; set; }

        [Precision(18, 4)]
        public decimal NonTeamCostsFiat { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CollaborativeId { get; set; }
        public required Collaborative Collaborative { get; set; }
        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();

    }
}
