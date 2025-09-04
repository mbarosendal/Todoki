using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Todoki.Server.Data.Models;
using Todoki.Shared;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One-to-one: ApplicationUser <-> UserSettings
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Settings)
            .WithOne(s => s.User)
            .HasForeignKey<UserSettings>(s => s.UserId)
            .IsRequired();

        // One-to-many: ApplicationUser <-> Projects
        modelBuilder.Entity<Project>()
            .HasOne(p => p.User)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.UserId)
            .IsRequired();

        // One-to-many: Project <-> TaskItems
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .IsRequired();
    }

}
