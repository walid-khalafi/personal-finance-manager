using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Core.Text;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

public class TransactionService(AppDbContext db)
{
    public async Task<PagedResult<TransactionListItem>> SearchAsync(TransactionFilter filter, CancellationToken ct = default)
    {
        var query = db.Transactions.AsNoTracking();

        if (filter.AccountId is int accountId)
        {
            query = query.Where(t => t.AccountId == accountId);
        }

        if (filter.Type is TransactionType type)
        {
            query = query.Where(t => t.Type == type);
        }

        if (filter.CategoryId is int categoryId)
        {
            query = query.Where(t => t.CategoryId == categoryId);
        }

        if (filter.From is DateOnly from)
        {
            query = query.Where(t => t.TransactionDate >= from);
        }

        if (filter.To is DateOnly to)
        {
            query = query.Where(t => t.TransactionDate <= to);
        }

        if (!filter.IncludeTransfers)
        {
            query = query.Where(t => t.TransferGroupId == null);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(t =>
                t.Description.Contains(term)
                || (t.ReferenceNumber != null && t.ReferenceNumber.Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);

        var ordered = filter.Sort switch
        {
            TransactionSort.DateAsc => query.OrderBy(t => t.TransactionDate).ThenBy(t => t.Id),
            TransactionSort.AmountDesc => query.OrderByDescending(t => t.AmountMinor).ThenByDescending(t => t.Id),
            TransactionSort.AmountAsc => query.OrderBy(t => t.AmountMinor).ThenBy(t => t.Id),
            _ => query.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id),
        };

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 5, 100);

        var items = await TransactionQueries.Project(ordered)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<TransactionListItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<List<TransactionListItem>> GetRecentAsync(int count = 10, CancellationToken ct = default)
    {
        var query = db.Transactions.AsNoTracking()
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id);

        return await TransactionQueries.Project(query)
            .Take(Math.Clamp(count, 1, 50))
            .ToListAsync(ct);
    }

    public Task<Transaction?> GetAsync(int id, CancellationToken ct = default)
        => db.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<TransactionListItem?> GetListItemAsync(int id, CancellationToken ct = default)
        => TransactionQueries.Project(db.Transactions.AsNoTracking().Where(t => t.Id == id))
            .FirstOrDefaultAsync(ct);

    public async Task<ServiceResult<int>> CreateAsync(TransactionInput input, CancellationToken ct = default)
    {
        var account = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == input.AccountId, ct);
        if (account is null)
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.NotFound, "حساب انتخابشده یافت نشد.");
        }

        if (!account.IsActive)
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.AccountInactive, "حساب انتخابشده غیرفعال است.");
        }

        var error = await ValidateAsync(input, current: null, ct);
        if (error is not null)
        {
            return ServiceResult<int>.Fail(error.Value.Code, error.Value.Message);
        }

        var transaction = new Transaction
        {
            AccountId = input.AccountId,
            Type = input.Type,
            AmountMinor = input.AmountMinor,
            TransactionDate = input.TransactionDate,
            CategoryId = input.CategoryId,
            Description = input.Description.Trim(),
            ReferenceNumber = NormalizeReference(input.ReferenceNumber),
            CardId = input.CardId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(ct);
        return ServiceResult<int>.Success(transaction.Id);
    }

    public async Task<ServiceResult> UpdateAsync(int id, TransactionInput input, CancellationToken ct = default)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (transaction is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "تراکنش یافت نشد.");
        }

        if (transaction.TransferGroupId is not null)
        {
            return ServiceResult.Fail(
                ServiceErrorCode.TransferEntryEdit,
                "این تراکنش بخشی از یک انتقال است؛ برای ویرایش از بخش «انتقالها» استفاده کنید.");
        }

        var targetAccount = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == input.AccountId, ct);
        if (targetAccount is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "حساب انتخابشده یافت نشد.");
        }

        if (input.AccountId != transaction.AccountId && !targetAccount.IsActive)
        {
            return ServiceResult.Fail(ServiceErrorCode.AccountInactive, "حساب مقصد غیرفعال است.");
        }

        var error = await ValidateAsync(input, current: transaction, ct);
        if (error is not null)
        {
            return ServiceResult.Fail(error.Value.Code, error.Value.Message);
        }

        transaction.AccountId = input.AccountId;
        transaction.Type = input.Type;
        transaction.AmountMinor = input.AmountMinor;
        transaction.TransactionDate = input.TransactionDate;
        transaction.CategoryId = input.CategoryId;
        transaction.Description = input.Description.Trim();
        transaction.ReferenceNumber = NormalizeReference(input.ReferenceNumber);
        transaction.CardId = input.CardId;

        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (transaction is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "تراکنش یافت نشد.");
        }

        if (transaction.TransferGroupId is not null)
        {
            return ServiceResult.Fail(
                ServiceErrorCode.TransferEntryEdit,
                "این تراکنش بخشی از یک انتقال است؛ برای حذف از بخش «انتقالها» استفاده کنید.");
        }

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    /// <summary>اعتبارسنجی مشترک قواعد مالی تراکنش</summary>
    private async Task<(ServiceErrorCode Code, string Message)?> ValidateAsync(
        TransactionInput input,
        Transaction? current,
        CancellationToken ct)
    {
        if (input.AmountMinor <= 0)
        {
            return (ServiceErrorCode.AmountInvalid, "مبلغ باید بزرگتر از صفر باشد.");
        }

        if (input.TransactionDate == default)
        {
            return (ServiceErrorCode.Validation, "تاریخ تراکنش نامعتبر است.");
        }

        if (string.IsNullOrWhiteSpace(input.Description))
        {
            return (ServiceErrorCode.Validation, "شرح تراکنش الزامی است.");
        }

        if (input.Description.Trim().Length > 400)
        {
            return (ServiceErrorCode.Validation, "شرح تراکنش حداکثر ۴۰۰ نویسه است.");
        }

        if (NormalizeReference(input.ReferenceNumber) is { Length: > 120 })
        {
            return (ServiceErrorCode.Validation, "شماره پیگیری حداکثر ۱۲۰ نویسه است.");
        }

        if (input.CardId is int cardId)
        {
            var card = await db.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cardId, ct);
            if (card is null)
            {
                return (ServiceErrorCode.NotFound, "کارت انتخابشده یافت نشد.");
            }

            if (card.AccountId != input.AccountId)
            {
                return (ServiceErrorCode.CardAccountMismatch, "کارت انتخابشده متعلق به این حساب نیست.");
            }

            var cardChanged = current is null || current.CardId != cardId;
            if (cardChanged && !card.IsActive)
            {
                return (ServiceErrorCode.Validation, "کارت انتخابشده غیرفعال است.");
            }
        }

        if (input.CategoryId is int categoryId)
        {
            var category = await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == categoryId, ct);
            if (category is null)
            {
                return (ServiceErrorCode.NotFound, "دستهبندی انتخابشده یافت نشد.");
            }

            if (!IsKindCompatible(category.Kind, input.Type))
            {
                return (ServiceErrorCode.Validation, "دستهبندی انتخابشده با نوع تراکنش سازگار نیست.");
            }

            var categoryChanged = current is null || current.CategoryId != categoryId;
            if (categoryChanged && !category.IsActive)
            {
                return (ServiceErrorCode.Validation, "دستهبندی انتخابشده غیرفعال است.");
            }
        }

        return null;
    }

    internal static bool IsKindCompatible(CategoryKind kind, TransactionType type)
        => kind switch
        {
            CategoryKind.Both => true,
            CategoryKind.Deposit => type == TransactionType.Deposit,
            CategoryKind.Withdrawal => type == TransactionType.Withdrawal,
            _ => false,
        };

    internal static string? NormalizeReference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return PersianText.NormalizeDigits(value).Trim();
    }
}
