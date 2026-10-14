using FinanceManager.Core.Entities;
using FinanceManager.Core.Text;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

public class CardService(AppDbContext db)
{
    public async Task<List<CardListItem>> GetCardsAsync(bool includeInactive = true, CancellationToken ct = default)
    {
        var query = db.Cards.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderByDescending(c => c.IsActive)
            .ThenBy(c => c.Account.Title)
            .ThenBy(c => c.Nickname)
            .Select(c => new CardListItem
            {
                Id = c.Id,
                AccountId = c.AccountId,
                AccountTitle = c.Account.Title,
                BankName = c.Account.BankName,
                CurrencyCode = c.Account.CurrencyCode,
                Nickname = c.Nickname,
                Last4 = c.Last4,
                ExpiryMonth = c.ExpiryMonth,
                ExpiryYear = c.ExpiryYear,
                IsActive = c.IsActive,
                Notes = c.Notes,
                TransactionCount = db.Transactions.Count(t => t.CardId == c.Id),
            })
            .ToListAsync(ct);
    }

    public Task<Card?> GetAsync(int id, CancellationToken ct = default)
        => db.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Card>> GetForSelectionAsync(bool includeInactive = true, CancellationToken ct = default)
    {
        var query = db.Cards.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return query.OrderBy(c => c.Nickname).ToListAsync(ct);
    }

    public async Task<ServiceResult<int>> CreateAsync(CardInput input, CancellationToken ct = default)
    {
        var account = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == input.AccountId, ct);
        var error = ValidateInput(input, account);
        if (error is not null)
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.Validation, error);
        }

        var card = new Card
        {
            AccountId = input.AccountId,
            Nickname = input.Nickname.Trim(),
            Last4 = NormalizeLast4(input.Last4),
            ExpiryMonth = input.ExpiryMonth,
            ExpiryYear = input.ExpiryYear,
            IsActive = input.IsActive,
            Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
        };

        db.Cards.Add(card);
        await db.SaveChangesAsync(ct);
        return ServiceResult<int>.Success(card.Id);
    }

    public async Task<ServiceResult> UpdateAsync(int id, CardInput input, CancellationToken ct = default)
    {
        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (card is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "کارت یافت نشد.");
        }

        var account = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == input.AccountId, ct);
        var error = ValidateInput(input, account);
        if (error is not null)
        {
            return ServiceResult.Fail(ServiceErrorCode.Validation, error);
        }

        if (card.AccountId != input.AccountId
            && await db.Transactions.AnyAsync(t => t.CardId == id, ct))
        {
            return ServiceResult.Fail(
                ServiceErrorCode.Validation,
                "این کارت دارای تراکنش است؛ انتقال آن به حساب دیگر مجاز نیست.");
        }

        card.AccountId = input.AccountId;
        card.Nickname = input.Nickname.Trim();
        card.Last4 = NormalizeLast4(input.Last4);
        card.ExpiryMonth = input.ExpiryMonth;
        card.ExpiryYear = input.ExpiryYear;
        card.IsActive = input.IsActive;
        card.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();

        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (card is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "کارت یافت نشد.");
        }

        card.IsActive = active;
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (card is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "کارت یافت نشد.");
        }

        // ارجاع کارت در تراکنشها بهصورت خودکار حذف (SET NULL) میشود تا سابقه حفظ شود.
        db.Cards.Remove(card);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    private static string? ValidateInput(CardInput input, Account? account)
    {
        if (account is null)
        {
            return "حساب انتخابشده یافت نشد.";
        }

        if (string.IsNullOrWhiteSpace(input.Nickname))
        {
            return "نام کارت الزامی است.";
        }

        if (input.Nickname.Trim().Length > 120)
        {
            return "نام کارت حداکثر ۱۲۰ نویسه است.";
        }

        if (!IsValidLast4(input.Last4))
        {
            return "چهار رقم آخر باید دقیقاً ۴ رقم باشد.";
        }

        if (input.ExpiryMonth is int month && (month < 1 || month > 12))
        {
            return "ماه انقضا باید بین ۱ تا ۱۲ باشد.";
        }

        if (input.ExpiryMonth is not null && input.ExpiryYear is null)
        {
            return "سال انقضا را نیز وارد کنید.";
        }

        if (input.ExpiryYear is not null && input.ExpiryMonth is null)
        {
            return "ماه انقضا را نیز وارد کنید.";
        }

        if (input.Notes is { Length: > 1000 })
        {
            return "یادداشت حداکثر ۱۰۰۰ نویسه است.";
        }

        return null;
    }

    private static bool IsValidLast4(string? value) => NormalizeLast4(value).Length == 4;

    /// <summary>نرمالسازی چهار رقم آخر (پذیرش ارقام فارسی)</summary>
    public static string NormalizeLast4(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var digits = PersianText.NormalizeDigits(value).Trim();
        return digits.All(char.IsAsciiDigit) ? digits : "invalid";
    }
}
