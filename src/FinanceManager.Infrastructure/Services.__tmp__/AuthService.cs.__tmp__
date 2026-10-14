using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

/// <summary>
/// احراز هویت محلی (نصب تکنفره) با هشگذاری استاندارد ASP.NET Core.
/// هیچ گذرواژه یا داده حساسی لاگ نمیشود.
/// </summary>
public class AuthService(AppDbContext db)
{
    private static readonly PasswordHasher<AppUser> Hasher = new();

    private static readonly AppUser DummyUser = new() { Username = "—", PasswordHash = "" };
    private static readonly string DummyHash = Hasher.HashPassword(DummyUser, "timing-placeholder-only");

    public Task<bool> HasAnyUserAsync(CancellationToken ct = default)
        => db.Users.AsNoTracking().AnyAsync(ct);

    public async Task<ServiceResult<int>> CreateInitialUserAsync(string username, string password, CancellationToken ct = default)
    {
        username = username.Trim();

        if (username.Length < 3 || username.Length > 64)
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.Validation, "نام کاربری باید بین ۳ تا ۶۴ نویسه باشد.");
        }

        if (password.Length < 8)
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.Validation, "گذرواژه باید حداقل ۸ نویسه باشد.");
        }

        if (password.Length > 200)
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.Validation, "گذرواژه بسیار طولانی است.");
        }

        if (await db.Users.AnyAsync(ct))
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.Conflict, "کاربر قبلاً ایجاد شده است.");
        }

        var user = new AppUser
        {
            Username = username,
            PasswordHash = "",
            CreatedAtUtc = DateTime.UtcNow,
        };
        user.PasswordHash = Hasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return ServiceResult<int>.Success(user.Id);
    }

    public async Task<AppUser?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username.Trim(), ct);
        if (user is null)
        {
            // هش ساختگی برای همسانسازی زمان پاسخ (کاهش افشای وجود کاربر)
            Hasher.VerifyHashedPassword(DummyUser, DummyHash, password);
            return null;
        }

        var result = Hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = Hasher.HashPassword(user, password);
        }

        user.LastLoginUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return user;
    }

    public async Task<ServiceResult> ChangePasswordAsync(
        string username,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
        if (user is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "کاربر یافت نشد.");
        }

        if (Hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
        {
            return ServiceResult.Fail(ServiceErrorCode.AuthFailed, "گذرواژه فعلی نادرست است.");
        }

        if (newPassword.Length < 8)
        {
            return ServiceResult.Fail(ServiceErrorCode.Validation, "گذرواژه جدید باید حداقل ۸ نویسه باشد.");
        }

        if (newPassword.Length > 200)
        {
            return ServiceResult.Fail(ServiceErrorCode.Validation, "گذرواژه جدید بسیار طولانی است.");
        }

        user.PasswordHash = Hasher.HashPassword(user, newPassword);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}
