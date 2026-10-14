using FinanceManager.Infrastructure.Persistence;
using FinanceManager.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceManager.Infrastructure;

public static class DependencyInjection
{
    /// <summary>محاسبه مسیرهای داده برنامه از تنظیمات</summary>
    public static AppPaths ResolveAppPaths(IConfiguration configuration, string contentRootPath)
    {
        var configuredDataDir = configuration["App:DataDirectory"];
        var dataDirectory = Path.GetFullPath(Path.Combine(
            contentRootPath,
            string.IsNullOrWhiteSpace(configuredDataDir) ? "App_Data" : configuredDataDir));

        return new AppPaths(
            dataDirectory,
            Path.Combine(dataDirectory, "finance.db"),
            Path.Combine(dataDirectory, "backups"),
            Path.Combine(dataDirectory, "keys"));
    }

    /// <summary>ثبت لایه زیرساخت: پایگاه داده SQLite، مسیرهای داده و سرویسهای برنامه</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        // مسیرها و رشته اتصال در زمان استفاده از تنظیمات خوانده میشوند
        // تا تغییرات پیکربندی (مثلاً در تستهای یکپارچهسازی) بهدرستی اعمال شود.
        services.AddSingleton(_ => ResolveAppPaths(configuration, contentRootPath));

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var paths = sp.GetRequiredService<AppPaths>();
            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = new SqliteConnectionStringBuilder { DataSource = paths.DatabasePath }.ToString();
            }

            options.UseSqlite(connectionString);
        });

        services.AddScoped<AccountService>();
        services.AddScoped<CardService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<TransferService>();
        services.AddScoped<ReportService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<AuthService>();
        services.AddScoped<BackupService>();

        return services;
    }
}
