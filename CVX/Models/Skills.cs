namespace CVX.Models
{
    public class Skills
    {
        public int Id { get; set; }
        public string Skill { get; set; }

        public int SkillGroupsId { get; set; }
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Collaborative> Collaboratives { get; set; }
    }
}
