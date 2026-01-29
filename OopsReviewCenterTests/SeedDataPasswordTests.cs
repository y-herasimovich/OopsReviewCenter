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

    public SeedDataPasswordTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void AdminPassword_ShouldVerifyCorrectly()
    {
        // Arrange - Values from seed-test-data.sql for Admin user (UserId=1)
        var password = "TestAdminPassword!@#$";
        var salt = "OUnH/j/xvVW/2UY2lr1ghw==";
        var hash = "iJ6mjfNn/pLbc5ixkUW8a0/OQHHLtZSEUjJdX6+ZGnA=";

        // Act
        var result = _passwordHasher.VerifyPassword(password, salt, hash);

        // Assert
        result.Should().BeTrue("the admin password should verify correctly with the hash and salt from seed-test-data.sql");
    }

    [Fact]
    public void AdminPassword_WithIncorrectPassword_ShouldNotVerify()
    {
        // Arrange - Values from seed-test-data.sql for Admin user (UserId=1)
        var incorrectPassword = "WrongPassword123";
        var salt = "OUnH/j/xvVW/2UY2lr1ghw==";
        var hash = "iJ6mjfNn/pLbc5ixkUW8a0/OQHHLtZSEUjJdX6+ZGnA=";

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, salt, hash);

        // Assert
        result.Should().BeFalse("an incorrect password should not verify");
    }

    [Fact]
    public void ViewerUserPassword_ShouldVerifyCorrectly()
    {
        // Arrange - Values from seed-test-data.sql for Viewer user (UserId=7)
        var password = "PasswordTestUSER!!!";
        var salt = "V349IS4Ym9KaKHehuXNutg==";
        var hash = "deZySnHC+eSCqzx1XUc+HdOFCzOvpc0UhqYXXtj2U5Y=";

        // Act
        var result = _passwordHasher.VerifyPassword(password, salt, hash);

        // Assert
        result.Should().BeTrue("the viewer user password should verify correctly with the hash and salt from seed-test-data.sql");
    }

    [Fact]
    public void ViewerUserPassword_WithIncorrectPassword_ShouldNotVerify()
    {
        // Arrange - Values from seed-test-data.sql for Viewer user (UserId=7)
        var incorrectPassword = "WrongPassword456";
        var salt = "V349IS4Ym9KaKHehuXNutg==";
        var hash = "deZySnHC+eSCqzx1XUc+HdOFCzOvpc0UhqYXXtj2U5Y=";

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, salt, hash);

        // Assert
        result.Should().BeFalse("an incorrect password should not verify");
    }

    [Fact]
    public void AdminPassword_RegeneratedHash_ShouldMatch()
    {
        // Arrange - Regenerate the hash using the same password and salt
        var password = "TestAdminPassword!@#$";
        var salt = "OUnH/j/xvVW/2UY2lr1ghw==";
        var expectedHash = "iJ6mjfNn/pLbc5ixkUW8a0/OQHHLtZSEUjJdX6+ZGnA=";

        // Act
        var regeneratedHash = _passwordHasher.HashPassword(password, salt);

        // Assert
        regeneratedHash.Should().Be(expectedHash, "regenerating the hash with the same password and salt should produce the same result");
    }

    [Fact]
    public void ViewerUserPassword_RegeneratedHash_ShouldMatch()
    {
        // Arrange - Regenerate the hash using the same password and salt
        var password = "PasswordTestUSER!!!";
        var salt = "V349IS4Ym9KaKHehuXNutg==";
        var expectedHash = "deZySnHC+eSCqzx1XUc+HdOFCzOvpc0UhqYXXtj2U5Y=";

        // Act
        var regeneratedHash = _passwordHasher.HashPassword(password, salt);

        // Assert
        regeneratedHash.Should().Be(expectedHash, "regenerating the hash with the same password and salt should produce the same result");
    }
}
