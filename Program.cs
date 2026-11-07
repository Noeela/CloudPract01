using Azure.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Practicum.Data;
using Practicum.Models;

try
{
    var builder = WebApplication.CreateBuilder(args);
    var cfg = builder.Configuration;

    builder.Services.AddControllersWithViews();
    if (!string.IsNullOrWhiteSpace(cfg["ApplicationInsights:ConnectionString"]) ||
        !string.IsNullOrWhiteSpace(cfg["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    {
        builder.Services.AddApplicationInsightsTelemetry();
    }

    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlServer(cfg.GetConnectionString("DefaultConnection"),
            sql => sql.EnableRetryOnFailure()));

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied"; });

    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

    builder.Services.AddSingleton(new BlobServiceClient(cfg["Storage:ConnectionString"]));
    builder.Services.AddSingleton(new TableServiceClient(cfg["Storage:ConnectionString"]));

    builder.Services.AddHttpClient("functions", c =>
    {
        var baseUrl = cfg["Functions:BaseUrl"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            throw new InvalidOperationException($"Functions:BaseUrl is invalid. Current value: '{baseUrl}'");
        c.BaseAddress = uri;
        c.DefaultRequestHeaders.Add("x-functions-key", cfg["Functions:Key"]);
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        if (!db.Users.Any(u => u.Role == "Admin"))
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            var admin = new User { Username = "admin", Role = "Admin" };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
            db.Users.Add(admin);
            db.SaveChanges();
        }
    }

    if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
    app.Run();
}
catch (Exception ex)
{
    // TEMPORARY: shows the startup error in the browser. Remove after diagnosing.
    var errApp = WebApplication.CreateBuilder(args).Build();
    errApp.MapGet("/{**path}", () => Results.Text(ex.ToString(), "text/plain"));
    errApp.Run();
}