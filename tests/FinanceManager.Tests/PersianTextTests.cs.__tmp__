using FinanceManager.Core.Text;

namespace FinanceManager.Tests;

public class PersianTextTests
{
    [Fact]
    public void ToPersianDigits_ConvertsAsciiDigits()
    {
        Assert.Equal("۱۴۰۳/۰۷/۲۲", PersianText.ToPersianDigits("1403/07/22"));
    }

    [Fact]
    public void NormalizeNumber_HandlesPersianAndArabicDigits()
    {
        Assert.Equal("1250.50", PersianText.NormalizeNumber("۱۲۵۰٫۵۰"));
        Assert.Equal("1250.50", PersianText.NormalizeNumber("١٢٥٠٫٥٠"));
        Assert.Equal("1250000", PersianText.NormalizeNumber("۱٬۲۵۰٬۰۰۰"));
        Assert.Equal("1250000", PersianText.NormalizeNumber("1,250,000"));
    }

    [Fact]
    public void FormatDate_KnownReference_Nowruz1403()
    {
        // نوروز ۱۴۰۳ برابر ۲۰ مارس ۲۰۲۴ است
        var date = new DateOnly(2024, 3, 20);
        Assert.Equal("۱۴۰۳/۰۱/۰۱", PersianText.FormatDate(date));
    }

    [Fact]
    public void ParseDate_RoundTrips()
    {
        var original = new DateOnly(2024, 10, 13); // ۱۴۰۳/۰۷/۲۲
        var text = PersianText.FormatDate(original);

        Assert.True(PersianText.TryParseDate(text, out var parsed));
        Assert.Equal(original, parsed);

        Assert.True(PersianText.TryParseDate("1403/07/22", out var parsedLatin));
        Assert.Equal(original, parsedLatin);

        Assert.True(PersianText.TryParseDate("۱۴۰۳-۷-۲۲", out var parsedSingle));
        Assert.Equal(original, parsedSingle);
    }

    [Theory]
    [InlineData("1403/13/01")]
    [InlineData("1403/01/32")]
    [InlineData("salam")]
    [InlineData("1403/07")]
    [InlineData("")]
    [InlineData("9999/01/01")]
    public void ParseDate_RejectsInvalid(string input)
    {
        Assert.False(PersianText.TryParseDate(input, out _));
    }

    [Fact]
    public void LocalizeNumber_FormatsSeparators()
    {
        Assert.Equal("۱٬۲۵۰٫۵", PersianText.LocalizeNumber("1,250.5"));
    }
}
