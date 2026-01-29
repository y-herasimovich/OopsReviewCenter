using System.Security.Claims;

namespace OopsReviewCenter.Services;

/// <summary>
/// Custom authentication middleware that populates HttpContext.User based on session.
/// </summary>
public class CustomAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public CustomAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, OopsReviewCenterAA authService)
    {
        // Get session data efficiently in one call
        var (userId, username, fullName, role) = authService.GetSessionData(context);
        
        if (userId.HasValue)
        {
            // Create claims and identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Name, username ?? userId.Value.ToString()),
                new Claim("FullName", fullName ?? username ?? "User")
            };

            // Only add role claim if role is not null or empty
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            var principal = new ClaimsPrincipal(identity);
            
            context.User = principal;
        }

        await _next(context);
    }
}
