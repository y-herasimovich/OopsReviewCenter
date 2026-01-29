using FluentAssertions;
using OopsReviewCenter.Services;

namespace OopsReviewCenterTests;

/// <summary>
/// Tests to verify that the password hashes in the seed-test-data.sql script
/// work correctly with the PasswordHasher service.
/// </summary>
public class SeedDataPasswordTests
{
    private readonly PasswordHasher _passwordHasher;

    // Admin user credentials from seed-test-data.sql (UserId=1)
    private const string AdminPassword = "TestAdminPassword!@#$";
    private const string AdminSalt = "OUnH/j/xvVW/2UY2lr1ghw==";
    private const string AdminHash = "iJ6mjfNn/pLbc5ixkUW8a0/OQHHLtZSEUjJdX6+ZGnA=";

    // Viewer user credentials from seed-test-data.sql (UserId=7)
    private const string ViewerPassword = "PasswordTestUSER!!!";
    private const string ViewerSalt = "V349IS4Ym9KaKHehuXNutg==";
    private const string ViewerHash = "deZySnHC+eSCqzx1XUc+HdOFCzOvpc0UhqYXXtj2U5Y=";

    public SeedDataPasswordTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void AdminPassword_ShouldVerifyCorrectly()
    {
        // Act
        var result = _passwordHasher.VerifyPassword(AdminPassword, AdminSalt, AdminHash);

        // Assert
        result.Should().BeTrue("the admin password should verify correctly with the hash and salt from seed-test-data.sql");
    }

    [Fact]
    public void AdminPassword_WithIncorrectPassword_ShouldNotVerify()
    {
        // Arrange
        var incorrectPassword = "WrongPassword123";

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, AdminSalt, AdminHash);

        // Assert
        result.Should().BeFalse("an incorrect password should not verify");
    }

    [Fact]
    public void ViewerUserPassword_ShouldVerifyCorrectly()
    {
        // Act
        var result = _passwordHasher.VerifyPassword(ViewerPassword, ViewerSalt, ViewerHash);

        // Assert
        result.Should().BeTrue("the viewer user password should verify correctly with the hash and salt from seed-test-data.sql");
    }

    [Fact]
    public void ViewerUserPassword_WithIncorrectPassword_ShouldNotVerify()
    {
        // Arrange
        var incorrectPassword = "WrongPassword456";

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, ViewerSalt, ViewerHash);

        // Assert
        result.Should().BeFalse("an incorrect password should not verify");
    }

    [Fact]
    public void AdminPassword_RegeneratedHash_ShouldMatch()
    {
        // Act
        var regeneratedHash = _passwordHasher.HashPassword(AdminPassword, AdminSalt);

        // Assert
        regeneratedHash.Should().Be(AdminHash, "regenerating the hash with the same password and salt should produce the same result");
    }

    [Fact]
    public void ViewerUserPassword_RegeneratedHash_ShouldMatch()
    {
        // Act
        var regeneratedHash = _passwordHasher.HashPassword(ViewerPassword, ViewerSalt);

        // Assert
        regeneratedHash.Should().Be(ViewerHash, "regenerating the hash with the same password and salt should produce the same result");
    }
}
