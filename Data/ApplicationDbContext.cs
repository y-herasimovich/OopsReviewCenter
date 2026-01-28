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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure many-to-many relationship
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

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Tags
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Database", Color = "#3498db", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 2, Name = "API", Color = "#e74c3c", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 3, Name = "Network", Color = "#f39c12", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 4, Name = "Security", Color = "#9b59b6", CreatedAt = DateTime.UtcNow },
            new Tag { Id = 5, Name = "Performance", Color = "#1abc9c", CreatedAt = DateTime.UtcNow }
        );

        // Seed Templates
        modelBuilder.Entity<Template>().HasData(
            new Template
            {
                Id = 1,
                Name = "Standard Incident Report",
                Type = "Incident",
                Content = "## Incident Summary\n\n## Timeline\n\n## Impact\n\n## Root Cause\n\n## Action Items",
                CreatedAt = DateTime.UtcNow
            },
            new Template
            {
                Id = 2,
                Name = "Quick Action Item",
                Type = "ActionItem",
                Content = "### Task\n\n### Expected Outcome\n\n### Dependencies",
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed Incidents
        modelBuilder.Entity<Incident>().HasData(
            new Incident
            {
                Id = 1,
                Title = "Database Connection Pool Exhaustion",
                Description = "Production database connection pool was exhausted causing application timeouts",
                OccurredAt = DateTime.UtcNow.AddDays(-7),
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                ResolvedAt = DateTime.UtcNow.AddDays(-6),
                Severity = "Critical",
                Status = "Resolved",
                RootCause = "Connection pool size was too small for peak traffic. Connections were not being properly released.",
                Impact = "Service was unavailable for 45 minutes. Approximately 1,200 users affected."
            },
            new Incident
            {
                Id = 2,
                Title = "API Rate Limit Exceeded",
                Description = "Third-party API rate limits were exceeded during batch processing",
                OccurredAt = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Severity = "High",
                Status = "Investigating",
                Impact = "Batch job delayed by 2 hours. No direct user impact."
            },
            new Incident
            {
                Id = 3,
                Title = "Slow Page Load Performance",
                Description = "Users reported significantly slower page load times on the dashboard",
                OccurredAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Severity = "Medium",
                Status = "Open",
                Impact = "Dashboard load times increased from 1s to 5s average"
            }
        );

        // Seed Timeline Events
        modelBuilder.Entity<TimelineEvent>().HasData(
            new TimelineEvent
            {
                Id = 1,
                IncidentId = 1,
                OccurredAt = DateTime.UtcNow.AddDays(-7),
                Description = "First reports of timeout errors in application logs",
                Author = "Monitoring System"
            },
            new TimelineEvent
            {
                Id = 2,
                IncidentId = 1,
                OccurredAt = DateTime.UtcNow.AddDays(-7).AddMinutes(10),
                Description = "Database connection pool exhaustion confirmed",
                Author = "DevOps Team"
            },
            new TimelineEvent
            {
                Id = 3,
                IncidentId = 1,
                OccurredAt = DateTime.UtcNow.AddDays(-7).AddMinutes(30),
                Description = "Emergency fix deployed - increased connection pool size",
                Author = "DevOps Team"
            },
            new TimelineEvent
            {
                Id = 4,
                IncidentId = 2,
                OccurredAt = DateTime.UtcNow.AddDays(-3),
                Description = "Batch job failed with rate limit error",
                Author = "System"
            },
            new TimelineEvent
            {
                Id = 5,
                IncidentId = 3,
                OccurredAt = DateTime.UtcNow.AddDays(-1),
                Description = "Multiple user complaints about slow dashboard",
                Author = "Support Team"
            }
        );

        // Seed Action Items
        modelBuilder.Entity<ActionItem>().HasData(
            new ActionItem
            {
                Id = 1,
                IncidentId = 1,
                Title = "Review and optimize connection pooling configuration",
                Description = "Perform comprehensive review of database connection pooling settings",
                Status = "Completed",
                Priority = "High",
                AssignedTo = "Backend Team",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                CompletedAt = DateTime.UtcNow.AddDays(-5)
            },
            new ActionItem
            {
                Id = 2,
                IncidentId = 1,
                Title = "Add connection pool monitoring and alerts",
                Description = "Implement monitoring for connection pool metrics",
                Status = "In Progress",
                Priority = "High",
                AssignedTo = "DevOps Team",
                DueDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new ActionItem
            {
                Id = 3,
                IncidentId = 2,
                Title = "Implement rate limiting strategy for API calls",
                Description = "Add exponential backoff and queue system for batch processing",
                Status = "Open",
                Priority = "High",
                AssignedTo = "Backend Team",
                DueDate = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new ActionItem
            {
                Id = 4,
                IncidentId = 3,
                Title = "Profile dashboard queries",
                Description = "Identify slow database queries on dashboard page",
                Status = "In Progress",
                Priority = "Medium",
                AssignedTo = "Backend Team",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        );

        // Seed Incident Tags
        modelBuilder.Entity<IncidentTag>().HasData(
            new IncidentTag { IncidentId = 1, TagId = 1 }, // Database
            new IncidentTag { IncidentId = 1, TagId = 5 }, // Performance
            new IncidentTag { IncidentId = 2, TagId = 2 }, // API
            new IncidentTag { IncidentId = 3, TagId = 5 }  // Performance
        );
    }
}
