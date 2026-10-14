using System.Globalization;
using FinanceManager.Core.Text;

namespace FinanceManager.Core.Money;

/// <summary>قالببندی مبالغ برای نمایش فارسی (ارقام فارسی، جداکنندههای فارسی).</summary>
public static class MoneyFormatter
{
    /// <summary>مبلغ (واحد خرد) با تعداد اعشار مشخص → متن با ارقام فارسی.</summary>
    public static string Format(long minorUnits, int decimals)
    {
        var value = MoneyMath.ToDecimal(minorUnits, decimals);
        var ascii = value.ToString("N" + decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        return PersianText.LocalizeNumber(ascii);
    }

    /// <summary>مبلغ (واحد خرد) با دقت ارز مشخصشده → متن با ارقام فارسی.</summary>
    public static string Format(long minorUnits, string currencyCode)
        => Format(minorUnits, CurrencyCatalog.Get(currencyCode).Decimals);

    /// <summary>مبلغ همراه با نام ارز، مثل «۱٬۲۵۰٬۰۰۰ تومان».</summary>
    public static string FormatWithName(long minorUnits, string currencyCode)
    {
        var info = CurrencyCatalog.Get(currencyCode);
        return $"{Format(minorUnits, info.Decimals)} {info.PersianName}";
    }
}
