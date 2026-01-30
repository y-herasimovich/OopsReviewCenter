using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Data;
using OopsReviewCenter.Models;

namespace OopsReviewCenter.Services;

/// <summary>
/// OopsReviewCenter Authentication and Authorization Service
/// Pure application service for user authentication and role-based permission checks.
/// No HTTP dependencies - testable with plain inputs and EF DbContext.
/// </summary>
public class OopsReviewCenterAA
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;

    public OopsReviewCenterAA(
        ApplicationDbContext dbContext,
        PasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Authenticates a user with the provided credentials.
    /// Pure method - no HTTP dependencies, only DB lookup and password verification.
    /// </summary>
    /// <param name="login">Username or UserId (as string)</param>
    /// <param name="password">User's password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AuthResult with user information on success or error message on failure</returns>
    public async Task<AuthResult> AuthenticateAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failed("Username and password are required");
        }

        try
        {
            // Try to find user by username first, or by UserId if login is a number
            User? user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == login, cancellationToken);

            // If not found by username and login is a valid integer, try by UserId
            if (user == null && int.TryParse(login, out var userId))
            {
                user = await _dbContext.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            }

            // User not found
            if (user == null)
            {
                return AuthResult.Failed("User not found.");
            }

            // Check if user is active
            if (user.IsActive != true)
            {
                return AuthResult.Failed("User is not activated.");
            }

            // Validate user has a role
            if (user.Role == null)
            {
                return AuthResult.Failed("User role is not assigned.");
            }

            // Verify password using constant-time comparison
            bool passwordValid;
            try
            {
                passwordValid = _passwordHasher.VerifyPassword(password, user.Salt, user.PasswordHash);
            }
            catch
            {
                // Don't expose internal errors to prevent information leakage
                return AuthResult.Failed("Invalid credentials");
            }

            if (!passwordValid)
            {
                return AuthResult.Failed("Invalid credentials");
            }

            // Success - return user information
            return AuthResult.Succeeded(
                user.UserId,
                user.Role.Name,
                user.IsActive ?? false,
                user.Username,
                user.FullName
            );
        }
        catch
        {
            // Don't expose internal errors
            return AuthResult.Failed("An error occurred during authentication");
        }
    }

    /// <summary>
    /// Checks if a role has full administrative access.
    /// </summary>
    /// <param name="roleName">Role name to check</param>
    /// <returns>True if role has full admin access</returns>
    public bool IsAdminFull(string? roleName)
    {
        return roleName == "Administrator" || roleName == "Incident Manager";
    }

    /// <summary>
    /// Checks if a role can edit operations data.
    /// </summary>
    /// <param name="roleName">Role name to check</param>
    /// <returns>True if role can edit operations data</returns>
    public bool CanEdit(string? roleName)
    {
        return roleName == "Administrator" 
            || roleName == "Incident Manager" 
            || roleName == "Developer";
    }

    /// <summary>
    /// Checks if a role can view operations data.
    /// </summary>
    /// <param name="roleName">Role name to check</param>
    /// <returns>True if role can view operations data</returns>
    public bool CanView(string? roleName)
    {
        return roleName == "Administrator" 
            || roleName == "Incident Manager" 
            || roleName == "Developer" 
            || roleName == "Viewer";
    }
}
