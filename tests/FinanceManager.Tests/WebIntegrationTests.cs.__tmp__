using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.Configuration;

namespace FinanceManager.Tests;

/// <summary>کارخانه وباپ برای تستهای یکپارچهسازی؛ هر نمونه پوشه داده موقت خودش را دارد.</summary>
public sealed class FinanceAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dataDir = Path.Combine(Path.GetTempPath(), "pfm-tests-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["App:DataDirectory"] = _dataDir,
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(_dataDir))
        {
            try
            {
                Directory.Delete(_dataDir, recursive: true);
            }
            catch (IOException)
            {
                // پاکسازی اختیاری است
            }
        }
    }
}

public class WebIntegrationTests
{
    private static HttpClient CreateClient(FinanceAppFactory factory)
        => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static string ExtractToken(string html)
    {
        var match = Regex.Match(html, "__RequestVerificationToken[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, "توکن ضدجعل در صفحه پیدا نشد.");
        return match.Groups[1].Value;
    }

    private static FormUrlEncodedContent Form(Dictionary<string, string> values) => new(values);

    [Fact]
    public async Task FirstRun_RedirectsAnonymousUserToSetup()
    {
        using var factory = new FinanceAppFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/auth/setup", response.Headers.Location!.ToString());

        var setupPage = await client.GetAsync("/auth/setup");
        Assert.Equal(HttpStatusCode.OK, setupPage.StatusCode);
    }

    [Fact]
    public async Task Setup_Login_Logout_FullFlow()
    {
        using var factory = new FinanceAppFactory();
        using var client = CreateClient(factory);

        // راهاندازی اولیه: ساخت کاربر
        var setupHtml = await client.GetStringAsync("/auth/setup");
        var setupResponse = await client.PostAsync("/auth/setup", Form(new()
        {
            ["Username"] = "walid",
            ["Password"] = "correct-horse-1",
            ["ConfirmPassword"] = "correct-horse-1",
            ["__RequestVerificationToken"] = ExtractToken(setupHtml),
        }));
        Assert.Equal(HttpStatusCode.Redirect, setupResponse.StatusCode);

        // دسترسی احراز هویتشده به داشبورد
        var dashboard = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        var dashboardHtml = await dashboard.Content.ReadAsStringAsync();
        Assert.Contains("داشبورد", dashboardHtml);

        // بعد از ساخت کاربر، صفحه راهاندازی دیگر در دسترس نیست
        var setupAgain = await client.GetAsync("/auth/setup");
        Assert.Equal(HttpStatusCode.Redirect, setupAgain.StatusCode);

        // خروج
        var logout = await client.PostAsync("/auth/logout", Form(new()
        {
            ["__RequestVerificationToken"] = ExtractToken(dashboardHtml),
        }));
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        // بعد از خروج: هدایت به صفحه ورود (کاربر وجود دارد)
        var afterLogout = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
        Assert.Contains("/auth/login", afterLogout.Headers.Location!.ToString());

        // گذرواژه نادرست رد میشود
        var loginHtml = await client.GetStringAsync("/auth/login");
        var badLogin = await client.PostAsync("/auth/login", Form(new()
        {
            ["Username"] = "walid",
            ["Password"] = "wrong-password",
            ["__RequestVerificationToken"] = ExtractToken(loginHtml),
        }));
        Assert.Equal(HttpStatusCode.OK, badLogin.StatusCode);
        var badHtml = await badLogin.Content.ReadAsStringAsync();
        Assert.Contains("نادرست", badHtml);

        // ورود با گذرواژه درست
        var goodLogin = await client.PostAsync("/auth/login", Form(new()
        {
            ["Username"] = "walid",
            ["Password"] = "correct-horse-1",
            ["__RequestVerificationToken"] = ExtractToken(badHtml),
        }));
        Assert.Equal(HttpStatusCode.Redirect, goodLogin.StatusCode);

        var finalDashboard = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, finalDashboard.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutAntiforgeryToken_IsRejected()
    {
        using var factory = new FinanceAppFactory();
        using var client = CreateClient(factory);

        var response = await client.PostAsync("/auth/setup", Form(new()
        {
            ["Username"] = "walid",
            ["Password"] = "correct-horse-1",
            ["ConfirmPassword"] = "correct-horse-1",
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
