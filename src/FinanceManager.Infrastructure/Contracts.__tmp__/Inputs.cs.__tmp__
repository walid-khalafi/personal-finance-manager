using FinanceManager.Core.Entities;

namespace FinanceManager.Infrastructure.Contracts;

public sealed class AccountInput
{
    public required string BankName { get; init; }

    public required string Title { get; init; }

    public AccountType Type { get; init; } = AccountType.Checking;

    public string? AccountNumber { get; init; }

    public string? Iban { get; init; }

    public required string CurrencyCode { get; init; }

    public long OpeningBalanceMinor { get; init; }

    public string? Notes { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed class CardInput
{
    public int AccountId { get; init; }

    public required string Nickname { get; init; }

    public required string Last4 { get; init; }

    public int? ExpiryMonth { get; init; }

    public int? ExpiryYear { get; init; }

    public string? Notes { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed class CategoryInput
{
    public required string Name { get; init; }

    public CategoryKind Kind { get; init; } = CategoryKind.Both;

    public int SortOrder { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed class TransactionInput
{
    public int AccountId { get; init; }

    public TransactionType Type { get; init; }

    /// <summary>مبلغ به واحد خرد ارز حساب؛ باید مثبت باشد</summary>
    public long AmountMinor { get; init; }

    public DateOnly TransactionDate { get; init; }

    public int? CategoryId { get; init; }

    public required string Description { get; init; }

    public string? ReferenceNumber { get; init; }

    public int? CardId { get; init; }
}

public sealed class TransferInput
{
    public int FromAccountId { get; init; }

    public int ToAccountId { get; init; }

    /// <summary>مبلغ به واحد خرد ارز هر دو حساب (ارزها باید یکسان باشند)</summary>
    public long AmountMinor { get; init; }

    public DateOnly TransactionDate { get; init; }

    public string? Description { get; init; }
}
