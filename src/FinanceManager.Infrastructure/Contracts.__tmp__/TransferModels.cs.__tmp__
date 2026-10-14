using FinanceManager.Core.Entities;

namespace FinanceManager.Infrastructure.Contracts;

public sealed class TransferLeg
{
    public int TransactionId { get; init; }

    public int AccountId { get; init; }

    public string AccountTitle { get; init; } = "";

    public TransactionType Type { get; init; }

    public long AmountMinor { get; init; }

    public DateOnly TransactionDate { get; init; }

    public string Description { get; init; } = "";

    public string CurrencyCode { get; init; } = "";
}

public sealed class TransferDetail
{
    public Guid GroupId { get; init; }

    public required TransferLeg From { get; init; }

    public required TransferLeg To { get; init; }
}
