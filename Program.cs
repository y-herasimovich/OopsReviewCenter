using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Components;
using OopsReviewCenter.Data;
using OopsReviewCenter.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add database context
var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
Directory.CreateDirectory(appDataPath);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? $"Data Source={Path.Combine(appDataPath, "oopsreviewcenter.db")}"));

// Add services
builder.Services.AddScoped<MarkdownExportService>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<OopsReviewCenterAA>();
builder.Services.AddHttpContextAccessor();

// Configure cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Add authorization with role-based policies
builder.Services.AddAuthorization(options =>
{
    // Administrator OR Incident Manager can access admin functions
    options.AddPolicy("AdminFullAccess", policy =>
        policy.RequireRole("Administrator", "Incident Manager"));
    
    // Administrator OR Incident Manager OR Developer can edit operations data
    options.AddPolicy("CanEditOpsData", policy =>
        policy.RequireRole("Administrator", "Incident Manager", "Developer"));
    
    // Administrator OR Incident Manager OR Developer OR Viewer can view operations data
    options.AddPolicy("CanViewOpsData", policy =>
        policy.RequireRole("Administrator", "Incident Manager", "Developer", "Viewer"));
});

builder.Services.AddAntiforgery(options =>
{
    // Custom antiforgery configuration
});

// Enable cascade authentication state
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Add authentication middleware
app.UseAuthentication();

// Add authorization middleware
app.UseAuthorization();

// Skip antiforgery for /auth/* endpoints
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/auth"))
    {
        // Enable antiforgery for non-auth routes
        var antiforgeryFeature = context.Features.Get<Microsoft.AspNetCore.Antiforgery.IAntiforgeryValidationFeature>();
        if (antiforgeryFeature == null)
        {
            await next();
        }
        else
        {
            await next();
        }
    }
    else
    {
        // Skip antiforgery validation for /auth routes
        await next();
    }
});

app.UseAntiforgery();

// Authentication endpoints
app.MapPost("/auth/login", async (HttpContext context, OopsReviewCenterAA authService) =>
{
    var form = await context.Request.ReadFormAsync();
    var login = form["login"].ToString();
    var password = form["password"].ToString();

    // Authenticate using the pure AA service
    var authResult = await authService.AuthenticateAsync(login, password);

    if (!authResult.Success)
    {
        // Redirect back to login with error message
        context.Response.Redirect($"/login?error={Uri.EscapeDataString(authResult.ErrorMessage ?? "Login failed")}");
        return;
    }

    // Create claims with the exact role name from the database
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, authResult.UserId.ToString()),
        new Claim(ClaimTypes.Role, authResult.RoleName ?? ""),
        new Claim(ClaimTypes.Name, authResult.Username ?? ""),
    };

    if (!string.IsNullOrEmpty(authResult.FullName))
    {
        claims.Add(new Claim(ClaimTypes.GivenName, authResult.FullName));
    }

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

    // Sign in with cookie authentication
    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        claimsPrincipal,
        new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        });

    // Redirect to home page
    context.Response.Redirect("/");
});

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    // Sign out
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
    // Redirect to login page
    context.Response.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
