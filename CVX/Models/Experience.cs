namespace CVX.Models
{
    public class Experience
    {
        public int Id { get; set; }
        public string Sector { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Collaborative> Collaboratives { get; set; }
    }
}
