using System.ComponentModel.DataAnnotations;
using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;

namespace FinanceManager.Web.ViewModels;

public class TransferFormViewModel
{
    [Required(ErrorMessage = "شناسه انتقال نامعتبر است.")]
    public Guid GroupId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "حساب مبدأ را انتخاب کنید.")]
    public int FromAccountId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "حساب مقصد را انتخاب کنید.")]
    public int ToAccountId { get; set; }

    [Required(ErrorMessage = "مبلغ را وارد کنید.")]
    public string Amount { get; set; } = "";

    [Required(ErrorMessage = "تاریخ انتقال را وارد کنید.")]
    public string Date { get; set; } = "";

    [StringLength(400, ErrorMessage = "شرح حداکثر ۴۰۰ نویسه است.")]
    public string? Description { get; set; }

    /// <summary>شناسه قلمهای دفتر کل برای پیوند در فرم ویرایش</summary>
    public int? FromTransactionId { get; set; }

    public int? ToTransactionId { get; set; }
}

public class TransferListViewModel
{
    public required PagedResult<TransferListItem> Page { get; init; }

    public required TransferFilter Filter { get; init; }

    public required List<Account> Accounts { get; init; }

    public string? FromText { get; init; }

    public string? ToText { get; init; }

    public string? FilterError { get; init; }

    public string? PageBaseUrl { get; init; }
}
