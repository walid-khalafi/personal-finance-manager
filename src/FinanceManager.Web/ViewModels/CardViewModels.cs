using System.ComponentModel.DataAnnotations;
using FinanceManager.Infrastructure.Contracts;

namespace FinanceManager.Web.ViewModels;

public class CardFormViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "حساب را انتخاب کنید.")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "نام کارت را وارد کنید.")]
    [StringLength(120, ErrorMessage = "نام کارت حداکثر ۱۲۰ نویسه است.")]
    public string Nickname { get; set; } = "";

    [Required(ErrorMessage = "چهار رقم آخر کارت را وارد کنید.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "چهار رقم آخر باید دقیقاً ۴ رقم باشد.")]
    public string Last4 { get; set; } = "";

    [Range(1, 12, ErrorMessage = "ماه انقضا باید بین ۱ تا ۱۲ باشد.")]
    public int? ExpiryMonth { get; set; }

    [Range(0, 9999, ErrorMessage = "سال انقضا نامعتبر است.")]
    public int? ExpiryYear { get; set; }

    [StringLength(1000, ErrorMessage = "یادداشت حداکثر ۱۰۰۰ نویسه است.")]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CardIndexViewModel
{
    public required List<CardListItem> Cards { get; init; }
}
