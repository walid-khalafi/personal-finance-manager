using System.Globalization;
using System.Text;

namespace FinanceManager.Core.Text;

/// <summary>
/// ابزار نمایش و تفسیر متن فارسی: ارقام فارسی/عربی، جداکنندههای عددی و تاریخ شمسی.
/// پیادهسازی مستقل از Culture سیستم است تا نتیجه در همه محیطها یکسان باشد.
/// </summary>
public static class PersianText
{
    private static readonly PersianCalendar Calendar = new();

    /// <summary>تبدیل ارقام لاتین به ارقام فارسی</summary>
    public static string ToPersianDigits(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            sb.Append(c is >= '0' and <= '9' ? (char)('۰' + (c - '0')) : c);
        }

        return sb.ToString();
    }

    /// <summary>تبدیل ارقام فارسی/عربی به ارقام لاتین (بقیه نویسهها حفظ میشوند)</summary>
    public static string NormalizeDigits(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var raw in input)
        {
            var c = raw;
            if (c is >= '۰' and <= '۹')
            {
                c = (char)('0' + (c - '۰'));
            }
            else if (c is >= '٠' and <= '٩')
            {
                c = (char)('0' + (c - '٠'));
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// نرمالسازی ورودی عددی: ارقام فارسی/عربی به لاتین، حذف جداکننده هزارگان و نویسههای نامرئی،
    /// و تبدیل ممیز فارسی «٫» به «.».
    /// </summary>
    public static string NormalizeNumber(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var raw in input)
        {
            var c = raw;
            if (c is >= '۰' and <= '۹')
            {
                c = (char)('0' + (c - '۰'));
            }
            else if (c is >= '٠' and <= '٩')
            {
                c = (char)('0' + (c - '٠'));
            }

            switch (c)
            {
                case '٫': // ممیز فارسی
                    sb.Append('.');
                    break;
                case '٬' or '،' or ',' or ' ' or '\u00a0' or '\u202f' or '\u200c' or '\u200e' or '\u200f':
                    break; // جداکنندهها و نویسههای نامرئی حذف میشوند
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>بومیسازی یک عدد قالببندیشده لاتین: جداکننده هزارگان «٬»، ممیز «٫»، منفی «−» و ارقام فارسی.</summary>
    public static string LocalizeNumber(string asciiNumber)
    {
        var sb = new StringBuilder(asciiNumber.Length);
        foreach (var c in asciiNumber)
        {
            switch (c)
            {
                case ',':
                    sb.Append('٬');
                    break;
                case '.':
                    sb.Append('٫');
                    break;
                case '-':
                    sb.Append('−');
                    break;
                default:
                    sb.Append(c is >= '0' and <= '9' ? (char)('۰' + (c - '0')) : c);
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>قالببندی تاریخ شمسی، مثل «۱۴۰۳/۰۷/۲۲»</summary>
    public static string FormatDate(DateOnly date)
    {
        var dt = date.ToDateTime(TimeOnly.MinValue);
        return ToPersianDigits($"{Calendar.GetYear(dt):0000}/{Calendar.GetMonth(dt):00}/{Calendar.GetDayOfMonth(dt):00}");
    }

    /// <summary>قالببندی زمان ثبت (UTC) به وقت محلی و تاریخ شمسی، مثل «۱۴۰۳/۰۷/۲۲ ۲۱:۳۲»</summary>
    public static string FormatTimestamp(DateTime utc)
    {
        var local = utc.Kind == DateTimeKind.Utc ? utc.ToLocalTime() : utc;
        return $"{FormatDate(DateOnly.FromDateTime(local))} {ToPersianDigits(local.ToString("HH:mm", CultureInfo.InvariantCulture))}";
    }

    /// <summary>
    /// تفسیر تاریخ شمسی از ورودی کاربر؛ قالبهای «۱۴۰۳/۰۷/۲۲»، «1403-07-22» و «۱۴۰۳.۷.۲۲» پذیرفته میشوند.
    /// </summary>
    public static bool TryParseDate(string? input, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = NormalizeNumber(input);
        var parts = normalized.Split(['/', '-', '.'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var year) ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var month) ||
            !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var day))
        {
            return false;
        }

        if (year is < 1200 or > 1599 || month is < 1 or > 12 || day < 1)
        {
            return false;
        }

        if (day > Calendar.GetDaysInMonth(year, month))
        {
            return false;
        }

        date = DateOnly.FromDateTime(Calendar.ToDateTime(year, month, day, 0, 0, 0, 0));
        return true;
    }
}
