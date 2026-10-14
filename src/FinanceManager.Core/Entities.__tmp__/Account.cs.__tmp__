namespace FinanceManager.Core.Entities;

/// <summary>حساب بانکی. مبالغ به واحد پول خرد (minor units) همان ارز ذخیره میشوند.</summary>
public class Account
{
    public int Id { get; set; }

    /// <summary>نام بانک</summary>
    public required string BankName { get; set; }

    /// <summary>عنوان/نام مستعار حساب، مثل «حساب حقوق»</summary>
    public required string Title { get; set; }

    public AccountType Type { get; set; } = AccountType.Checking;

    /// <summary>شماره حساب (اطلاعات حساس — در فهرستها پوشانده میشود)</summary>
    public string? AccountNumber { get; set; }

    /// <summary>شماره شبا (اطلاعات حساس — در فهرستها پوشانده میشود)</summary>
    public string? Iban { get; set; }

    /// <summary>کد ارز، مثل IRT, IRR, USD</summary>
    public required string CurrencyCode { get; set; }

    /// <summary>موجودی اولیه پیش از هر تراکنش ثبتشده (واحد خرد)</summary>
    public long OpeningBalanceMinor { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public List<Card> Cards { get; set; } = [];
    public List<Transaction> Transactions { get; set; } = [];
}
