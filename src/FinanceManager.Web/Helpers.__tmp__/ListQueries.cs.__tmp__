using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Core.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace FinanceManager.Web.Helpers;

/// <summary>ساخت فیلترهای فهرست از پارامترهای QueryString و ساخت آدرس صفحهبندی</summary>
public static class ListQueries
{
    public static TransactionFilter BuildTransactionFilter(
        int? accountId,
        int? type,
        int? categoryId,
        string? from,
        string? to,
        string? q,
        int sort,
        int page,
        out string? filterError)
    {
        var fromOk = true;
        var toOk = true;
        var fromDate = default(DateOnly);
        var toDate = default(DateOnly);

        if (!string.IsNullOrWhiteSpace(from) && !PersianText.TryParseDate(from, out fromDate))
        {
            fromOk = false;
        }

        if (!string.IsNullOrWhiteSpace(to) && !PersianText.TryParseDate(to, out toDate))
        {
            toOk = false;
        }

        filterError = !fromOk || !toOk
            ? "تاریخهای فیلتر معتبر نیستند؛ نمونه صحیح: ۱۴۰۳/۰۷/۲۲"
            : null;

        return new TransactionFilter
        {
            AccountId = accountId,
            Type = type is int t && Enum.IsDefined(typeof(TransactionType), t) ? (TransactionType)t : null,
            CategoryId = categoryId,
            From = fromOk && !string.IsNullOrWhiteSpace(from) ? fromDate : null,
            To = toOk && !string.IsNullOrWhiteSpace(to) ? toDate : null,
            Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            Sort = Enum.IsDefined(typeof(TransactionSort), sort) ? (TransactionSort)sort : TransactionSort.DateDesc,
            Page = page,
            PageSize = 20,
        };
    }

    public static string BuildPageBaseUrl(HttpRequest request)
    {
        var pairs = request.Query
            .Where(kv => !string.Equals(kv.Key, "page", StringComparison.OrdinalIgnoreCase))
            .SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, string?>(kv.Key, v)))
            .ToList();

        return QueryHelpers.AddQueryString(request.Path.ToString(), pairs);
    }
}
