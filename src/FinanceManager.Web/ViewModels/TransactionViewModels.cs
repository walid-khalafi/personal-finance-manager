using System.ComponentModel.DataAnnotations;
using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;

namespace FinanceManager.Web.ViewModels;

public class TransactionFormViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "حساب را انتخاب کنید.")]
    public int AccountId { get; set; }

    public TransactionType Type { get; set; } = TransactionType.Withdrawal;

    [Required(ErrorMessage = "مبلغ را وارد کنید.")]
    public string Amount { get; set; } = "";

    [Required(ErrorMessage = "تاریخ تراکنش را وارد کنید.")]
    public string Date { get; set; } = "";

    public int? CategoryId { get; set; }

    public int? CardId { get; set; }

    [Required(ErrorMessage = "شرح تراکنش را وارد کنید.")]
    [StringLength(400, ErrorMessage = "شرح تراکنش حداکثر ۴۰۰ نویسه است.")]
    public string Description { get; set; } = "";

    [StringLength(120, ErrorMessage = "شماره پیگیری حداکثر ۱۲۰ نویسه است.")]
    public string? ReferenceNumber { get; set; }
}

/// <summary>فهرست تراکنشها همراه فیلتر جاری و دادههای فرم فیلتر</summary>
public class TransactionListViewModel
{
    public required PagedResult<TransactionListItem> Page { get; init; }

    public required TransactionFilter Filter { get; init; }

    public required List<Account> Accounts { get; init; }

    public required List<CategoryListItem> Categories { get; init; }

    public string? FromText { get; init; }

    public string? ToText { get; init; }

    public string? FilterError { get; init; }

    /// <summary>آدرس پایه برای صفحهبندی (بدون پارامتر page)</summary>
    public string? PageBaseUrl { get; init; }
}
