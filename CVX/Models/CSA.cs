using CVX.Models;

namespace CVX.Models
{
    public class CSA
    {
        public int Id { get; set; }
        public int CollaborativeId { get; set; }
        public Collaborative? Collaborative { get; set; }
        public string? Url { get; set; } // or FilePath, or both
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
