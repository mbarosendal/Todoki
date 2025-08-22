using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TickTask.Server.Models;
using TickTask.Shared;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions options,
        IOptions<OperationalStoreOptions> operationalStoreOptions)
        : base(options, operationalStoreOptions)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One-to-one: ApplicationUser <-> UserSettings
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Settings)
            .WithOne()
            .HasForeignKey<UserSettings>(s => s.UserId)
            .IsRequired();

        // One-to-many: ApplicationUser <-> Projects
        modelBuilder.Entity<Project>()
            .HasOne<ApplicationUser>() // each project has one ApplicationUser
            .WithMany(u => u.Projects) // each user has many Projects
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
