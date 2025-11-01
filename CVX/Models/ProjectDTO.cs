namespace CVX.Models
{
    public class ProjectDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Budget { get; set; }

        public decimal Balance { get; set; }

        public string? AdminId { get; set; }

        public string? AdminName { get; set; }

        public string? AdminEmail { get; set; }

        public decimal AdminPay { get; set; }

        public string? CreatedAt { get; set; }

        public string? ApprovalStatus { get; set; } = "Draft";

        public int CollabId { get; set; }

        public string? CollabName { get; set; }

        public string? CollabLogoUrl { get; set; }

        public bool UserIsProjectAdmin { get; set; } = false;
    }
}
