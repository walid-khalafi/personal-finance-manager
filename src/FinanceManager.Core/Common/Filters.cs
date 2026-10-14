using FinanceManager.Core.Entities;

namespace FinanceManager.Core.Common;

public enum TransactionSort
{
    DateDesc = 0,
    DateAsc = 1,
    AmountDesc = 2,
    AmountAsc = 3,
}

/// <summary>فیلتر جستوجوی تراکنشها (فیلتر و صفحهبندی سمت پایگاه داده)</summary>
public sealed class TransactionFilter
{
    public int? AccountId { get; set; }

    public TransactionType? Type { get; set; }

    public int? CategoryId { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    /// <summary>جستوجو در شرح و شماره پیگیری</summary>
    public string? Search { get; set; }

    /// <summary>خطوط انتقالی را شامل شود؟ (پیشفرض: بله)</summary>
    public bool IncludeTransfers { get; set; } = true;

    public TransactionSort Sort { get; set; } = TransactionSort.DateDesc;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

/// <summary>فیلتر فهرست انتقالها</summary>
public sealed class TransferFilter
{
    public int? AccountId { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
