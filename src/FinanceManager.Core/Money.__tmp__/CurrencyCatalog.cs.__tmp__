namespace FinanceManager.Core.Money;

/// <summary>اطلاعات یک ارز: کد، تعداد رقم اعشار و نام فارسی</summary>
public sealed record CurrencyInfo(string Code, int Decimals, string PersianName);

/// <summary>
/// فهرست ارزهای پشتیبانیشده با دقت اعشار آنها.
/// دقت اعشار برای ذخیرهسازی مبلغ به واحد خرد (minor units) حیاتی است.
/// </summary>
public static class CurrencyCatalog
{
    public const int DefaultDecimals = 2;

    private static readonly (string Code, int Decimals, string Name)[] Data =
    [
        ("IRT", 0, "تومان"),
        ("IRR", 0, "ریال"),
        ("USD", 2, "دلار آمریکا"),
        ("EUR", 2, "یورو"),
        ("GBP", 2, "پوند انگلیس"),
        ("AED", 2, "درهم امارات"),
        ("TRY", 2, "لیر ترکیه"),
        ("CNY", 2, "یوان چین"),
        ("JPY", 0, "یِن ژاپن"),
        ("KRW", 0, "وون کره"),
        ("CAD", 2, "دلار کانادا"),
        ("AUD", 2, "دلار استرالیا"),
        ("CHF", 2, "فرانک سوئیس"),
        ("SEK", 2, "کرون سوئد"),
        ("DKK", 2, "کرون دانمارک"),
        ("NOK", 2, "کرون نروژ"),
        ("PLN", 2, "زلوتی لهستان"),
        ("RUB", 2, "روبل روسیه"),
        ("IQD", 3, "دینار عراق"),
        ("KWD", 3, "دینار کویت"),
        ("BHD", 3, "دینار بحرین"),
        ("OMR", 3, "ریال عمان"),
        ("SAR", 2, "ریال سعودی"),
        ("QAR", 2, "ریال قطر"),
        ("INR", 2, "روپیه هند"),
        ("PKR", 2, "روپیه پاکستان"),
        ("AFN", 2, "افغانی"),
        ("AZN", 2, "منات آذربایجان"),
        ("AMD", 2, "درام ارمنستان"),
        ("GEL", 2, "لاری گرجستان"),
    ];

    private static readonly Dictionary<string, CurrencyInfo> ByCode =
        Data.ToDictionary(d => d.Code, d => new CurrencyInfo(d.Code, d.Decimals, d.Name), StringComparer.OrdinalIgnoreCase);

    /// <summary>همه ارزهای شناختهشده</summary>
    public static IReadOnlyList<CurrencyInfo> All { get; } = Data.Select(d => ByCode[d.Code]).ToList();

    public static bool Contains(string? code) => !string.IsNullOrWhiteSpace(code) && ByCode.ContainsKey(code);

    /// <summary>اطلاعات ارز؛ برای کدهای ناشناخته مقدار پیشفرض (۲ رقم اعشار) برگردانده میشود.</summary>
    public static CurrencyInfo Get(string code)
        => ByCode.TryGetValue(code, out var info) ? info : new CurrencyInfo(code.Trim().ToUpperInvariant(), DefaultDecimals, code.Trim().ToUpperInvariant());

    public static int GetDecimals(string code) => Get(code).Decimals;

    public static bool TryGet(string? code, out CurrencyInfo info)
    {
        if (!string.IsNullOrWhiteSpace(code) && ByCode.TryGetValue(code, out var found))
        {
            info = found;
            return true;
        }

        info = null!;
        return false;
    }
}
