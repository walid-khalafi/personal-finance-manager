using FinanceManager.Core.Money;

namespace FinanceManager.Tests;

public class MoneyMathTests
{
    [Theory]
    [InlineData("1000", 0, 1000)]
    [InlineData("1000", 2, 100000)]
    [InlineData("1000.50", 2, 100050)]
    [InlineData("0.01", 2, 1)]
    [InlineData("1.234", 3, 1234)]
    [InlineData("۱۲۵۰", 0, 1250)]           // ارقام فارسی
    [InlineData("۱٬۲۵۰٬۰۰۰", 0, 1250000)]    // جداکننده فارسی
    [InlineData("1,250,000", 0, 1250000)]    // جداکننده لاتین
    [InlineData("۱۲۵۰٫۵۰", 2, 125050)]       // ممیز فارسی
    [InlineData(" 42 ", 0, 42)]
    public void ParseAmount_ValidInputs(string input, int decimals, long expected)
    {
        var status = MoneyMath.ParseAmount(input, decimals, out var minor);
        Assert.Equal(AmountParseStatus.Ok, status);
        Assert.Equal(expected, minor);
    }

    [Theory]
    [InlineData("100.5", 0, AmountParseStatus.TooManyDecimals)]
    [InlineData("100.555", 2, AmountParseStatus.TooManyDecimals)]
    [InlineData("abc", 2, AmountParseStatus.NotANumber)]
    [InlineData("1.2.3", 2, AmountParseStatus.NotANumber)]
    [InlineData("", 2, AmountParseStatus.Empty)]
    [InlineData("   ", 2, AmountParseStatus.Empty)]
    public void ParseAmount_InvalidInputs(string input, int decimals, AmountParseStatus expected)
    {
        var status = MoneyMath.ParseAmount(input, decimals, out _);
        Assert.Equal(expected, status);
    }

    [Fact]
    public void ParseAmount_ZeroAndNegative_AreParsedButNotFinancialValid()
    {
        Assert.Equal(AmountParseStatus.Ok, MoneyMath.ParseAmount("0", 2, out var zero));
        Assert.Equal(0, zero);
        Assert.Equal(AmountParseStatus.Ok, MoneyMath.ParseAmount("-5.00", 2, out var negative));
        Assert.Equal(-500, negative);
    }

    [Fact]
    public void ToDecimal_RoundTrip()
    {
        Assert.Equal(1250.50m, MoneyMath.ToDecimal(125050, 2));
        Assert.Equal(1250m, MoneyMath.ToDecimal(1250, 0));
        Assert.Equal(1.234m, MoneyMath.ToDecimal(1234, 3));
    }

    [Fact]
    public void TryToMinor_RejectsExcessPrecision()
    {
        Assert.False(MoneyMath.TryToMinor(1.005m, 2, out _));
        Assert.True(MoneyMath.TryToMinor(1.05m, 2, out var minor));
        Assert.Equal(105, minor);
    }

    [Fact]
    public void CurrencyCatalog_KnowsDecimalPrecision()
    {
        Assert.Equal(0, CurrencyCatalog.GetDecimals("IRT"));
        Assert.Equal(0, CurrencyCatalog.GetDecimals("JPY"));
        Assert.Equal(2, CurrencyCatalog.GetDecimals("USD"));
        Assert.Equal(3, CurrencyCatalog.GetDecimals("KWD"));
        Assert.Equal(2, CurrencyCatalog.GetDecimals("XXX-UNKNOWN")); // پیشفرض
    }

    [Fact]
    public void MoneyFormatter_UsesPersianDigitsAndSeparators()
    {
        var text = MoneyFormatter.Format(1_250_000, 0);
        Assert.Equal("۱٬۲۵۰٬۰۰۰", text);

        var withDecimals = MoneyFormatter.Format(125050, 2);
        Assert.Equal("۱٬۲۵۰٫۵۰", withDecimals);

        var withName = MoneyFormatter.FormatWithName(1_250_000, "IRT");
        Assert.Equal("۱٬۲۵۰٬۰۰۰ تومان", withName);
    }

    [Fact]
    public void MoneyFormatter_NegativeBalance()
    {
        var text = MoneyFormatter.Format(-1_300, 0);
        Assert.Equal("−۱٬۳۰۰", text);
    }

    [Fact]
    public void LedgerMath_SingleAuthoritativeRule()
    {
        // سناریوی سند الزامات: ۱۰۰۰ + ۵۰۰ − ۲۰۰ = ۱۳۰۰
        Assert.Equal(1300, LedgerMath.ComputeBalance(1000, 500, 200));
        Assert.Equal(-200, LedgerMath.ComputeBalance(1000, 0, 1200));
    }
}
