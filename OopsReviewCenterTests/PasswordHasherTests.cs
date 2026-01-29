using FluentAssertions;
using OopsReviewCenter.Services;

namespace OopsReviewCenterTests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void GenerateSalt_ShouldReturnNonEmptyString()
    {
        // Act
        var salt = _passwordHasher.GenerateSalt();

        // Assert
        salt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateSalt_ShouldReturnDifferentSaltsOnMultipleCalls()
    {
        // Act
        var salt1 = _passwordHasher.GenerateSalt();
        var salt2 = _passwordHasher.GenerateSalt();

        // Assert
        salt1.Should().NotBe(salt2);
    }

    [Fact]
    public void HashPassword_WithSamePasswordAndSalt_ShouldReturnSameHash()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = _passwordHasher.GenerateSalt();

        // Act
        var hash1 = _passwordHasher.HashPassword(password, salt);
        var hash2 = _passwordHasher.HashPassword(password, salt);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashPassword_WithSamePasswordDifferentSalt_ShouldReturnDifferentHash()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt1 = _passwordHasher.GenerateSalt();
        var salt2 = _passwordHasher.GenerateSalt();

        // Act
        var hash1 = _passwordHasher.HashPassword(password, salt1);
        var hash2 = _passwordHasher.HashPassword(password, salt2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_WithDifferentPasswords_ShouldReturnDifferentHash()
    {
        // Arrange
        var password1 = "TestPassword123!";
        var password2 = "DifferentPassword456!";
        var salt = _passwordHasher.GenerateSalt();

        // Act
        var hash1 = _passwordHasher.HashPassword(password1, salt);
        var hash2 = _passwordHasher.HashPassword(password2, salt);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var salt = _passwordHasher.GenerateSalt();

        // Act & Assert
        var action = () => _passwordHasher.HashPassword(string.Empty, salt);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Password*");
    }

    [Fact]
    public void HashPassword_WithNullPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var salt = _passwordHasher.GenerateSalt();

        // Act & Assert
        var action = () => _passwordHasher.HashPassword(null!, salt);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Password*");
    }

    [Fact]
    public void HashPassword_WithEmptySalt_ShouldThrowArgumentException()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act & Assert
        var action = () => _passwordHasher.HashPassword(password, string.Empty);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Salt*");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = _passwordHasher.GenerateSalt();
        var hash = _passwordHasher.HashPassword(password, salt);

        // Act
        var result = _passwordHasher.VerifyPassword(password, salt, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "TestPassword123!";
        var incorrectPassword = "WrongPassword456!";
        var salt = _passwordHasher.GenerateSalt();
        var hash = _passwordHasher.HashPassword(correctPassword, salt);

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, salt, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithWrongSalt_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var correctSalt = _passwordHasher.GenerateSalt();
        var wrongSalt = _passwordHasher.GenerateSalt();
        var hash = _passwordHasher.HashPassword(password, correctSalt);

        // Act
        var result = _passwordHasher.VerifyPassword(password, wrongSalt, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_ShouldProduceBase64EncodedString()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = _passwordHasher.GenerateSalt();

        // Act
        var hash = _passwordHasher.HashPassword(password, salt);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        // Verify it's valid Base64
        var action = () => Convert.FromBase64String(hash);
        action.Should().NotThrow();
    }
}
