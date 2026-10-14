using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

/// <summary>پروجکشن مشترک تراکنشها (یک پرسوجو با Join؛ بدون N+1)</summary>
internal static class TransactionQueries
{
    public static IQueryable<TransactionListItem> Project(IQueryable<Transaction> source)
        => source.Select(t => new TransactionListItem
        {
            Id = t.Id,
            AccountId = t.AccountId,
            AccountTitle = t.Account.Title,
            BankName = t.Account.BankName,
            CurrencyCode = t.Account.CurrencyCode,
            Type = t.Type,
            AmountMinor = t.AmountMinor,
            TransactionDate = t.TransactionDate,
            CategoryId = t.CategoryId,
            CategoryName = t.Category != null ? t.Category.Name : null,
            Description = t.Description,
            ReferenceNumber = t.ReferenceNumber,
            CardId = t.CardId,
            CardNickname = t.Card != null ? t.Card.Nickname : null,
            TransferGroupId = t.TransferGroupId,
            CreatedAtUtc = t.CreatedAtUtc,
        });
}

/// <summary>جمعهای دفتر کل به تفکیک حساب</summary>
internal static class BalanceQueries
{
    public static async Task<Dictionary<int, (long Deposits, long Withdrawals)>> GetAccountSumsAsync(
        AppDbContext db,
        CancellationToken ct = default)
    {
        var sums = await db.Transactions.AsNoTracking()
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Deposits = g.Sum(t => t.Type == TransactionType.Deposit ? t.AmountMinor : 0L),
                Withdrawals = g.Sum(t => t.Type == TransactionType.Withdrawal ? t.AmountMinor : 0L),
            })
            .ToListAsync(ct);

        return sums.ToDictionary(x => x.AccountId, x => (x.Deposits, x.Withdrawals));
    }
}
