using Microsoft.EntityFrameworkCore;

namespace CVX.Models
{
    public class StakingTier
    {
        public int Id { get; set; }
        public string Tier { get; set; } // Name of the staking tier

        [Precision(18, 4)]
        public decimal ExchangeRate { get; set; } // Percentage associated with the staking tier
        public int? CollaborativeId { get; set; } // Foreign key to the Collaboratives table
        public Collaborative? Collaborative { get; set; } // Navigation property to the Collaboratives table
    }
}
