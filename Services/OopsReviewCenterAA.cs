using System.Collections.Concurrent;
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
    
    // Simple in-memory session storage
    private static readonly ConcurrentDictionary<string, UserSession> _sessions = new();
    
    private class UserSession
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public OopsReviewCenterAA(
        ApplicationDbContext dbContext,
        PasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Attempts to sign in a user with the provided credentials.
    /// </summary>
    /// <param name="login">Username</param>
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

            // Validate HttpContext
            if (httpContext == null)
            {
                return (false, "Unable to authenticate. Please try again.");
            }

            // Find user by username
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == login);

            if (user == null)
            {
                return (false, "Invalid username or password.");
            }

            // Validate user has a role
            if (user.Role == null)
            {
                return (false, "Unable to authenticate. Please contact an administrator.");
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
            catch
            {
                // Log the exception internally in production systems
                return (false, "Unable to verify credentials. Please try again.");
            }

            if (!passwordValid)
            {
                return (false, "Invalid username or password.");
            }

            // Create session
            var sessionId = Guid.NewGuid().ToString();
            var session = new UserSession
            {
                UserId = user.UserId,
                Username = user.Username ?? user.UserId.ToString(),
                FullName = user.FullName ?? user.Username ?? "User",
                RoleName = user.Role.Name,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };
            
            // Store session
            _sessions[sessionId] = session;
            
            // Set session cookie
            httpContext.Response.Cookies.Append("SessionId", sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            return (true, null);
        }
        catch
        {
            // Log the exception internally in production systems
            return (false, "An error occurred during login. Please try again later.");
        }
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    public Task SignOutAsync(HttpContext httpContext)
    {
        // Get session ID from cookie
        if (httpContext.Request.Cookies.TryGetValue("SessionId", out var sessionId))
        {
            // Remove session from storage
            _sessions.TryRemove(sessionId, out _);
            
            // Delete session cookie
            httpContext.Response.Cookies.Delete("SessionId");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the current user session.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>User session or null if not authenticated or expired</returns>
    private UserSession? GetCurrentSession(HttpContext httpContext)
    {
        if (!httpContext.Request.Cookies.TryGetValue("SessionId", out var sessionId))
        {
            return null;
        }
        
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return null;
        }
        
        // Check if session has expired
        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _sessions.TryRemove(sessionId, out _);
            return null;
        }
        
        return session;
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
        var session = GetCurrentSession(httpContext);
        return session?.UserId;
    }

    /// <summary>
    /// Gets the current user's ID asynchronously (for compatibility with async workflows).
    /// Note: This is a thin wrapper around GetCurrentUserId for API consistency.
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
        var session = GetCurrentSession(httpContext);
        return session?.RoleName == roleName;
    }

    /// <summary>
    /// Checks if the current user is in the specified role asynchronously.
    /// Note: This is a thin wrapper around IsInRole for API consistency.
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
    public Task<bool> HasPolicyAsync(HttpContext httpContext, string policyName)
    {
        var session = GetCurrentSession(httpContext);
        if (session == null)
        {
            return Task.FromResult(false);
        }
        
        // Check policy based on role
        bool hasPolicy = policyName switch
        {
            "AdminFullAccess" => session.RoleName == "Administrator" || session.RoleName == "Incident Manager",
            "CanEditOpsData" => session.RoleName == "Administrator" || session.RoleName == "Incident Manager" || session.RoleName == "Developer",
            "CanViewOpsData" => session.RoleName == "Administrator" || session.RoleName == "Incident Manager" || session.RoleName == "Developer" || session.RoleName == "Viewer",
            _ => false
        };
        
        return Task.FromResult(hasPolicy);
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>True if user is authenticated, false otherwise</returns>
    public bool IsAuthenticated(HttpContext httpContext)
    {
        return GetCurrentSession(httpContext) != null;
    }

    /// <summary>
    /// Checks if the current user is authenticated asynchronously.
    /// Note: This is a thin wrapper around IsAuthenticated for API consistency.
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
        var session = GetCurrentSession(httpContext);
        return session?.FullName;
    }

    /// <summary>
    /// Gets the current user's username from claims.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>Username or null if not authenticated</returns>
    public string? GetCurrentUserName(HttpContext httpContext)
    {
        var session = GetCurrentSession(httpContext);
        return session?.Username;
    }

    /// <summary>
    /// Gets the current user's role name from claims.
    /// </summary>
    /// <param name="httpContext">Current HTTP context</param>
    /// <returns>Role name or null if not authenticated</returns>
    public string? GetCurrentUserRole(HttpContext httpContext)
    {
        var session = GetCurrentSession(httpContext);
        return session?.RoleName;
    }
}
