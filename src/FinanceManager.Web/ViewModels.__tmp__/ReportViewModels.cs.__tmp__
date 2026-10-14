using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Web.Helpers;

namespace FinanceManager.Web.ViewModels;

public class SummaryViewModel
{
    public required PeriodSelection Selection { get; init; }

    public required List<SummaryRow> Rows { get; init; }
}

public class MonthlyReportViewModel
{
    public required PeriodSelection Selection { get; init; }

    public required List<MonthlyPointRow> Points { get; init; }

    public required string CurrencyCode { get; init; }

    public required List<string> Currencies { get; init; }
}

public class AccountReportViewModel
{
    public required PeriodSelection Selection { get; init; }

    public required List<AccountReportRow> Rows { get; init; }

    public string? CurrencyCode { get; init; }

    public required List<string> Currencies { get; init; }
}

public class CategoryReportViewModel
{
    public required PeriodSelection Selection { get; init; }

    public required CategoryReport Report { get; init; }

    public required List<string> Currencies { get; init; }

    public TransactionType Type { get; init; } = TransactionType.Withdrawal;
}
