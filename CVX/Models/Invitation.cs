namespace CVX.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddHours(24);
        public bool IsUsed { get; set; } = false;
    }
}
