using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

/// <summary>
/// انتقال بین دو حساب با ارز یکسان. هر انتقال دو قلم دفتر کل مرتبط
/// (یک برداشت از مبدأ و یک واریز به مقصد) است که در یک تراکنش پایگاه داده ثبت میشوند.
/// </summary>
public class TransferService(AppDbContext db)
{
    public async Task<PagedResult<TransferListItem>> SearchAsync(TransferFilter filter, CancellationToken ct = default)
    {
        var query = db.Transactions.AsNoTracking().Where(t => t.TransferGroupId != null);

        if (filter.AccountId is int accountId)
        {
            query = query.Where(t => t.AccountId == accountId);
        }

        if (filter.From is DateOnly from)
        {
            query = query.Where(t => t.TransactionDate >= from);
        }

        if (filter.To is DateOnly to)
        {
            query = query.Where(t => t.TransactionDate <= to);
        }

        var groupsQuery = query
            .GroupBy(t => t.TransferGroupId!.Value)
            .Select(g => new { GroupId = g.Key, Date = g.Min(t => t.TransactionDate) });

        var totalCount = await groupsQuery.CountAsync(ct);

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 5, 100);

        var pageGroups = await groupsQuery
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.GroupId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var ids = pageGroups.Select(g => g.GroupId).ToList();
        List<TransferListItem> items = ids.Count == 0 ? [] : await LoadItemsAsync(ids, ct);

        var ordered = ids
            .Select(id => items.FirstOrDefault(i => i.GroupId == id))
            .Where(i => i is not null)
            .Cast<TransferListItem>()
            .ToList();

        return new PagedResult<TransferListItem>
        {
            Items = ordered,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<TransferDetail?> GetAsync(Guid groupId, CancellationToken ct = default)
    {
        var legs = await db.Transactions.AsNoTracking()
            .Where(t => t.TransferGroupId == groupId)
            .Select(t => new TransferLeg
            {
                TransactionId = t.Id,
                AccountId = t.AccountId,
                AccountTitle = t.Account.Title,
                Type = t.Type,
                AmountMinor = t.AmountMinor,
                TransactionDate = t.TransactionDate,
                Description = t.Description,
                CurrencyCode = t.Account.CurrencyCode,
            })
            .ToListAsync(ct);

        var from = legs.FirstOrDefault(l => l.Type == TransactionType.Withdrawal);
        var to = legs.FirstOrDefault(l => l.Type == TransactionType.Deposit);
        if (from is null || to is null)
        {
            return null;
        }

        return new TransferDetail { GroupId = groupId, From = from, To = to };
    }

    public async Task<ServiceResult<Guid>> CreateAsync(TransferInput input, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(input, exemptFromAccountId: null, exemptToAccountId: null, ct);
        if (validation.Error is { } error)
        {
            return ServiceResult<Guid>.Fail(error.Code, error.Message);
        }

        var groupId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var fromEntry = new Transaction
        {
            AccountId = validation.From!.Id,
            Type = TransactionType.Withdrawal,
            AmountMinor = input.AmountMinor,
            TransactionDate = input.TransactionDate,
            Description = Describe(input.Description, $"انتقال به {validation.To!.Title}"),
            CreatedAtUtc = now,
            TransferGroupId = groupId,
        };

        var toEntry = new Transaction
        {
            AccountId = validation.To!.Id,
            Type = TransactionType.Deposit,
            AmountMinor = input.AmountMinor,
            TransactionDate = input.TransactionDate,
            Description = Describe(input.Description, $"انتقال از {validation.From!.Title}"),
            CreatedAtUtc = now,
            TransferGroupId = groupId,
        };

        await using var trx = await db.Database.BeginTransactionAsync(ct);
        db.Transactions.AddRange(fromEntry, toEntry);
        await db.SaveChangesAsync(ct);
        await trx.CommitAsync(ct);

        return ServiceResult<Guid>.Success(groupId);
    }

    public async Task<ServiceResult> UpdateAsync(Guid groupId, TransferInput input, CancellationToken ct = default)
    {
        var legs = await db.Transactions.Where(t => t.TransferGroupId == groupId).ToListAsync(ct);
        var fromLeg = legs.FirstOrDefault(l => l.Type == TransactionType.Withdrawal);
        var toLeg = legs.FirstOrDefault(l => l.Type == TransactionType.Deposit);
        if (fromLeg is null || toLeg is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "انتقال یافت نشد.");
        }

        var validation = await ValidateAsync(input, fromLeg.AccountId, toLeg.AccountId, ct);
        if (validation.Error is { } error)
        {
            return ServiceResult.Fail(error.Code, error.Message);
        }

        await using var trx = await db.Database.BeginTransactionAsync(ct);

        fromLeg.AccountId = validation.From!.Id;
        fromLeg.AmountMinor = input.AmountMinor;
        fromLeg.TransactionDate = input.TransactionDate;
        fromLeg.Description = Describe(input.Description, $"انتقال به {validation.To!.Title}");

        toLeg.AccountId = validation.To!.Id;
        toLeg.AmountMinor = input.AmountMinor;
        toLeg.TransactionDate = input.TransactionDate;
        toLeg.Description = Describe(input.Description, $"انتقال از {validation.From!.Title}");

        await db.SaveChangesAsync(ct);
        await trx.CommitAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid groupId, CancellationToken ct = default)
    {
        var legs = await db.Transactions.Where(t => t.TransferGroupId == groupId).ToListAsync(ct);
        if (legs.Count == 0)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "انتقال یافت نشد.");
        }

        await using var trx = await db.Database.BeginTransactionAsync(ct);
        db.Transactions.RemoveRange(legs);
        await db.SaveChangesAsync(ct);
        await trx.CommitAsync(ct);

        return ServiceResult.Success();
    }

    private async Task<((ServiceErrorCode Code, string Message)? Error, Account? From, Account? To)> ValidateAsync(
        TransferInput input,
        int? exemptFromAccountId,
        int? exemptToAccountId,
        CancellationToken ct)
    {
        if (input.AmountMinor <= 0)
        {
            return ((ServiceErrorCode.AmountInvalid, "مبلغ انتقال باید بزرگتر از صفر باشد."), null, null);
        }

        if (input.TransactionDate == default)
        {
            return ((ServiceErrorCode.Validation, "تاریخ انتقال نامعتبر است."), null, null);
        }

        if (input.Description is { Length: > 400 })
        {
            return ((ServiceErrorCode.Validation, "شرح انتقال حداکثر ۴۰۰ نویسه است."), null, null);
        }

        if (input.FromAccountId == input.ToAccountId)
        {
            return ((ServiceErrorCode.SameAccountTransfer, "حساب مبدأ و مقصد نباید یکسان باشند."), null, null);
        }

        var from = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == input.FromAccountId, ct);
        if (from is null)
        {
            return ((ServiceErrorCode.NotFound, "حساب مبدأ یافت نشد."), null, null);
        }

        var to = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == input.ToAccountId, ct);
        if (to is null)
        {
            return ((ServiceErrorCode.NotFound, "حساب مقصد یافت نشد."), null, null);
        }

        if (!string.Equals(from.CurrencyCode, to.CurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            return ((ServiceErrorCode.CurrencyMismatch, "انتقال بین حسابهای با ارز متفاوت مجاز نیست."), null, null);
        }

        if (!from.IsActive && from.Id != exemptFromAccountId)
        {
            return ((ServiceErrorCode.AccountInactive, "حساب مبدأ غیرفعال است."), null, null);
        }

        if (!to.IsActive && to.Id != exemptToAccountId)
        {
            return ((ServiceErrorCode.AccountInactive, "حساب مقصد غیرفعال است."), null, null);
        }

        return (null, from, to);
    }

    private async Task<List<TransferListItem>> LoadItemsAsync(List<Guid> ids, CancellationToken ct)
    {
        var legs = await db.Transactions.AsNoTracking()
            .Where(t => t.TransferGroupId != null && ids.Contains(t.TransferGroupId.Value))
            .Select(t => new
            {
                GroupId = t.TransferGroupId!.Value,
                t.AccountId,
                AccountTitle = t.Account.Title,
                BankName = t.Account.BankName,
                CurrencyCode = t.Account.CurrencyCode,
                t.Type,
                t.AmountMinor,
                t.TransactionDate,
                t.Description,
            })
            .ToListAsync(ct);

        var result = new List<TransferListItem>();
        foreach (var groupId in ids)
        {
            var groupLegs = legs.Where(l => l.GroupId == groupId).ToList();
            var from = groupLegs.FirstOrDefault(l => l.Type == TransactionType.Withdrawal);
            var to = groupLegs.FirstOrDefault(l => l.Type == TransactionType.Deposit);
            if (from is null || to is null)
            {
                continue;
            }

            result.Add(new TransferListItem
            {
                GroupId = groupId,
                TransactionDate = from.TransactionDate,
                Description = from.Description,
                AmountMinor = from.AmountMinor,
                CurrencyCode = from.CurrencyCode,
                FromAccountId = from.AccountId,
                FromAccountTitle = from.AccountTitle,
                FromBankName = from.BankName,
                ToAccountId = to.AccountId,
                ToAccountTitle = to.AccountTitle,
                ToBankName = to.BankName,
            });
        }

        return result;
    }

    private static string Describe(string? custom, string fallback)
        => string.IsNullOrWhiteSpace(custom) ? fallback : custom.Trim();
}
