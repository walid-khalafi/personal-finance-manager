namespace FinanceManager.Core.Entities;

/// <summary>کارت بانکی مرتبط با یک حساب. کارت، حساب مالی مستقل نیست.</summary>
public class Card
{
    public int Id { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    /// <summary>نام مستعار کارت</summary>
    public required string Nickname { get; set; }

    /// <summary>چهار رقم آخر شماره کارت</summary>
    public required string Last4 { get; set; }

    /// <summary>ماه انقضا (۱ تا ۱۲) — اختیاری</summary>
    public int? ExpiryMonth { get; set; }

    /// <summary>سال انقضا — اختیاری</summary>
    public int? ExpiryYear { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }
}
