using FinanceManager.Core.Text;
using FinanceManager.Core.Time;

namespace FinanceManager.Web.Helpers;

/// <summary>وضعیت انتخاب دوره گزارش در فرمها</summary>
public sealed class PeriodSelection
{
    public PeriodKind Kind { get; init; }

    public DateRange Range { get; init; }

    /// <summary>متن تاریخ شروع (شمسی) برای پیشپر کردن فرم بازه دلخواه</summary>
    public string FromText { get; init; } = "";

    /// <summary>متن تاریخ پایان (شمسی) برای پیشپر کردن فرم بازه دلخواه</summary>
    public string ToText { get; init; } = "";

    public string? Error { get; init; }

    public static PeriodSelection Resolve(int period, string? from, string? to)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var kind = Enum.IsDefined(typeof(PeriodKind), period) ? (PeriodKind)period : PeriodKind.ThisMonth;

        if (kind != PeriodKind.Custom)
        {
            var range = PeriodResolver.Resolve(kind, today);
            return new PeriodSelection
            {
                Kind = kind,
                Range = range,
                FromText = PersianText.FormatDate(range.From),
                ToText = PersianText.FormatDate(range.To),
            };
        }

        var hasFrom = PersianText.TryParseDate(from, out var fromDate);
        var hasTo = PersianText.TryParseDate(to, out var toDate);
        var invalidFrom = !string.IsNullOrWhiteSpace(from) && !hasFrom;
        var invalidTo = !string.IsNullOrWhiteSpace(to) && !hasTo;

        if (invalidFrom || invalidTo)
        {
            var fallback = PeriodResolver.Resolve(PeriodKind.Last30Days, today);
            return new PeriodSelection
            {
                Kind = PeriodKind.Custom,
                Range = fallback,
                FromText = string.IsNullOrWhiteSpace(from) ? PersianText.FormatDate(fallback.From) : from,
                ToText = string.IsNullOrWhiteSpace(to) ? PersianText.FormatDate(fallback.To) : to,
                Error = "قالب تاریخ معتبر نیست؛ نمونه صحیح: ۱۴۰۳/۰۷/۲۲",
            };
        }

        var customRange = PeriodResolver.Resolve(
            PeriodKind.Custom,
            today,
            hasFrom ? fromDate : null,
            hasTo ? toDate : null);

        return new PeriodSelection
        {
            Kind = PeriodKind.Custom,
            Range = customRange,
            FromText = PersianText.FormatDate(customRange.From),
            ToText = PersianText.FormatDate(customRange.To),
        };
    }
}
