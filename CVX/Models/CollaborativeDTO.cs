namespace CVX.Models
{
    public class CollaborativeDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? LogoUrl { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public decimal CollabAdminCompensationPercent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Draft;
        public string? CsaDocUrl { get; set; }

        public int LaunchTokensCreated { get; set; }
        public decimal LaunchTokensPriorWorkPercent { get; set; }
        public int LaunchCyclePeriod { get; set; }
        public decimal LaunchTokenReleaseRate { get; set; }
        // The number of $ a launch token is worth
        public decimal LaunchTokenValue { get; set; }

        // Second launch token release must happen after collab approval, so this number is relative to that
        public int LaunchTokenSecondReleaseWeeks { get; set; }

        public ICollection<int> Skills { get; set; } = new List<int>();
        public ICollection<int> Experience { get; set; } = new List<int>();

    }
}
