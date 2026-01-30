using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Data;
using OopsReviewCenter.Models;
using OopsReviewCenter.Services;

namespace OopsReviewCenterTests;

public class OopsReviewCenterAATests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly OopsReviewCenterAA _authService;

    public OopsReviewCenterAATests()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _passwordHasher = new PasswordHasher();
        _authService = new OopsReviewCenterAA(_context, _passwordHasher);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private void SeedTestUser(string username, string password, string roleName, bool isActive = true)
    {
        var role = new Role { Name = roleName, Description = $"{roleName} role" };
        _context.Roles.Add(role);
        _context.SaveChanges();

        var salt = _passwordHasher.GenerateSalt();
        var hash = _passwordHasher.HashPassword(password, salt);

        var user = new User
        {
            RoleId = role.RoleId,
            Username = username,
            Email = $"{username}@example.com",
            FullName = $"{username} Full Name",
            PasswordHash = hash,
            Salt = salt,
            IsActive = isActive
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AuthenticateAsync_WithCorrectCredentials_ShouldReturnSuccess()
    {
        // Arrange
        SeedTestUser("admin", "Admin123!", "Administrator");

        // Act
        var result = await _authService.AuthenticateAsync("admin", "Admin123!");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.UserId.Should().BeGreaterThan(0);
        result.RoleName.Should().Be("Administrator");
        result.Username.Should().Be("admin");
        result.FullName.Should().Be("admin Full Name");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticateAsync_WithWrongPassword_ShouldReturnFailure()
    {
        // Arrange
        SeedTestUser("admin", "Admin123!", "Administrator");

        // Act
        var result = await _authService.AuthenticateAsync("admin", "WrongPassword");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
        result.UserId.Should().Be(0);
        result.RoleName.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        SeedTestUser("viewer", "Viewer123!", "Viewer", isActive: false);

        // Act
        var result = await _authService.AuthenticateAsync("viewer", "Viewer123!");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("User inactive");
    }

    [Fact]
    public async Task AuthenticateAsync_WithMissingUser_ShouldReturnFailure()
    {
        // Arrange - no users seeded

        // Act
        var result = await _authService.AuthenticateAsync("nonexistent", "password");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyUsername_ShouldReturnFailure()
    {
        // Act
        var result = await _authService.AuthenticateAsync("", "password");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username and password are required.");
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyPassword_ShouldReturnFailure()
    {
        // Arrange
        SeedTestUser("admin", "Admin123!", "Administrator");

        // Act
        var result = await _authService.AuthenticateAsync("admin", "");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username and password are required.");
    }

    [Fact]
    public async Task AuthenticateAsync_WithUserId_ShouldReturnSuccess()
    {
        // Arrange
        SeedTestUser("admin", "Admin123!", "Administrator");
        var user = await _context.Users.FirstAsync();

        // Act
        var result = await _authService.AuthenticateAsync(user.UserId.ToString(), "Admin123!");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.UserId.Should().Be(user.UserId);
        result.RoleName.Should().Be("Administrator");
    }

    [Fact]
    public async Task AuthenticateAsync_WithDifferentRoles_ShouldReturnCorrectRole()
    {
        // Arrange
        SeedTestUser("dev", "Dev123!", "Developer");

        // Act
        var result = await _authService.AuthenticateAsync("dev", "Dev123!");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.RoleName.Should().Be("Developer");
    }

    [Fact]
    public void IsAdminFull_WithAdministrator_ShouldReturnTrue()
    {
        // Act
        var result = _authService.IsAdminFull("Administrator");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAdminFull_WithIncidentManager_ShouldReturnTrue()
    {
        // Act
        var result = _authService.IsAdminFull("Incident Manager");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAdminFull_WithDeveloper_ShouldReturnFalse()
    {
        // Act
        var result = _authService.IsAdminFull("Developer");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAdminFull_WithViewer_ShouldReturnFalse()
    {
        // Act
        var result = _authService.IsAdminFull("Viewer");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAdminFull_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = _authService.IsAdminFull(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEdit_WithAdministrator_ShouldReturnTrue()
    {
        // Act
        var result = _authService.CanEdit("Administrator");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEdit_WithIncidentManager_ShouldReturnTrue()
    {
        // Act
        var result = _authService.CanEdit("Incident Manager");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEdit_WithDeveloper_ShouldReturnTrue()
    {
        // Act
        var result = _authService.CanEdit("Developer");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEdit_WithViewer_ShouldReturnFalse()
    {
        // Act
        var result = _authService.CanEdit("Viewer");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEdit_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = _authService.CanEdit(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanView_WithAdministrator_ShouldReturnTrue()
    {
        // Act
        var result = _authService.CanView("Administrator");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanView_WithIncidentManager_ShouldReturnTrue()
    {
        // Act
        var result = _authService.CanView("Incident Manager");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanView_WithDeveloper_ShouldReturnTrue()
    {
        // Act
        var result = _authService.CanView("Developer");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanView_WithViewer_ShouldReturnTrue()
    {
        // Act
        var result = _authService.CanView("Viewer");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanView_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = _authService.CanView(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_SamePasswordWithDifferentSalt_ShouldProduceDifferentHash()
    {
        // This test verifies that the same password produces different hashes with different salts
        // Arrange
        var password = "TestPassword123!";
        
        var role = new Role { Name = "Test", Description = "Test role" };
        _context.Roles.Add(role);
        _context.SaveChanges();

        var salt1 = _passwordHasher.GenerateSalt();
        var hash1 = _passwordHasher.HashPassword(password, salt1);

        var user1 = new User
        {
            RoleId = role.RoleId,
            Username = "user1",
            Email = "user1@example.com",
            PasswordHash = hash1,
            Salt = salt1,
            IsActive = true
        };

        var salt2 = _passwordHasher.GenerateSalt();
        var hash2 = _passwordHasher.HashPassword(password, salt2);

        var user2 = new User
        {
            RoleId = role.RoleId,
            Username = "user2",
            Email = "user2@example.com",
            PasswordHash = hash2,
            Salt = salt2,
            IsActive = true
        };

        _context.Users.AddRange(user1, user2);
        _context.SaveChanges();

        // Assert - Different salts produce different hashes
        hash1.Should().NotBe(hash2);
        
        // Act - Both users should authenticate successfully with the same password
        var result1 = await _authService.AuthenticateAsync("user1", password);
        var result2 = await _authService.AuthenticateAsync("user2", password);

        // Assert
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
    }
}
