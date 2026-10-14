using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Core.Money;
using FinanceManager.Core.Time;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.Helpers;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

public class ReportsController(
    ReportService reports,
    SettingsService settings,
    AccountService accounts,
    CategoryService categories,
    TransactionService transactions) : Controller
{
    public IActionResult Index() => View();

    public async Task<IActionResult> Summary(
        int period = (int)PeriodKind.ThisMonth,
        string? from = null,
        string? to = null,
        CancellationToken ct = default)
    {
        var selection = PeriodSelection.Resolve(period, from, to);
        var rows = await reports.GetSummaryAsync(selection.Range, ct);

        return View(new SummaryViewModel { Selection = selection, Rows = rows });
    }

    public async Task<IActionResult> Monthly(
        int period = (int)PeriodKind.ThisMonth,
        string? from = null,
        string? to = null,
        string? currency = null,
        CancellationToken ct = default)
    {
        var selection = PeriodSelection.Resolve(period, from, to);
        var currencies = await GetCurrenciesAsync(ct);
        var code = await ResolveCurrencyAsync(currency, currencies, ct);

        var points = code is null
            ? new List<MonthlyPointRow>()
            : await reports.GetMonthlySeriesAsync(selection.Range, code, ct);

        return View(new MonthlyReportViewModel
        {
            Selection = selection,
            Points = points,
            CurrencyCode = code ?? "",
            Currencies = currencies,
        });
    }

    public async Task<IActionResult> Accounts(
        int period = (int)PeriodKind.ThisMonth,
        string? from = null,
        string? to = null,
        string? currency = null,
        CancellationToken ct = default)
    {
        var selection = PeriodSelection.Resolve(period, from, to);
        var currencies = await GetCurrenciesAsync(ct);
        var code = await ResolveCurrencyAsync(currency, currencies, ct);

        var rows = code is null
            ? new List<AccountReportRow>()
            : await reports.GetAccountReportAsync(selection.Range, code, ct);

        return View(new AccountReportViewModel
        {
            Selection = selection,
            Rows = rows,
            CurrencyCode = code,
            Currencies = currencies,
        });
    }

    public async Task<IActionResult> Categories(
        int period = (int)PeriodKind.ThisMonth,
        string? from = null,
        string? to = null,
        string? currency = null,
        int type = (int)TransactionType.Withdrawal,
        CancellationToken ct = default)
    {
        var selection = PeriodSelection.Resolve(period, from, to);
        var currencies = await GetCurrenciesAsync(ct);
        var code = await ResolveCurrencyAsync(currency, currencies, ct);
        var txType = Enum.IsDefined(typeof(TransactionType), type) ? (TransactionType)type : TransactionType.Withdrawal;

        var report = code is null
            ? new CategoryReport { CurrencyCode = "", Type = txType }
            : await reports.GetCategoryReportAsync(selection.Range, code, txType, ct);

        return View(new CategoryReportViewModel
        {
            Selection = selection,
            Report = report,
            Currencies = currencies,
            Type = txType,
        });
    }

    public async Task<IActionResult> Transactions(
        int? accountId = null,
        int? type = null,
        int? categoryId = null,
        string? from = null,
        string? to = null,
        string? q = null,
        int sort = (int)TransactionSort.DateDesc,
        int page = 1,
        CancellationToken ct = default)
    {
        var filter = ListQueries.BuildTransactionFilter(accountId, type, categoryId, from, to, q, sort, page, out var filterError);
        var paged = await transactions.SearchAsync(filter, ct);

        return View(new TransactionListViewModel
        {
            Page = paged,
            Filter = filter,
            Accounts = await accounts.GetAllAsync(ct),
            Categories = await categories.GetCategoriesAsync(includeInactive: true, ct),
            FromText = from,
            ToText = to,
            FilterError = filterError,
            PageBaseUrl = ListQueries.BuildPageBaseUrl(Request),
        });
    }

    private async Task<List<string>> GetCurrenciesAsync(CancellationToken ct)
    {
        var all = await accounts.GetAllAsync(ct);
        return all
            .Select(a => a.CurrencyCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<string?> ResolveCurrencyAsync(string? requested, List<string> currencies, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(requested) && CurrencyCatalog.Contains(requested))
        {
            return requested.Trim().ToUpperInvariant();
        }

        var fallback = await settings.GetDefaultCurrencyAsync(ct);
        if (currencies.Contains(fallback, StringComparer.OrdinalIgnoreCase))
        {
            return currencies.First(c => string.Equals(c, fallback, StringComparison.OrdinalIgnoreCase));
        }

        return currencies.Count > 0 ? currencies[0] : null;
    }
}
