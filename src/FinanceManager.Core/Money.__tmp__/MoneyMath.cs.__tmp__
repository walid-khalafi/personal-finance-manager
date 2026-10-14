using System.Globalization;
using FinanceManager.Core.Text;

namespace FinanceManager.Core.Money;

public enum AmountParseStatus
{
    Ok = 0,
    Empty = 1,
    NotANumber = 2,
    TooManyDecimals = 3,
    OutOfRange = 4,
}

/// <summary>
/// تبدیل دقیق مبالغ بین متن ورودی، decimal و واحد خرد (minor units).
/// هیچ گردکردن خاموشی انجام نمیشود؛ مبلغ با اعشار بیشتر از دقت ارز رد میشود.
/// </summary>
public static class MoneyMath
{
    private const int MaxInputLength = 32;

    /// <summary>تبدیل مبلغ متنی (با پذیرش اعداد فارسی) به واحد خرد همان ارز.</summary>
    public static AmountParseStatus ParseAmount(string? input, int decimals, out long minorUnits)
    {
        minorUnits = 0;
        if (string.IsNullOrWhiteSpace(input))
        {
            return AmountParseStatus.Empty;
        }

        var normalized = PersianText.NormalizeNumber(input);
        if (normalized.Length == 0 || normalized.Length > MaxInputLength)
        {
            return AmountParseStatus.NotANumber;
        }

        if (!decimal.TryParse(
                normalized,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var value))
        {
            return AmountParseStatus.NotANumber;
        }

        var factor = Pow10(decimals);
        decimal scaled;
        try
        {
            scaled = value * factor;
        }
        catch (OverflowException)
        {
            return AmountParseStatus.OutOfRange;
        }

        if (scaled != decimal.Truncate(scaled))
        {
            return AmountParseStatus.TooManyDecimals;
        }

        if (scaled < long.MinValue || scaled > long.MaxValue)
        {
            return AmountParseStatus.OutOfRange;
        }

        minorUnits = (long)scaled;
        return AmountParseStatus.Ok;
    }

    /// <summary>تبدیل واحد خرد به decimal (فقط برای نمایش/محاسبات؛ ذخیرهسازی همیشه بهصورت عدد صحیح است).</summary>
    public static decimal ToDecimal(long minorUnits, int decimals) => minorUnits / Pow10(decimals);

    /// <summary>تبدیل مبلغ decimal اعتبارسنجیشده به واحد خرد (فقط زمانی که مقدار دقیقاً قابل نمایش باشد).</summary>
    public static bool TryToMinor(decimal value, int decimals, out long minorUnits)
    {
        minorUnits = 0;
        var scaled = value * Pow10(decimals);
        if (scaled != decimal.Truncate(scaled) || scaled < long.MinValue || scaled > long.MaxValue)
        {
            return false;
        }

        minorUnits = (long)scaled;
        return true;
    }

    private static decimal Pow10(int decimals)
    {
        if (decimals is < 0 or > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(decimals), decimals, "تعداد رقم اعشار باید بین ۰ و ۱۵ باشد.");
        }

        return decimals switch
        {
            0 => 1m,
            1 => 10m,
            2 => 100m,
            3 => 1000m,
            4 => 10000m,
            _ => (decimal)Math.Pow(10, decimals),
        };
    }
}
