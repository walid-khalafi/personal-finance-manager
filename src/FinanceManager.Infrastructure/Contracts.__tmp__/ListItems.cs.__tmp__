using FinanceManager.Core.Entities;
using FinanceManager.Core.Money;

namespace FinanceManager.Infrastructure.Contracts;

public sealed class AccountListItem
{
    public int Id { get; init; }

    public string BankName { get; init; } = "";

    public string Title { get; init; } = "";

    public AccountType Type { get; init; }

    public string CurrencyCode { get; init; } = "";

    public string? AccountNumber { get; init; }

    public string? Iban { get; init; }

    public long OpeningBalanceMinor { get; init; }

    public bool IsActive { get; init; }

    public string? Notes { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public long DepositsMinor { get; init; }

    public long WithdrawalsMinor { get; init; }

    public long BalanceMinor => LedgerMath.ComputeBalance(OpeningBalanceMinor, DepositsMinor, WithdrawalsMinor);
}

public sealed class CardListItem
{
    public int Id { get; init; }

    public int AccountId { get; init; }

    public string AccountTitle { get; init; } = "";

    public string BankName { get; init; } = "";

    public string CurrencyCode { get; init; } = "";

    public string Nickname { get; init; } = "";

    public string Last4 { get; init; } = "";

    public int? ExpiryMonth { get; init; }

    public int? ExpiryYear { get; init; }

    public bool IsActive { get; init; }

    public string? Notes { get; init; }

    public int TransactionCount { get; init; }
}

public sealed class CategoryListItem
{
    public int Id { get; init; }

    public string Name { get; init; } = "";

    public CategoryKind Kind { get; init; }

    public bool IsActive { get; init; }

    public int SortOrder { get; init; }

    public int TransactionCount { get; init; }
}

public sealed class TransactionListItem
{
    public int Id { get; init; }

    public int AccountId { get; init; }

    public string AccountTitle { get; init; } = "";

    public string BankName { get; init; } = "";

    public string CurrencyCode { get; init; } = "";

    public TransactionType Type { get; init; }

    public long AmountMinor { get; init; }

    public DateOnly TransactionDate { get; init; }

    public int? CategoryId { get; init; }

    public string? CategoryName { get; init; }

    public string Description { get; init; } = "";

    public string? ReferenceNumber { get; init; }

    public int? CardId { get; init; }

    public string? CardNickname { get; init; }

    public Guid? TransferGroupId { get; init; }

    public bool IsTransfer => TransferGroupId is not null;

    public DateTime CreatedAtUtc { get; init; }
}

public sealed class TransferListItem
{
    public Guid GroupId { get; init; }

    public DateOnly TransactionDate { get; init; }

    public string? Description { get; init; }

    public long AmountMinor { get; init; }

    public string CurrencyCode { get; init; } = "";

    public int FromAccountId { get; init; }

    public string FromAccountTitle { get; init; } = "";

    public string FromBankName { get; init; } = "";

    public int ToAccountId { get; init; }

    public string ToAccountTitle { get; init; } = "";

    public string ToBankName { get; init; } = "";
}
