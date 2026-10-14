using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

public sealed record BackupInfo(string Name, long SizeBytes, DateTime CreatedUtc);

/// <summary>
/// پشتیبانگیری آنلاین و سازگار از پایگاه SQLite با API رسمی Backup
/// (کپی ساده فایل در حین نوشتن پایگاه داده مجاز نیست).
/// </summary>
public class BackupService(AppDbContext db, AppPaths paths)
{
    public List<BackupInfo> ListBackups()
    {
        if (!Directory.Exists(paths.BackupsDirectory))
        {
            return [];
        }

        return new DirectoryInfo(paths.BackupsDirectory)
            .GetFiles("*.db")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Select(f => new BackupInfo(f.Name, f.Length, f.LastWriteTimeUtc))
            .ToList();
    }

    public async Task<ServiceResult<string>> CreateBackupAsync(CancellationToken ct = default)
    {
        try
        {
            Directory.CreateDirectory(paths.BackupsDirectory);
            var name = $"finance-{DateTime.Now:yyyyMMdd-HHmmss}.db";
            var target = Path.Combine(paths.BackupsDirectory, name);

            var sourceConnectionString = db.Database.GetConnectionString()
                ?? new SqliteConnectionStringBuilder { DataSource = paths.DatabasePath }.ToString();

            await using var source = new SqliteConnection(sourceConnectionString);
            await source.OpenAsync(ct);

            await using var destination = new SqliteConnection(
                new SqliteConnectionStringBuilder
                {
                    DataSource = target,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                }.ToString());
            await destination.OpenAsync(ct);

            source.BackupDatabase(destination);

            return ServiceResult<string>.Success(target);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Fail(ServiceErrorCode.IoError, $"ایجاد نسخه پشتیبان ناموفق بود: {ex.Message}");
        }
    }

    /// <summary>مسیر امن یک فایل پشتیبان (جلوگیری از پیمایش مسیر)</summary>
    public string? ResolveBackupPath(string fileName)
    {
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName) || !safeName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var root = Path.GetFullPath(paths.BackupsDirectory) + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(Path.Combine(paths.BackupsDirectory, safeName));
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return File.Exists(fullPath) ? fullPath : null;
    }
}
