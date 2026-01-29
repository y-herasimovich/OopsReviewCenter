using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
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

// Add authentication & authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
