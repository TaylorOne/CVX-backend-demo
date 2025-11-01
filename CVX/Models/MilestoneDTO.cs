using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CVX.Models
{
    public class MilestoneDTO
    {
        public int ProjectId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DefinitionOfDone { get; set; }
        public string? Deliverables { get; set; }
        public decimal LaunchTokenAmount { get; set; }
        public string? StartDate { get; set; }
        public string? DueDate { get; set; }
        public string? AssigneeId { get; set; }
    }
}
