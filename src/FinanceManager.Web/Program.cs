using System.Text.Encodings.Web;
using System.Text.Unicode;
using FinanceManager.Infrastructure;
using FinanceManager.Infrastructure.Persistence;
using FinanceManager.Web.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // محافظت خودکار همه درخواستهای تغییردهنده در برابر CSRF
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

// نمایش مستقیم نویسههای فارسی بهجای کدگذاری رشتهای (نویسههای خطرناک HTML همچنان کدگذاری میشوند)
builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

// کلیدهای محافظت از داده در پوشه داده برنامه نگهداری میشوند (مسیر در زمان استفاده خوانده میشود)
builder.Services.AddDataProtection().SetApplicationName("FinanceManager");
builder.Services.AddOptions<KeyManagementOptions>().Configure<AppPaths, ILoggerFactory>((options, appPaths, loggerFactory) =>
{
    Directory.CreateDirectory(appPaths.KeysDirectory);
    options.XmlRepository = new FileSystemXmlRepository(new DirectoryInfo(appPaths.KeysDirectory), loggerFactory);
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/login";
        options.Cookie.Name = "pfm.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    // همه نقاط پایانی بهطور پیشفرض نیازمند احراز هویتاند
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var appPaths = scope.ServiceProvider.GetRequiredService<AppPaths>();

    Directory.CreateDirectory(appPaths.DataDirectory);
    Directory.CreateDirectory(appPaths.BackupsDirectory);
    Directory.CreateDirectory(appPaths.KeysDirectory);

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    logger.LogInformation("پایگاه داده آماده است: {DatabasePath}", appPaths.DatabasePath);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<FirstRunSetupMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

/// <summary>برای تستهای یکپارچهسازی (WebApplicationFactory)</summary>
public partial class Program;
