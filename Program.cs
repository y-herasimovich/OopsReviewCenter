using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Components;
using OopsReviewCenter.Data;
using OopsReviewCenter.Services;

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

// Add authentication with cookie scheme
// Note: The cookie authentication handler is configured here to enable authorization challenges
// (redirects to login/access-denied). However, the actual session management and authentication
// is performed by CustomAuthenticationMiddleware using a custom SessionId cookie. The timeout
// and expiration settings below do not affect the actual session lifetime, which is controlled
// in OopsReviewCenterAA.SignInAsync (8 hours with sliding expiration).
builder.Services.AddAuthentication("CustomAuth")
    .AddCookie("CustomAuth", options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
    });

// Add authorization with policies
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
// Note: This middleware is required for ASP.NET Core's authorization system to work properly.
// It enables the cookie authentication handler to process authorization challenges (401) and
// forbidden responses (403) by redirecting to login/access-denied pages. The actual user
// authentication happens in CustomAuthenticationMiddleware below, which reads the SessionId
// cookie and sets HttpContext.User. Both middlewares are required in this specific order:
// 1. UseAuthentication() - Enables challenge/forbid redirects via cookie handler
// 2. CustomAuthenticationMiddleware - Performs actual authentication via session
// 3. UseAuthorization() - Checks authorization and triggers challenges when needed
app.UseAuthentication();

// Add custom authentication middleware
app.UseMiddleware<CustomAuthenticationMiddleware>();

// Add authorization middleware
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
