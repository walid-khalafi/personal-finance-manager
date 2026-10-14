using System.Globalization;
using FinanceManager.Core.Entities;
using FinanceManager.Core.Money;
using FinanceManager.Core.Text;
using FinanceManager.Core.Time;

namespace FinanceManager.Web.Helpers;

/// <summary>کمکتابهای نمایش (فارسیسازی اعداد/تاریخها، برچسبها و پوشاندن داده حساس)</summary>
public static class Display
{
    public static string Money(long minorUnits, string currencyCode)
        => MoneyFormatter.Format(minorUnits, currencyCode);

    public static string MoneyWithName(long minorUnits, string currencyCode)
        => MoneyFormatter.FormatWithName(minorUnits, currencyCode);

    public static string Date(DateOnly date) => PersianText.FormatDate(date);

    public static string Timestamp(DateTime utc) => PersianText.FormatTimestamp(utc);

    public static string DateRange(DateRange range) => $"{PersianText.FormatDate(range.From)} تا {PersianText.FormatDate(range.To)}";

    /// <summary>پوشاندن شماره حساب/شبا: نمایش فقط ۴ نویسه آخر</summary>
    public static string Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "—";
        }

        var visible = value.Length <= 4 ? "" : value[^4..];
        var masked = new string('•', Math.Max(4, value.Length - 4));
        var text = visible.Length == 0 ? masked : $"{masked} {visible}";
        return PersianText.ToPersianDigits(text);
    }

    /// <summary>نمایش چهار رقم آخر کارت، مثل «•••• ۴۳۲۱»</summary>
    public static string MaskCard(string? last4)
        => string.IsNullOrWhiteSpace(last4) ? "—" : $"•••• {PersianText.ToPersianDigits(last4)}";

    public static string Percent(decimal share)
        => PersianText.ToPersianDigits((share * 100m).ToString("0.#", CultureInfo.InvariantCulture)) + "٪";

    public static string TransactionTypeLabel(TransactionType type)
        => type == TransactionType.Deposit ? "واریز" : "برداشت";

    public static string AccountTypeLabel(AccountType type) => type switch
    {
        AccountType.Checking => "جاری",
        AccountType.Savings => "پسانداز",
        AccountType.Deposit => "سپرده",
        _ => "سایر",
    };

    public static string CategoryKindLabel(CategoryKind kind) => kind switch
    {
        CategoryKind.Deposit => "فقط واریز",
        CategoryKind.Withdrawal => "فقط برداشت",
        _ => "عمومی",
    };

    public static string PeriodLabel(PeriodKind kind) => kind switch
    {
        PeriodKind.ThisMonth => "این ماه",
        PeriodKind.LastMonth => "ماه گذشته",
        PeriodKind.ThisYear => "امسال",
        PeriodKind.Last30Days => "۳۰ روز اخیر",
        _ => "بازه دلخواه",
    };

    public static string AmountClass(TransactionType type)
        => type == TransactionType.Deposit ? "amount-deposit" : "amount-withdrawal";

    /// <summary>نمایش حجم فایل با واحد فارسی</summary>
    public static string FileSize(long bytes)
    {
        string[] units = ["بایت", "کیلوبایت", "مگابایت", "گیگابایت"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{PersianText.LocalizeNumber(value.ToString("0.#", CultureInfo.InvariantCulture))} {units[unit]}";
    }

    public static string SignedAmount(long minorUnits, string currencyCode, TransactionType type)
    {
        var sign = type == TransactionType.Deposit ? "+" : "−";
        return sign + Money(minorUnits, currencyCode);
    }
}
