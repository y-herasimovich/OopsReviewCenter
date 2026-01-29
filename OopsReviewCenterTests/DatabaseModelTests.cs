using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Data;
using OopsReviewCenter.Models;

namespace OopsReviewCenterTests;

public class DatabaseModelTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;

    public DatabaseModelTests()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Database_ShouldCreateAllTables()
    {
        // Act - Database is created in constructor

        // Assert - Verify all DbSets are accessible
        _context.Roles.Should().NotBeNull();
        _context.Users.Should().NotBeNull();
        _context.Incidents.Should().NotBeNull();
        _context.TimelineEvents.Should().NotBeNull();
        _context.ActionItems.Should().NotBeNull();
        _context.Templates.Should().NotBeNull();
        _context.Tags.Should().NotBeNull();
        _context.IncidentTags.Should().NotBeNull();
    }

    [Fact]
    public void Role_ShouldInsertAndQuery()
    {
        // Arrange
        var role = new Role
        {
            Name = "Test Role",
            Description = "Test Description"
        };

        // Act
        _context.Roles.Add(role);
        _context.SaveChanges();

        // Assert
        var savedRole = _context.Roles.FirstOrDefault();
        savedRole.Should().NotBeNull();
        savedRole!.Name.Should().Be("Test Role");
        savedRole.Description.Should().Be("Test Description");
        savedRole.RoleId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void User_ShouldInsertAndQueryWithRole()
    {
        // Arrange
        var role = new Role { Name = "Admin", Description = "Administrator" };
        _context.Roles.Add(role);
        _context.SaveChanges();

        var user = new User
        {
            RoleId = role.RoleId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Salt = "salt123",
            IsActive = true
        };

        // Act
        _context.Users.Add(user);
        _context.SaveChanges();

        // Assert
        var savedUser = _context.Users.Include(u => u.Role).FirstOrDefault();
        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("testuser");
        savedUser.Email.Should().Be("test@example.com");
        savedUser.IsActive.Should().BeTrue();
        savedUser.Role.Should().NotBeNull();
        savedUser.Role.Name.Should().Be("Admin");
    }

    [Fact]
    public void Incident_ShouldInsertAndQuery()
    {
        // Arrange
        var incident = new Incident
        {
            Title = "Test Incident",
            Description = "Test Description",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Severity = "High",
            Status = "Resolved",
            ResolvedAt = DateTime.UtcNow
        };

        // Act
        _context.Incidents.Add(incident);
        _context.SaveChanges();

        // Assert
        var savedIncident = _context.Incidents.FirstOrDefault();
        
        savedIncident.Should().NotBeNull();
        savedIncident!.Title.Should().Be("Test Incident");
        savedIncident.Status.Should().Be("Resolved");
    }

    [Fact]
    public void Incident_WithNullResolvedAt_ShouldSaveSuccessfully()
    {
        // Arrange
        var incident = new Incident
        {
            Title = "Unresolved Incident",
            Description = "Still investigating",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Severity = "Medium",
            Status = "Open"
        };

        // Act
        _context.Incidents.Add(incident);
        _context.SaveChanges();

        // Assert
        var savedIncident = _context.Incidents.FirstOrDefault();
        savedIncident.Should().NotBeNull();
        savedIncident!.ResolvedAt.Should().BeNull();
    }

    [Fact]
    public void IncidentTag_ManyToMany_ShouldWork()
    {
        // Arrange
        var tag = new Tag
        {
            Name = "Database",
            Color = "#3498db",
            CreatedAt = DateTime.UtcNow
        };
        _context.Tags.Add(tag);
        _context.SaveChanges();

        var incident = new Incident
        {
            Title = "Database Issue",
            Description = "Connection problems",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Severity = "High",
            Status = "Open"
        };
        _context.Incidents.Add(incident);
        _context.SaveChanges();

        var incidentTag = new IncidentTag
        {
            IncidentId = incident.Id,
            TagId = tag.Id
        };

        // Act
        _context.IncidentTags.Add(incidentTag);
        _context.SaveChanges();

        // Assert
        var savedIncidentTag = _context.IncidentTags.FirstOrDefault();
        savedIncidentTag.Should().NotBeNull();
        savedIncidentTag!.IncidentId.Should().Be(incident.Id);
        savedIncidentTag.TagId.Should().Be(tag.Id);
    }

    [Fact]
    public void ActionItem_ShouldLinkToIncident()
    {
        // Arrange
        var incident = new Incident
        {
            Title = "Test Incident",
            Description = "Description",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Severity = "Medium",
            Status = "Open"
        };
        _context.Incidents.Add(incident);
        _context.SaveChanges();

        var actionItem = new ActionItem
        {
            IncidentId = incident.Id,
            Title = "Fix the issue",
            Description = "Action description",
            Status = "Open",
            Priority = "High",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.ActionItems.Add(actionItem);
        _context.SaveChanges();

        // Assert
        var savedActionItem = _context.ActionItems
            .Include(a => a.Incident)
            .FirstOrDefault();
        
        savedActionItem.Should().NotBeNull();
        savedActionItem!.IncidentId.Should().Be(incident.Id);
        savedActionItem.Incident.Should().NotBeNull();
        savedActionItem.Incident!.Title.Should().Be("Test Incident");
    }

    [Fact]
    public void TimelineEvent_ShouldLinkToIncident()
    {
        // Arrange
        var incident = new Incident
        {
            Title = "Test Incident",
            Description = "Description",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Severity = "Low",
            Status = "Open"
        };
        _context.Incidents.Add(incident);
        _context.SaveChanges();

        var timelineEvent = new TimelineEvent
        {
            IncidentId = incident.Id,
            OccurredAt = DateTime.UtcNow,
            Description = "Something happened",
            Author = "System"
        };

        // Act
        _context.TimelineEvents.Add(timelineEvent);
        _context.SaveChanges();

        // Assert
        var savedEvent = _context.TimelineEvents
            .Include(t => t.Incident)
            .FirstOrDefault();
        
        savedEvent.Should().NotBeNull();
        savedEvent!.IncidentId.Should().Be(incident.Id);
        savedEvent.Incident.Should().NotBeNull();
        savedEvent.Incident!.Title.Should().Be("Test Incident");
    }

    [Fact]
    public void Incident_WithResolvedByUser_ShouldSaveAndLoadCorrectly()
    {
        // Arrange
        var role = new Role { Name = "Admin", Description = "Administrator" };
        _context.Roles.Add(role);
        _context.SaveChanges();

        var user = new User
        {
            RoleId = role.RoleId,
            Username = "resolver",
            Email = "resolver@example.com",
            PasswordHash = "hashedpassword",
            Salt = "salt123",
            IsActive = true
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        var incident = new Incident
        {
            Title = "Resolved Incident",
            Description = "This incident was resolved",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ResolvedAt = DateTime.UtcNow,
            Severity = "High",
            Status = "Resolved",
            ResolvedByUserId = user.UserId
        };

        // Act
        _context.Incidents.Add(incident);
        _context.SaveChanges();

        // Assert
        var savedIncident = _context.Incidents
            .Include(i => i.ResolvedByUser)
            .FirstOrDefault();
        
        savedIncident.Should().NotBeNull();
        savedIncident!.Title.Should().Be("Resolved Incident");
        savedIncident.ResolvedByUserId.Should().Be(user.UserId);
        savedIncident.ResolvedByUser.Should().NotBeNull();
        savedIncident.ResolvedByUser!.Username.Should().Be("resolver");
    }

    [Fact]
    public void Incident_WithoutResolvedByUser_ShouldSaveSuccessfully()
    {
        // Arrange
        var incident = new Incident
        {
            Title = "Unresolved Incident",
            Description = "Still investigating",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Severity = "Medium",
            Status = "Open"
        };

        // Act
        _context.Incidents.Add(incident);
        _context.SaveChanges();

        // Assert
        var savedIncident = _context.Incidents
            .Include(i => i.ResolvedByUser)
            .FirstOrDefault();
        
        savedIncident.Should().NotBeNull();
        savedIncident!.ResolvedByUserId.Should().BeNull();
        savedIncident.ResolvedByUser.Should().BeNull();
    }

    [Fact]
    public void Incident_WhenUserDeleted_ResolvedByUserIdShouldBeSetToNull()
    {
        // Arrange
        var role = new Role { Name = "Admin", Description = "Administrator" };
        _context.Roles.Add(role);
        _context.SaveChanges();

        var user = new User
        {
            RoleId = role.RoleId,
            Username = "tempuser",
            Email = "temp@example.com",
            PasswordHash = "hashedpassword",
            Salt = "salt123",
            IsActive = true
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        var incident = new Incident
        {
            Title = "Test Incident",
            Description = "Description",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ResolvedAt = DateTime.UtcNow,
            Severity = "High",
            Status = "Resolved",
            ResolvedByUserId = user.UserId
        };
        _context.Incidents.Add(incident);
        _context.SaveChanges();

        // Act - Delete the user
        _context.Users.Remove(user);
        _context.SaveChanges();

        // Assert - Foreign key should be set to null due to SetNull behavior
        var savedIncident = _context.Incidents.FirstOrDefault();
        savedIncident.Should().NotBeNull();
        savedIncident!.ResolvedByUserId.Should().BeNull();
        savedIncident.ResolvedByUser.Should().BeNull();
    }
}
