using System.Globalization;
using FinanceManager.Core.Text;

namespace FinanceManager.Core.Time;

/// <summary>دورههای گزارشی متداول</summary>
public enum PeriodKind
{
    ThisMonth = 0,  // این ماه
    LastMonth = 1,  // ماه گذشته
    ThisYear = 2,   // امسال
    Last30Days = 3, // ۳۰ روز اخیر
    Custom = 4,     // بازه دلخواه
}

/// <summary>بازه تاریخ بسته (شامل هر دو سر)</summary>
public readonly record struct DateRange(DateOnly From, DateOnly To)
{
    public bool Contains(DateOnly date) => date >= From && date <= To;

    public int Days => To.DayNumber - From.DayNumber + 1;
}

public static class PersianMonths
{
    public static readonly string[] Names =
    [
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند",
    ];

    /// <summary>برچسب ماه شمسی، مثل «مهر ۱۴۰۳»</summary>
    public static string Label(int year, int month)
        => month is >= 1 and <= 12
            ? $"{Names[month - 1]} {PersianText.ToPersianDigits(year.ToString(CultureInfo.InvariantCulture))}"
            : $"{year}/{month}";
}

/// <summary>
/// تبدیل دورههای گزارشی به بازه تاریخ. مرزهای ماه/سال بر پایه تقویم هجری شمسی است.
/// </summary>
public static class PeriodResolver
{
    private static readonly PersianCalendar Calendar = new();

    public static DateRange Resolve(PeriodKind kind, DateOnly today, DateOnly? customFrom = null, DateOnly? customTo = null)
    {
        var (year, month) = PersianYearMonth(today);
        return kind switch
        {
            PeriodKind.ThisMonth => new DateRange(FirstOfMonth(year, month), LastOfMonth(year, month)),
            PeriodKind.LastMonth => ShiftRange(year, month, -1),
            PeriodKind.ThisYear => new DateRange(FirstOfMonth(year, 1), LastOfMonth(year, 12)),
            PeriodKind.Last30Days => new DateRange(today.AddDays(-29), today),
            _ => CustomRange(customFrom, customTo, today),
        };
    }

    public static (int Year, int Month) PersianYearMonth(DateOnly date)
    {
        var dt = date.ToDateTime(TimeOnly.MinValue);
        return (Calendar.GetYear(dt), Calendar.GetMonth(dt));
    }

    public static DateOnly FirstOfMonth(int year, int month)
        => DateOnly.FromDateTime(Calendar.ToDateTime(year, month, 1, 0, 0, 0, 0));

    public static DateOnly LastOfMonth(int year, int month)
        => DateOnly.FromDateTime(Calendar.ToDateTime(year, month, Calendar.GetDaysInMonth(year, month), 0, 0, 0, 0));

    /// <summary>فهرست ماههای شمسی از «از» تا «به» (برای سریهای ماهانه نمودارها)</summary>
    public static IEnumerable<(int Year, int Month)> EnumerateMonths(DateOnly from, DateOnly to)
    {
        var (year, month) = PersianYearMonth(from);
        var (endYear, endMonth) = PersianYearMonth(to);
        while (year < endYear || (year == endYear && month <= endMonth))
        {
            yield return (year, month);
            (year, month) = ShiftMonth(year, month, 1);
        }
    }

    /// <summary>بازه N ماه شمسی منتهی به ماه جاری</summary>
    public static DateRange LastNMonths(DateOnly today, int count)
    {
        if (count < 1)
        {
            count = 1;
        }

        var (year, month) = PersianYearMonth(today);
        var (startYear, startMonth) = ShiftMonth(year, month, -(count - 1));
        return new DateRange(FirstOfMonth(startYear, startMonth), LastOfMonth(year, month));
    }

    private static DateRange ShiftRange(int year, int month, int delta)
    {
        var (y, m) = ShiftMonth(year, month, delta);
        return new DateRange(FirstOfMonth(y, m), LastOfMonth(y, m));
    }

    private static (int Year, int Month) ShiftMonth(int year, int month, int delta)
    {
        var index = (year * 12) + (month - 1) + delta;
        return (index / 12, (index % 12) + 1);
    }

    private static DateRange CustomRange(DateOnly? from, DateOnly? to, DateOnly today)
    {
        var f = from ?? today.AddDays(-29);
        var t = to ?? today;
        if (f > t)
        {
            (f, t) = (t, f);
        }

        return new DateRange(f, t);
    }
}
