using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Data;
using OopsReviewCenter.Models;

namespace OopsReviewCenter.Services;

/// <summary>
/// OopsReviewCenter Authentication and Authorization Service
/// Centralized service for handling user authentication, authorization, and role-based access control.
/// </summary>
public class OopsReviewCenterAA
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;
    private readonly IAuthorizationService _authorizationService;

    public OopsReviewCenterAA(
        ApplicationDbContext dbContext,
        PasswordHasher passwordHasher,
        IAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Attempts to sign in a user with the provided credentials.
    /// </summary>
    /// <param name="login">Username or email</param>
    /// <param name="password">User's password</param>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>A tuple indicating success and an optional error message</returns>
    public async Task<(bool Ok, string? Error)> SignInAsync(string login, string password, HttpContext httpContext)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Username and password are required.");
            }

            // Find user by username
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == login);

            if (user == null)
            {
                return (false, "Invalid username or password.");
            }

            // Check if user is active
            if (user.IsActive != true)
            {
                return (false, "Your account is not active. Please contact an administrator.");
            }

            // Verify password
            bool passwordValid = false;
            try
            {
                passwordValid = _passwordHasher.VerifyPassword(password, user.Salt, user.PasswordHash);
            }
            catch (Exception ex)
            {
                return (false, $"Password verification error: {ex.Message}");
            }

            if (!passwordValid)
            {
                return (false, "Invalid username or password.");
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("FullName", user.FullName ?? user.Username ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred during login: {ex.Message}");
        }
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    public async Task SignOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Gets the current authenticated user with role information.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>The current user or null if not authenticated</returns>
    public async Task<User?> GetCurrentUserAsync(HttpContext httpContext)
    {
        var userId = GetCurrentUserId(httpContext);
        if (userId == null)
        {
            return null;
        }

        return await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId.Value);
    }

    /// <summary>
    /// Gets the current user's ID from the authentication context.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>User ID or null if not authenticated</returns>
    public int? GetCurrentUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Gets the current user's ID asynchronously (for compatibility with async workflows).
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>User ID or null if not authenticated</returns>
    public Task<int?> GetCurrentUserIdAsync(HttpContext httpContext)
    {
        return Task.FromResult(GetCurrentUserId(httpContext));
    }

    /// <summary>
    /// Checks if the current user is in the specified role.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <param name="roleName">Name of the role to check</param>
    /// <returns>True if user is in the role, false otherwise</returns>
    public bool IsInRole(HttpContext httpContext, string roleName)
    {
        return httpContext.User.IsInRole(roleName);
    }

    /// <summary>
    /// Checks if the current user is in the specified role asynchronously.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <param name="roleName">Name of the role to check</param>
    /// <returns>True if user is in the role, false otherwise</returns>
    public Task<bool> IsInRoleAsync(HttpContext httpContext, string roleName)
    {
        return Task.FromResult(IsInRole(httpContext, roleName));
    }

    /// <summary>
    /// Checks if the current user satisfies the specified authorization policy.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <param name="policyName">Name of the policy to check</param>
    /// <returns>True if user satisfies the policy, false otherwise</returns>
    public async Task<bool> HasPolicyAsync(HttpContext httpContext, string policyName)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            httpContext.User,
            policyName);
        return authResult.Succeeded;
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>True if user is authenticated, false otherwise</returns>
    public bool IsAuthenticated(HttpContext httpContext)
    {
        return httpContext.User?.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Checks if the current user is authenticated asynchronously.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>True if user is authenticated, false otherwise</returns>
    public Task<bool> IsAuthenticatedAsync(HttpContext httpContext)
    {
        return Task.FromResult(IsAuthenticated(httpContext));
    }

    /// <summary>
    /// Gets the current user's full name from claims.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>User's full name or null if not authenticated</returns>
    public string? GetCurrentUserFullName(HttpContext httpContext)
    {
        return httpContext.User.FindFirst("FullName")?.Value;
    }

    /// <summary>
    /// Gets the current user's username from claims.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>Username or null if not authenticated</returns>
    public string? GetCurrentUserName(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Gets the current user's role name from claims.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>Role name or null if not authenticated</returns>
    public string? GetCurrentUserRole(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
    }
}
