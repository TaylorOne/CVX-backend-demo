using Microsoft.EntityFrameworkCore;

namespace CVX.Models
{
    public class Milestone
    {
        public int Id { get; set; }

        public required string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string DefinitionOfDone { get; set; } = string.Empty;

        public string Deliverables { get; set; } = string.Empty;

        public string CompletionSummary { get; set; } = string.Empty;

        public string Feedback { get; set; } = string.Empty;

        public string ReasonForDecline { get; set; } = string.Empty;

        public bool IsComplete { get; set; } = false;

        public string ArtifactUrl { get; set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Draft;

        public AssigneeStatus AssigneeStatus { get; set; } = AssigneeStatus.Assigned;

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Precision(18, 4)]
        public decimal AllocatedLaunchTokens { get; set; }

        [Precision(18, 4)]
        public decimal AllocatedFiat { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public int AssigneeId { get; set; }
        public ProjectMember? Assignee { get; set; }
    }
}
