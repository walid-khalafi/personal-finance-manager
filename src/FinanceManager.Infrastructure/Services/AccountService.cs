using FinanceManager.Core.Entities;
using FinanceManager.Core.Money;
using FinanceManager.Core.Text;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

public class AccountService(AppDbContext db)
{
    public async Task<List<AccountListItem>> GetAccountsAsync(bool includeInactive = true, CancellationToken ct = default)
    {
        var query = db.Accounts.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(a => a.IsActive);
        }

        var accounts = await query
            .OrderByDescending(a => a.IsActive)
            .ThenBy(a => a.BankName)
            .ThenBy(a => a.Title)
            .Select(a => new
            {
                a.Id,
                a.BankName,
                a.Title,
                a.Type,
                a.CurrencyCode,
                a.AccountNumber,
                a.Iban,
                a.OpeningBalanceMinor,
                a.IsActive,
                a.Notes,
                a.CreatedAtUtc,
            })
            .ToListAsync(ct);

        var sums = await BalanceQueries.GetAccountSumsAsync(db, ct);

        return accounts.Select(a =>
        {
            sums.TryGetValue(a.Id, out var s);
            return new AccountListItem
            {
                Id = a.Id,
                BankName = a.BankName,
                Title = a.Title,
                Type = a.Type,
                CurrencyCode = a.CurrencyCode,
                AccountNumber = a.AccountNumber,
                Iban = a.Iban,
                OpeningBalanceMinor = a.OpeningBalanceMinor,
                IsActive = a.IsActive,
                Notes = a.Notes,
                CreatedAtUtc = a.CreatedAtUtc,
                DepositsMinor = s.Deposits,
                WithdrawalsMinor = s.Withdrawals,
            };
        }).ToList();
    }

    public Task<Account?> GetAsync(int id, CancellationToken ct = default)
        => db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    /// <summary>همه حسابها برای فهرستهای انتخابی (شامل غیرفعالها، با علامتگذاری)</summary>
    public Task<List<Account>> GetAllAsync(CancellationToken ct = default)
        => db.Accounts.AsNoTracking()
            .OrderByDescending(a => a.IsActive)
            .ThenBy(a => a.BankName)
            .ThenBy(a => a.Title)
            .ToListAsync(ct);

    public async Task<ServiceResult<int>> CreateAsync(AccountInput input, CancellationToken ct = default)
    {
        var error = ValidateInput(input);
        if (error is not null)
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.Validation, error);
        }

        var account = new Account
        {
            BankName = input.BankName.Trim(),
            Title = input.Title.Trim(),
            Type = input.Type,
            AccountNumber = NormalizeSensitiveNumber(input.AccountNumber),
            Iban = NormalizeSensitiveNumber(input.Iban),
            CurrencyCode = input.CurrencyCode.Trim().ToUpperInvariant(),
            OpeningBalanceMinor = input.OpeningBalanceMinor,
            IsActive = input.IsActive,
            Notes = NormalizeNotes(input.Notes),
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
        return ServiceResult<int>.Success(account.Id);
    }

    public async Task<ServiceResult> UpdateAsync(int id, AccountInput input, CancellationToken ct = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (account is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "حساب یافت نشد.");
        }

        var error = ValidateInput(input);
        if (error is not null)
        {
            return ServiceResult.Fail(ServiceErrorCode.Validation, error);
        }

        var newCurrency = input.CurrencyCode.Trim().ToUpperInvariant();
        if (!string.Equals(account.CurrencyCode, newCurrency, StringComparison.OrdinalIgnoreCase)
            && await db.Transactions.AnyAsync(t => t.AccountId == id, ct))
        {
            return ServiceResult.Fail(
                ServiceErrorCode.Validation,
                "این حساب دارای تراکنش است؛ تغییر ارز پس از ثبت تراکنش مجاز نیست.");
        }

        account.BankName = input.BankName.Trim();
        account.Title = input.Title.Trim();
        account.Type = input.Type;
        account.AccountNumber = NormalizeSensitiveNumber(input.AccountNumber);
        account.Iban = NormalizeSensitiveNumber(input.Iban);
        account.CurrencyCode = newCurrency;
        account.OpeningBalanceMinor = input.OpeningBalanceMinor;
        account.IsActive = input.IsActive;
        account.Notes = NormalizeNotes(input.Notes);

        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (account is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "حساب یافت نشد.");
        }

        account.IsActive = active;
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (account is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "حساب یافت نشد.");
        }

        if (await db.Transactions.AnyAsync(t => t.AccountId == id, ct))
        {
            return ServiceResult.Fail(
                ServiceErrorCode.InUse,
                "این حساب دارای تراکنش است و حذف نمیشود؛ برای حفظ سابقه، حساب را غیرفعال کنید.");
        }

        if (await db.Cards.AnyAsync(c => c.AccountId == id, ct))
        {
            return ServiceResult.Fail(ServiceErrorCode.InUse, "ابتدا کارتهای این حساب را حذف یا منتقل کنید.");
        }

        db.Accounts.Remove(account);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    private static string? ValidateInput(AccountInput input)
    {
        if (string.IsNullOrWhiteSpace(input.BankName))
        {
            return "نام بانک الزامی است.";
        }

        if (input.BankName.Trim().Length > 120)
        {
            return "نام بانک حداکثر ۱۲۰ نویسه است.";
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return "عنوان حساب الزامی است.";
        }

        if (input.Title.Trim().Length > 120)
        {
            return "عنوان حساب حداکثر ۱۲۰ نویسه است.";
        }

        if (!CurrencyCatalog.Contains(input.CurrencyCode))
        {
            return "ارز انتخابشده پشتیبانی نمیشود.";
        }

        if (input.Notes is { Length: > 1000 })
        {
            return "یادداشت حداکثر ۱۰۰۰ نویسه است.";
        }

        if (NormalizeSensitiveNumber(input.AccountNumber) is { Length: > 64 })
        {
            return "شماره حساب حداکثر ۶۴ نویسه است.";
        }

        if (NormalizeSensitiveNumber(input.Iban) is { Length: > 64 })
        {
            return "شماره شبا حداکثر ۶۴ نویسه است.";
        }

        return null;
    }

    internal static string? NormalizeSensitiveNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return PersianText.NormalizeDigits(value).Replace(" ", "").Trim().ToUpperInvariant();
    }

    private static string? NormalizeNotes(string? notes)
        => string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
