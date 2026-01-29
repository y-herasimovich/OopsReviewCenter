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
        // Get session data
        var userId = authService.GetCurrentUserId(context);
        if (userId.HasValue)
        {
            var username = authService.GetCurrentUserName(context);
            var fullName = authService.GetCurrentUserFullName(context);
            var role = authService.GetCurrentUserRole(context);

            // Create claims and identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Name, username ?? userId.Value.ToString()),
                new Claim(ClaimTypes.Role, role ?? string.Empty),
                new Claim("FullName", fullName ?? username ?? "User")
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            var principal = new ClaimsPrincipal(identity);
            
            context.User = principal;
        }

        await _next(context);
    }
}
