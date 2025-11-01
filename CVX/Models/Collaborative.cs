using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace CVX.Models
{
    public class Collaborative
    {
        public int Id { get; set; }

        // Nullable int to allow for no parent collaborative
        public int? ParentCollaborative { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? LogoUrl { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        // Percentage value
        [Precision(18, 4)]
        public decimal RevenueShare { get; set; }

        // Percentage value
        [Precision(18, 4)]
        public decimal IndirectCosts { get; set; }

        // Percentage value
        [Precision(18, 4)]
        public decimal CollabAdminCompensationPercent { get; set; }

        // Monthly, Quarterly, Yearly
        public string? PayoutFrequency { get; set; }

        public string? CreatorId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Draft;

        public string? ReasonForDecline { get; set; }

        public bool ReadyForSubmittal { get; set; } = false;

        public int? CurrentCSAId { get; set; }

        public CSA? CurrentCSA { get; set; }

        public ICollection<CSA> CSAs { get; set; } = new List<CSA>();

        public ICollection<Skills>? Skills { get; set; } = new List<Skills>();

        public ICollection<Experience>? Sectors { get; set; } = new List<Experience>();

        public ICollection<StakingTier>? StakingTiers { get; set; }

        public ICollection<CollaborativeMember> CollaborativeMembers { get; set; } = new List<CollaborativeMember>();
        
        public ICollection<Project> Projects { get; set; } = new List<Project>();


        // LAUNCH TOKENS

        // Total number of launch tokens created for the collaborative, 10,000 by default
        public int? LaunchTokensCreated { get; set; } = 10000;

        public decimal? LaunchTokensPriorWorkPercent { get; set; } = 0;

        // Launch cycle period in weeks, 12 weeks by default
        public int? LaunchCyclePeriod { get; set; } = 12;

        // Percentage of launch tokens distributed per cycle, 10% by default
        [Precision(18, 4)]
        public decimal? TokenReleaseRate { get; set; } = 0.10m;

        // Date when the collaborative's second cycle of launch tokens is released
        public DateTime? SecondTokenReleaseDate { get; set; }

        // Launch tokens balance (number released for use but not yet assigned)
        [Precision(18, 4)]
        public decimal LaunchTokensBalance { get; set; } = 0;

        // Last token release
        public DateTime? LastTokenRelease { get; set; }

        // Value of launch token in fiat
        public decimal LaunchTokenValue { get; set; } = 0.01m; // Default value of $0.01 per launch token

        // This number is dependent upon the date of collab approval by network admin
        // 0 is the default, meaning the first cycle begins immediately after approval
        public int? WeeksTillSecondTokenRelease { get; set; } = 0;
    }
}
