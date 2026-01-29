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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
