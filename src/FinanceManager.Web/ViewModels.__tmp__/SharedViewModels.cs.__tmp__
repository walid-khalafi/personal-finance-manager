using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Web.Helpers;

namespace FinanceManager.Web.ViewModels;

/// <summary>مدل صفحهبندی برای پارشال مشترک</summary>
public sealed class PaginationModel
{
    public required int Page { get; init; }

    public required int TotalPages { get; init; }

    /// <summary>آدرس پایه (شامل سایر پارامترهای فیلتر؛ بدون پارامتر page)</summary>
    public required string BaseUrl { get; init; }
}

/// <summary>مدل انتخابگر دوره (و بهصورت اختیاری ارز) برای فرمهای GET</summary>
public sealed class PeriodSelectorModel
{
    public required PeriodSelection Selection { get; init; }

    /// <summary>ارز انتخابشده فعلی؛ اگر null باشد انتخابگر ارز نمایش داده نمیشود</summary>
    public string? CurrencyCode { get; init; }

    public List<string> Currencies { get; init; } = [];
}

/// <summary>مدل جدول تراکنشها</summary>
public sealed class TransactionTableModel
{
    public required List<TransactionListItem> Items { get; init; }

    public bool ShowActions { get; init; } = true;

    public bool ShowAccountColumn { get; init; } = true;
}
