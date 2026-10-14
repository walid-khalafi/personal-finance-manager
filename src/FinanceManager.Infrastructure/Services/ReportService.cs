using FinanceManager.Core.Entities;
using FinanceManager.Core.Money;
using FinanceManager.Core.Time;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

/// <summary>
/// گزارشها و آمار داشبورد. همه محاسبات از رکوردهای واقعی دفتر کل انجام میشود.
/// انتقالهای داخلی از جمعهای درآمد/هزینه خارجی کنار گذاشته میشوند و
/// هیچگاه ارزهای مختلف با هم جمع نمیشوند.
/// </summary>
public class ReportService(AppDbContext db)
{
    public async Task<List<AccountBalanceRow>> GetAccountBalancesAsync(
        bool includeInactive = true,
        CancellationToken ct = default)
    {
        var query = db.Accounts.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(a => a.IsActive);
        }

        var accounts = await query
            .OrderBy(a => a.BankName)
            .ThenBy(a => a.Title)
            .Select(a => new
            {
                a.Id,
                a.BankName,
                a.Title,
                a.Type,
                a.CurrencyCode,
                a.IsActive,
                a.OpeningBalanceMinor,
            })
            .ToListAsync(ct);

        var sums = await BalanceQueries.GetAccountSumsAsync(db, ct);

        return accounts.Select(a =>
        {
            sums.TryGetValue(a.Id, out var s);
            return new AccountBalanceRow
            {
                AccountId = a.Id,
                BankName = a.BankName,
                Title = a.Title,
                Type = a.Type,
                CurrencyCode = a.CurrencyCode,
                IsActive = a.IsActive,
                OpeningBalanceMinor = a.OpeningBalanceMinor,
                BalanceMinor = LedgerMath.ComputeBalance(a.OpeningBalanceMinor, s.Deposits, s.Withdrawals),
            };
        }).ToList();
    }

    public async Task<DashboardData> GetDashboardAsync(
        DateRange range,
        string? currencyCode,
        DateOnly? today = null,
        CancellationToken ct = default)
    {
        var currentDay = today ?? DateOnly.FromDateTime(DateTime.Now);

        var accounts = await GetAccountBalancesAsync(includeInactive: false, ct: ct);

        var balancesByCurrency = accounts
            .GroupBy(a => a.CurrencyCode, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CurrencyBalanceRow
            {
                CurrencyCode = g.Key,
                TotalMinor = g.Sum(a => a.BalanceMinor),
                AccountCount = g.Count(),
            })
            .OrderByDescending(r => r.TotalMinor)
            .ToList();

        var availableCurrencies = accounts
            .Select(a => a.CurrencyCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        string? chartCurrency = null;
        if (availableCurrencies.Count > 0)
        {
            chartCurrency = currencyCode is not null && availableCurrencies.Contains(currencyCode, StringComparer.OrdinalIgnoreCase)
                ? availableCurrencies.First(c => string.Equals(c, currencyCode, StringComparison.OrdinalIgnoreCase))
                : availableCurrencies[0];
        }

        var periodTotals = await GetPeriodTotalsAsync(range, ct);

        var recentQuery = db.Transactions.AsNoTracking()
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id);
        var recent = await TransactionQueries.Project(recentQuery).Take(10).ToListAsync(ct);

        var monthly = chartCurrency is null
            ? new List<MonthlyPointRow>()
            : await GetMonthlySeriesAsync(PeriodResolver.LastNMonths(currentDay < range.To ? currentDay : range.To, 6), chartCurrency, ct);

        var categories = chartCurrency is null
            ? new List<CategorySliceRow>()
            : await GetCategoryDistributionAsync(range, chartCurrency, TransactionType.Withdrawal, ct);

        return new DashboardData
        {
            Range = range,
            ChartCurrencyCode = chartCurrency,
            ChartCurrencyName = chartCurrency is null ? null : CurrencyCatalog.Get(chartCurrency).PersianName,
            AvailableCurrencies = availableCurrencies,
            Accounts = accounts,
            BalancesByCurrency = balancesByCurrency,
            PeriodByCurrency = periodTotals,
            RecentTransactions = recent,
            MonthlySeries = monthly,
            CategoryDistribution = categories,
        };
    }

    /// <summary>جمع واریز/برداشت دورهای به تفکیک ارز — فقط جریانهای خارجی (بدون انتقالها)</summary>
    public async Task<List<PeriodTotalsRow>> GetPeriodTotalsAsync(DateRange range, CancellationToken ct = default)
    {
        var rows = await db.Transactions.AsNoTracking()
            .Where(t => t.TransferGroupId == null
                && t.TransactionDate >= range.From
                && t.TransactionDate <= range.To)
            .GroupBy(t => t.Account.CurrencyCode)
            .Select(g => new PeriodTotalsRow
            {
                CurrencyCode = g.Key,
                DepositsMinor = g.Sum(t => t.Type == TransactionType.Deposit ? t.AmountMinor : 0L),
                WithdrawalsMinor = g.Sum(t => t.Type == TransactionType.Withdrawal ? t.AmountMinor : 0L),
            })
            .ToListAsync(ct);

        return rows.OrderBy(r => r.CurrencyCode, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>سری ماهانه (شمسی) واریز/برداشت برای یک ارز — بدون انتقالها</summary>
    public async Task<List<MonthlyPointRow>> GetMonthlySeriesAsync(DateRange range, string currencyCode, CancellationToken ct = default)
    {
        currencyCode = currencyCode.Trim().ToUpperInvariant();

        var days = await db.Transactions.AsNoTracking()
            .Where(t => t.TransferGroupId == null
                && t.TransactionDate >= range.From
                && t.TransactionDate <= range.To
                && t.Account.CurrencyCode == currencyCode)
            .GroupBy(t => t.TransactionDate)
            .Select(g => new
            {
                Date = g.Key,
                Deposits = g.Sum(t => t.Type == TransactionType.Deposit ? t.AmountMinor : 0L),
                Withdrawals = g.Sum(t => t.Type == TransactionType.Withdrawal ? t.AmountMinor : 0L),
            })
            .ToListAsync(ct);

        var byMonth = days
            .GroupBy(d => PeriodResolver.PersianYearMonth(d.Date))
            .ToDictionary(g => g.Key, g => (Deposits: g.Sum(x => x.Deposits), Withdrawals: g.Sum(x => x.Withdrawals)));

        var months = new List<MonthlyPointRow>();
        foreach (var (year, month) in PeriodResolver.EnumerateMonths(range.From, range.To))
        {
            byMonth.TryGetValue((year, month), out var sums);
            months.Add(new MonthlyPointRow
            {
                Year = year,
                Month = month,
                DepositsMinor = sums.Deposits,
                WithdrawalsMinor = sums.Withdrawals,
            });
        }

        return months;
    }

    /// <summary>توزیع مبالغ بر اساس دستهبندی برای یک ارز و نوع تراکنش — بدون انتقالها</summary>
    public async Task<List<CategorySliceRow>> GetCategoryDistributionAsync(
        DateRange range,
        string currencyCode,
        TransactionType type,
        CancellationToken ct = default)
    {
        currencyCode = currencyCode.Trim().ToUpperInvariant();

        var raw = await db.Transactions.AsNoTracking()
            .Where(t => t.TransferGroupId == null
                && t.Type == type
                && t.TransactionDate >= range.From
                && t.TransactionDate <= range.To
                && t.Account.CurrencyCode == currencyCode)
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                TotalMinor = g.Sum(t => t.AmountMinor),
                Count = g.Count(),
            })
            .ToListAsync(ct);

        var categoryIds = raw.Where(r => r.CategoryId is not null).Select(r => r.CategoryId!.Value).ToList();
        var names = await db.Categories.AsNoTracking()
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        return raw
            .Select(r => new CategorySliceRow
            {
                CategoryId = r.CategoryId,
                Name = r.CategoryId is int id && names.TryGetValue(id, out var name) ? name : "بدون دستهبندی",
                TotalMinor = r.TotalMinor,
                Count = r.Count,
            })
            .OrderByDescending(r => r.TotalMinor)
            .ToList();
    }

    /// <summary>
    /// خلاصه مالی دورهای به تفکیک ارز:
    /// موجودی آغاز = موجودیهای اولیه + خالص دفتر تا پیش از شروع دوره؛
    /// واریز/برداشت دوره فقط جریانهای خارجی؛ موجودی پایان = آغاز + واریز − برداشت.
    /// </summary>
    public async Task<List<SummaryRow>> GetSummaryAsync(DateRange range, CancellationToken ct = default)
    {
        var openingByCurrency = await db.Accounts.AsNoTracking()
            .GroupBy(a => a.CurrencyCode)
            .Select(g => new { Currency = g.Key, Sum = g.Sum(a => a.OpeningBalanceMinor) })
            .ToListAsync(ct);

        var priorByCurrency = await db.Transactions.AsNoTracking()
            .Where(t => t.TransactionDate < range.From)
            .GroupBy(t => t.Account.CurrencyCode)
            .Select(g => new
            {
                Currency = g.Key,
                Net = g.Sum(t => t.Type == TransactionType.Deposit ? t.AmountMinor : -t.AmountMinor),
            })
            .ToListAsync(ct);

        var period = await GetPeriodTotalsAsync(range, ct);

        var counts = await db.Transactions.AsNoTracking()
            .Where(t => t.TransferGroupId == null
                && t.TransactionDate >= range.From
                && t.TransactionDate <= range.To)
            .GroupBy(t => t.Account.CurrencyCode)
            .Select(g => new { Currency = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var currencies = openingByCurrency.Select(x => x.Currency)
            .Concat(priorByCurrency.Select(x => x.Currency))
            .Concat(period.Select(x => x.CurrencyCode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var result = new List<SummaryRow>();
        foreach (var currency in currencies)
        {
            var opening = openingByCurrency.Where(x => Eq(x.Currency, currency)).Sum(x => x.Sum)
                + priorByCurrency.Where(x => Eq(x.Currency, currency)).Sum(x => x.Net);

            var p = period.FirstOrDefault(x => Eq(x.CurrencyCode, currency));
            var deposits = p?.DepositsMinor ?? 0;
            var withdrawals = p?.WithdrawalsMinor ?? 0;
            var count = counts.FirstOrDefault(x => Eq(x.Currency, currency))?.Count ?? 0;

            result.Add(new SummaryRow
            {
                CurrencyCode = currency,
                OpeningMinor = opening,
                DepositsMinor = deposits,
                WithdrawalsMinor = withdrawals,
                ClosingMinor = opening + deposits - withdrawals,
                TransactionCount = count,
            });
        }

        return result;
    }

    /// <summary>گزارش حسابها برای یک دوره؛ جمعها با دفتر کل قابل تطبیق است.</summary>
    public async Task<List<AccountReportRow>> GetAccountReportAsync(
        DateRange range,
        string? currencyCode = null,
        CancellationToken ct = default)
    {
        var accounts = await db.Accounts.AsNoTracking()
            .Where(a => currencyCode == null || a.CurrencyCode == currencyCode)
            .OrderBy(a => a.BankName)
            .ThenBy(a => a.Title)
            .Select(a => new
            {
                a.Id,
                a.BankName,
                a.Title,
                a.CurrencyCode,
                a.IsActive,
                a.OpeningBalanceMinor,
            })
            .ToListAsync(ct);

        if (accounts.Count == 0)
        {
            return [];
        }

        var ids = accounts.Select(a => a.Id).ToList();

        var before = await db.Transactions.AsNoTracking()
            .Where(t => ids.Contains(t.AccountId) && t.TransactionDate < range.From)
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Net = g.Sum(t => t.Type == TransactionType.Deposit ? t.AmountMinor : -t.AmountMinor),
            })
            .ToListAsync(ct);

        var period = await db.Transactions.AsNoTracking()
            .Where(t => ids.Contains(t.AccountId)
                && t.TransactionDate >= range.From
                && t.TransactionDate <= range.To)
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Deposits = g.Sum(t => t.TransferGroupId == null && t.Type == TransactionType.Deposit ? t.AmountMinor : 0L),
                Withdrawals = g.Sum(t => t.TransferGroupId == null && t.Type == TransactionType.Withdrawal ? t.AmountMinor : 0L),
                TransfersIn = g.Sum(t => t.TransferGroupId != null && t.Type == TransactionType.Deposit ? t.AmountMinor : 0L),
                TransfersOut = g.Sum(t => t.TransferGroupId != null && t.Type == TransactionType.Withdrawal ? t.AmountMinor : 0L),
            })
            .ToListAsync(ct);

        return accounts.Select(a =>
        {
            var beforeNet = before.FirstOrDefault(x => x.AccountId == a.Id)?.Net ?? 0;
            var p = period.FirstOrDefault(x => x.AccountId == a.Id);
            return new AccountReportRow
            {
                AccountId = a.Id,
                BankName = a.BankName,
                Title = a.Title,
                CurrencyCode = a.CurrencyCode,
                IsActive = a.IsActive,
                OpeningMinor = a.OpeningBalanceMinor + beforeNet,
                DepositsMinor = p?.Deposits ?? 0,
                WithdrawalsMinor = p?.Withdrawals ?? 0,
                TransfersInMinor = p?.TransfersIn ?? 0,
                TransfersOutMinor = p?.TransfersOut ?? 0,
            };
        }).ToList();
    }

    /// <summary>گزارش دستهبندیها همراه با سهم هر دسته از کل</summary>
    public async Task<CategoryReport> GetCategoryReportAsync(
        DateRange range,
        string currencyCode,
        TransactionType type,
        CancellationToken ct = default)
    {
        var slices = await GetCategoryDistributionAsync(range, currencyCode, type, ct);
        var total = slices.Sum(s => s.TotalMinor);
        var count = slices.Sum(s => s.Count);

        var rows = slices.Select(s => new CategoryReportRow
        {
            CategoryId = s.CategoryId,
            Name = s.Name,
            TotalMinor = s.TotalMinor,
            Count = s.Count,
            Share = total == 0 ? 0m : (decimal)s.TotalMinor / total,
        }).ToList();

        return new CategoryReport
        {
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            Type = type,
            Rows = rows,
            TotalMinor = total,
            TotalCount = count,
        };
    }

    private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
