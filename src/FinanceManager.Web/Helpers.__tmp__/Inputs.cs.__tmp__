using System.Globalization;
using FinanceManager.Core.Money;
using FinanceManager.Core.Text;

namespace FinanceManager.Web.Helpers;

/// <summary>تفسیر ورودیهای فرم (مبلغ و تاریخ) با پیامهای خطای فارسی</summary>
public static class Inputs
{
    public static bool TryParseMoney(string? text, string currencyCode, out long minorUnits, out string? error)
    {
        var decimals = CurrencyCatalog.GetDecimals(currencyCode);
        var status = MoneyMath.ParseAmount(text, decimals, out minorUnits);

        error = status switch
        {
            AmountParseStatus.Ok => null,
            AmountParseStatus.Empty => "مبلغ را وارد کنید.",
            AmountParseStatus.NotANumber => "مبلغ واردشده عدد معتبر نیست.",
            AmountParseStatus.TooManyDecimals => decimals == 0
                ? "این ارز رقم اعشار ندارد."
                : $"مبلغ حداکثر {PersianText.ToPersianDigits(decimals.ToString())} رقم اعشار دارد.",
            AmountParseStatus.OutOfRange => "مبلغ واردشده بسیار بزرگ است.",
            _ => "مبلغ واردشده معتبر نیست.",
        };

        return status == AmountParseStatus.Ok;
    }

    public static bool TryParseDate(string? text, out DateOnly date, out string? error)
    {
        if (PersianText.TryParseDate(text, out date))
        {
            error = null;
            return true;
        }

        error = "تاریخ واردشده معتبر نیست؛ نمونه صحیح: ۱۴۰۳/۰۷/۲۲";
        return false;
    }

    /// <summary>قالببندی مبلغ برای input فرم (ارقام لاتین، بدون جداکننده)</summary>
    public static string FormatMoneyForInput(long minorUnits, string currencyCode)
    {
        var info = CurrencyCatalog.Get(currencyCode);
        var value = MoneyMath.ToDecimal(minorUnits, info.Decimals);
        return value.ToString("F" + info.Decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}
