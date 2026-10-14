using FinanceManager.Core.Time;

namespace FinanceManager.Tests;

public class PeriodResolverTests
{
    // ۱۳ اکتبر ۲۰۲۴ = ۱۴۰۳/۰۷/۲۲ (۲۲ مهر ۱۴۰۳)
    private static readonly DateOnly Today = new(2024, 10, 13);

    [Fact]
    public void ThisMonth_UsesPersianMonthBoundaries()
    {
        // مهر ۱۴۰۳ از ۲۲ سپتامبر تا ۲۱ اکتبر ۲۰۲۴
        var range = PeriodResolver.Resolve(PeriodKind.ThisMonth, Today);
        Assert.Equal(new DateOnly(2024, 9, 22), range.From);
        Assert.Equal(new DateOnly(2024, 10, 21), range.To);
    }

    [Fact]
    public void LastMonth_IsShahrivar()
    {
        // شهریور ۱۴۰۳ از ۲۲ اوت تا ۲۱ سپتامبر ۲۰۲۴
        var range = PeriodResolver.Resolve(PeriodKind.LastMonth, Today);
        Assert.Equal(new DateOnly(2024, 8, 22), range.From);
        Assert.Equal(new DateOnly(2024, 9, 21), range.To);
    }

    [Fact]
    public void ThisYear_StartsAtNowruz()
    {
        var range = PeriodResolver.Resolve(PeriodKind.ThisYear, Today);
        Assert.Equal(new DateOnly(2024, 3, 20), range.From);

        // پایان سال همان بازه ۱۲ماهه منتهی به اسفند است
        var (year, month) = PeriodResolver.PersianYearMonth(range.To);
        Assert.Equal(1403, year);
        Assert.Equal(12, month);
    }

    [Fact]
    public void Last30Days_IncludesToday()
    {
        var range = PeriodResolver.Resolve(PeriodKind.Last30Days, Today);
        Assert.Equal(Today.AddDays(-29), range.From);
        Assert.Equal(Today, range.To);
        Assert.Equal(30, range.Days);
    }

    [Fact]
    public void CustomRange_SwapsReversedBounds()
    {
        var from = new DateOnly(2024, 10, 1);
        var to = new DateOnly(2024, 9, 1);
        var range = PeriodResolver.Resolve(PeriodKind.Custom, Today, from, to);
        Assert.Equal(to, range.From);
        Assert.Equal(from, range.To);
    }

    [Fact]
    public void EnumerateMonths_CoversAllPersianMonthsInRange()
    {
        var range = PeriodResolver.Resolve(PeriodKind.ThisMonth, Today);
        var months = PeriodResolver.EnumerateMonths(range.From, range.To).ToList();

        Assert.Single(months);
        Assert.Equal((1403, 7), months[0]);

        var yearRange = PeriodResolver.Resolve(PeriodKind.ThisYear, Today);
        Assert.Equal(12, PeriodResolver.EnumerateMonths(yearRange.From, yearRange.To).Count());
    }

    [Fact]
    public void LastNMonths_EndsAtGivenMonth()
    {
        var range = PeriodResolver.LastNMonths(Today, 6);

        var (startYear, startMonth) = PeriodResolver.PersianYearMonth(range.From);
        Assert.Equal((1403, 2), (startYear, startMonth)); // اردیبهشت ۱۴۰۳

        var (endYear, endMonth) = PeriodResolver.PersianYearMonth(range.To);
        Assert.Equal((1403, 7), (endYear, endMonth)); // مهر ۱۴۰۳
    }
}
