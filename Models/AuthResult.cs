namespace OopsReviewCenter.Models;

/// <summary>
/// Result of an authentication attempt
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int UserId { get; set; }
    public string? RoleName { get; set; }
    public bool IsActive { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }

    public static AuthResult Failed(string errorMessage)
    {
        return new AuthResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }

    public static AuthResult Succeeded(int userId, string roleName, bool isActive, string? username = null, string? fullName = null)
    {
        return new AuthResult
        {
            Success = true,
            UserId = userId,
            RoleName = roleName,
            IsActive = isActive,
            Username = username,
            FullName = fullName
        };
    }
}
