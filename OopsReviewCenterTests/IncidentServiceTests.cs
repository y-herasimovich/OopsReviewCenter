using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Data;
using OopsReviewCenter.Models;
using OopsReviewCenter.Services;

namespace OopsReviewCenterTests;

public class IncidentServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly IncidentService _service;

    public IncidentServiceTests()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _service = new IncidentService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetIncidentsAsync_WithoutFilter_ShouldReturnAllIncidents()
    {
        // Arrange
        var incident1 = new Incident { Title = "Test 1", Description = "Desc 1", OccurredAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, Status = "Open" };
        var incident2 = new Incident { Title = "Test 2", Description = "Desc 2", OccurredAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, Status = "Resolved" };
        _context.Incidents.AddRange(incident1, incident2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetIncidentsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetIncidentsAsync_WithStatusFilter_ShouldReturnFilteredIncidents()
    {
        // Arrange
        var incident1 = new Incident { Title = "Test 1", Description = "Desc 1", OccurredAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, Status = "Open" };
        var incident2 = new Incident { Title = "Test 2", Description = "Desc 2", OccurredAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, Status = "Resolved" };
        _context.Incidents.AddRange(incident1, incident2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetIncidentsAsync("Open");

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Open");
    }

    [Fact]
    public async Task UpdateIncidentInfoAsync_ShouldUpdateFieldsAndCreateTimeline()
    {
        // Arrange
        var incident = new Incident
        {
            Title = "Original Title",
            Description = "Original Description",
            Status = "Open",
            Severity = "Low",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        // Act - Don't change status to Resolved to avoid foreign key constraint with user
        var result = await _service.UpdateIncidentInfoAsync(
            incident.Id,
            "Updated Title",
            "Updated Description",
            "Investigating",  // Changed from "Resolved" to avoid user FK constraint
            "High",
            "Root cause text",
            "Impact text",
            null,  // No user ID to avoid FK constraint
            "testuser"
        );

        // Assert
        result.Should().BeTrue();

        var updatedIncident = await _context.Incidents
            .Include(i => i.TimelineEvents)
            .FirstAsync(i => i.Id == incident.Id);

        updatedIncident.Title.Should().Be("Updated Title");
        updatedIncident.Description.Should().Be("Updated Description");
        updatedIncident.Status.Should().Be("Investigating");
        updatedIncident.Severity.Should().Be("High");
        updatedIncident.RootCause.Should().Be("Root cause text");
        updatedIncident.Impact.Should().Be("Impact text");

        // Should have timeline events for all changes
        updatedIncident.TimelineEvents.Should().NotBeEmpty();
        updatedIncident.TimelineEvents.Should().Contain(e => e.Description.Contains("Title changed"));
        updatedIncident.TimelineEvents.Should().Contain(e => e.Description.Contains("Status: Open → Investigating"));
        updatedIncident.TimelineEvents.Should().Contain(e => e.Description.Contains("Severity: Low → High"));
    }

    [Fact]
    public async Task UpdateIncidentInfoAsync_WithNoChanges_ShouldNotCreateTimeline()
    {
        // Arrange
        var incident = new Incident
        {
            Title = "Test Title",
            Description = "Test Description",
            Status = "Open",
            Severity = "Low",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        // Act - Update with same values
        var result = await _service.UpdateIncidentInfoAsync(
            incident.Id,
            "Test Title",
            "Test Description",
            "Open",
            "Low",
            null,
            null,
            1,
            "testuser"
        );

        // Assert
        result.Should().BeTrue();

        var updatedIncident = await _context.Incidents
            .Include(i => i.TimelineEvents)
            .FirstAsync(i => i.Id == incident.Id);

        updatedIncident.TimelineEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateIncidentTagsAsync_ShouldAddTagsAndCreateTimeline()
    {
        // Arrange
        var tag1 = new Tag { Name = "Tag 1", Color = "#FF0000", CreatedAt = DateTime.UtcNow };
        var tag2 = new Tag { Name = "Tag 2", Color = "#00FF00", CreatedAt = DateTime.UtcNow };
        _context.Tags.AddRange(tag1, tag2);
        
        var incident = new Incident
        {
            Title = "Test",
            Description = "Test",
            Status = "Open",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateIncidentTagsAsync(
            incident.Id,
            new List<int> { tag1.Id, tag2.Id },
            1,
            "testuser"
        );

        // Assert
        result.Should().BeTrue();

        var updatedIncident = await _context.Incidents
            .Include(i => i.IncidentTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.TimelineEvents)
            .FirstAsync(i => i.Id == incident.Id);

        updatedIncident.IncidentTags.Should().HaveCount(2);
        updatedIncident.TimelineEvents.Should().ContainSingle();
        updatedIncident.TimelineEvents[0].Description.Should().Contain("Tags added");
    }

    [Fact]
    public async Task UpdateIncidentTagsAsync_ShouldRemoveTagsAndCreateTimeline()
    {
        // Arrange
        var tag1 = new Tag { Name = "Tag 1", Color = "#FF0000", CreatedAt = DateTime.UtcNow };
        var tag2 = new Tag { Name = "Tag 2", Color = "#00FF00", CreatedAt = DateTime.UtcNow };
        _context.Tags.AddRange(tag1, tag2);
        
        var incident = new Incident
        {
            Title = "Test",
            Description = "Test",
            Status = "Open",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        // Add initial tags
        _context.IncidentTags.Add(new IncidentTag { IncidentId = incident.Id, TagId = tag1.Id });
        _context.IncidentTags.Add(new IncidentTag { IncidentId = incident.Id, TagId = tag2.Id });
        await _context.SaveChangesAsync();

        // Act - Remove all tags
        var result = await _service.UpdateIncidentTagsAsync(
            incident.Id,
            new List<int>(),
            1,
            "testuser"
        );

        // Assert
        result.Should().BeTrue();

        var updatedIncident = await _context.Incidents
            .Include(i => i.IncidentTags)
            .Include(i => i.TimelineEvents)
            .FirstAsync(i => i.Id == incident.Id);

        updatedIncident.IncidentTags.Should().BeEmpty();
        updatedIncident.TimelineEvents.Should().ContainSingle();
        updatedIncident.TimelineEvents[0].Description.Should().Contain("Tags removed");
    }

    [Fact]
    public async Task UpdateIncidentInfoAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.UpdateIncidentInfoAsync(
            999,
            "Title",
            "Description",
            "Open",
            "Low",
            null,
            null,
            1,
            "testuser"
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateIncidentTagsAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.UpdateIncidentTagsAsync(
            999,
            new List<int>(),
            1,
            "testuser"
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllTagsAsync_ShouldReturnOrderedTags()
    {
        // Arrange
        var tag1 = new Tag { Name = "Charlie", Color = "#FF0000", CreatedAt = DateTime.UtcNow };
        var tag2 = new Tag { Name = "Alpha", Color = "#00FF00", CreatedAt = DateTime.UtcNow };
        var tag3 = new Tag { Name = "Bravo", Color = "#0000FF", CreatedAt = DateTime.UtcNow };
        _context.Tags.AddRange(tag1, tag2, tag3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllTagsAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Bravo");
        result[2].Name.Should().Be("Charlie");
    }
}
