using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceManager.Infrastructure.Persistence;

/// <summary>
/// فقط برای ابزار طراحی (dotnet ef migrations). در زمان اجرا استفاده نمیشود؛
/// اتصال واقعی از تنظیمات برنامه خوانده میشود.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;

        return new AppDbContext(options);
    }
}
