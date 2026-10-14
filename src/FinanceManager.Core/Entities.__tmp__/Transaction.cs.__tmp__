namespace FinanceManager.Core.Entities;

/// <summary>
/// یک قلم دفتر کل: واریز یا برداشت روی یک حساب.
/// مبلغ همیشه مثبت است؛ نوع تراکنش تعیین میکند که موجودی افزایش یا کاهش مییابد.
/// </summary>
public class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public TransactionType Type { get; set; }

    /// <summary>مبلغ به واحد خرد ارز حساب؛ همیشه مثبت (واحد خرد)</summary>
    public long AmountMinor { get; set; }

    /// <summary>تاریخ تراکنش (تقویم میلادی ذخیره میشود؛ در رابط کاربری شمسی نمایش داده میشود)</summary>
    public DateOnly TransactionDate { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>شرح تراکنش</summary>
    public required string Description { get; set; }

    /// <summary>شماره پیگیری/مرجع — اختیاری</summary>
    public string? ReferenceNumber { get; set; }

    public int? CardId { get; set; }
    public Card? Card { get; set; }

    /// <summary>زمان ثبت (UTC) — متفاوت از تاریخ تقویمی تراکنش</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// شناسه گروه انتقال: اگر مقدار داشته باشد، این قلم بخشی از یک انتقال داخلی است
    /// و در محاسبات درآمد/هزینه خارجی لحاظ نمیشود.
    /// </summary>
    public Guid? TransferGroupId { get; set; }

    public bool IsTransferEntry => TransferGroupId is not null;
}
