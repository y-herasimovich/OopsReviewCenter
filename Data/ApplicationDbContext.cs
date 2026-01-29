using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Models;

namespace OopsReviewCenter.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Incident> Incidents { get; set; } = null!;
    public DbSet<TimelineEvent> TimelineEvents { get; set; } = null!;
    public DbSet<ActionItem> ActionItems { get; set; } = null!;
    public DbSet<Template> Templates { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<IncidentTag> IncidentTags { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure many-to-many relationship for IncidentTag
        modelBuilder.Entity<IncidentTag>()
            .HasKey(it => new { it.IncidentId, it.TagId });

        modelBuilder.Entity<IncidentTag>()
            .HasOne(it => it.Incident)
            .WithMany(i => i.IncidentTags)
            .HasForeignKey(it => it.IncidentId);

        modelBuilder.Entity<IncidentTag>()
            .HasOne(it => it.Tag)
            .WithMany(t => t.IncidentTags)
            .HasForeignKey(it => it.TagId);

        // Configure User-Role relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Incident-ResolvedByUser relationship
        modelBuilder.Entity<Incident>()
            .HasOne(i => i.ResolvedByUser)
            .WithMany(u => u.ResolvedIncidents)
            .HasForeignKey(i => i.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
