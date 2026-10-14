using FinanceManager.Core.Entities;
using FinanceManager.Core.Time;

namespace FinanceManager.Infrastructure.Contracts;

public sealed class AccountBalanceRow
{
    public int AccountId { get; init; }

    public string BankName { get; init; } = "";

    public string Title { get; init; } = "";

    public AccountType Type { get; init; }

    public string CurrencyCode { get; init; } = "";

    public bool IsActive { get; init; }

    public long OpeningBalanceMinor { get; init; }

    public long BalanceMinor { get; init; }
}

public sealed class CurrencyBalanceRow
{
    public string CurrencyCode { get; init; } = "";

    public long TotalMinor { get; init; }

    public int AccountCount { get; init; }
}

public sealed class PeriodTotalsRow
{
    public string CurrencyCode { get; init; } = "";

    public long DepositsMinor { get; init; }

    public long WithdrawalsMinor { get; init; }

    public long NetMinor => DepositsMinor - WithdrawalsMinor;
}

public sealed class MonthlyPointRow
{
    public int Year { get; init; }

    public int Month { get; init; }

    public long DepositsMinor { get; init; }

    public long WithdrawalsMinor { get; init; }

    public long NetMinor => DepositsMinor - WithdrawalsMinor;

    public string Label => PersianMonths.Label(Year, Month);
}

public sealed class CategorySliceRow
{
    public int? CategoryId { get; init; }

    public string Name { get; init; } = "";

    public long TotalMinor { get; init; }

    public int Count { get; init; }
}

public sealed class DashboardData
{
    public DateRange Range { get; init; }

    public string? ChartCurrencyCode { get; init; }

    public string? ChartCurrencyName { get; init; }

    public List<string> AvailableCurrencies { get; init; } = [];

    public List<AccountBalanceRow> Accounts { get; init; } = [];

    public List<CurrencyBalanceRow> BalancesByCurrency { get; init; } = [];

    public List<PeriodTotalsRow> PeriodByCurrency { get; init; } = [];

    public List<TransactionListItem> RecentTransactions { get; init; } = [];

    public List<MonthlyPointRow> MonthlySeries { get; init; } = [];

    public List<CategorySliceRow> CategoryDistribution { get; init; } = [];
}

public sealed class SummaryRow
{
    public string CurrencyCode { get; init; } = "";

    public long OpeningMinor { get; init; }

    public long DepositsMinor { get; init; }

    public long WithdrawalsMinor { get; init; }

    public long ClosingMinor { get; init; }

    public int TransactionCount { get; init; }

    public long NetMinor => DepositsMinor - WithdrawalsMinor;
}

public sealed class AccountReportRow
{
    public int AccountId { get; init; }

    public string BankName { get; init; } = "";

    public string Title { get; init; } = "";

    public string CurrencyCode { get; init; } = "";

    public bool IsActive { get; init; }

    public long OpeningMinor { get; init; }

    public long DepositsMinor { get; init; }

    public long WithdrawalsMinor { get; init; }

    public long TransfersInMinor { get; init; }

    public long TransfersOutMinor { get; init; }

    public long NetMinor => DepositsMinor + TransfersInMinor - WithdrawalsMinor - TransfersOutMinor;

    public long ClosingMinor => OpeningMinor + NetMinor;
}

public sealed class CategoryReportRow
{
    public int? CategoryId { get; init; }

    public string Name { get; init; } = "";

    public long TotalMinor { get; init; }

    public int Count { get; init; }

    public decimal Share { get; init; }
}

public sealed class CategoryReport
{
    public string CurrencyCode { get; init; } = "";

    public TransactionType Type { get; init; }

    public List<CategoryReportRow> Rows { get; init; } = [];

    public long TotalMinor { get; init; }

    public int TotalCount { get; init; }
}
