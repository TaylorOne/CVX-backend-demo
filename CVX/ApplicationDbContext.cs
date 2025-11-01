using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CVX.Models;


public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StakingTier>()
            .HasOne(st => st.Collaborative)
            .WithMany(c => c.StakingTiers)
            .HasForeignKey(st => st.CollaborativeId)
            .OnDelete(DeleteBehavior.Cascade); // Enable cascade delete

        modelBuilder.Entity<Collaborative>()
            .HasMany(c => c.CSAs)
            .WithOne(csa => csa.Collaborative)
            .HasForeignKey(csa => csa.CollaborativeId)
            .OnDelete(DeleteBehavior.Cascade); // Enable cascade delete

        modelBuilder.Entity<Collaborative>()
            .HasOne(c => c.CurrentCSA)
            .WithOne()
            .HasForeignKey<Collaborative>(c => c.CurrentCSAId)
        .OnDelete(DeleteBehavior.NoAction);
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<Collaborative> Collaboratives { get; set; }
    public DbSet<CollaborativeMember> CollaborativeMembers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<LaunchTokenTransaction> LaunchTokenTransactions { get; set; }
    public DbSet<CSA> CSAs { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<StakingTier> StakingTiers { get; set; }
    public DbSet<Experience> Sectors { get; set; }
    public DbSet<Skills> Skills { get; set; }
    public DbSet<SkillGroups> SkillGroups { get; set; }

}   