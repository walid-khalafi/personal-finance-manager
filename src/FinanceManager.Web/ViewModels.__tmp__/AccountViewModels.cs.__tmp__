using System.ComponentModel.DataAnnotations;
using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;

namespace FinanceManager.Web.ViewModels;

public class AccountFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نام بانک را وارد کنید.")]
    [StringLength(120, ErrorMessage = "نام بانک حداکثر ۱۲۰ نویسه است.")]
    public string BankName { get; set; } = "";

    [Required(ErrorMessage = "عنوان حساب را وارد کنید.")]
    [StringLength(120, ErrorMessage = "عنوان حساب حداکثر ۱۲۰ نویسه است.")]
    public string Title { get; set; } = "";

    public AccountType Type { get; set; } = AccountType.Checking;

    [StringLength(64, ErrorMessage = "شماره حساب حداکثر ۶۴ نویسه است.")]
    public string? AccountNumber { get; set; }

    [StringLength(64, ErrorMessage = "شماره شبا حداکثر ۶۴ نویسه است.")]
    public string? Iban { get; set; }

    [Required(ErrorMessage = "ارز را انتخاب کنید.")]
    public string CurrencyCode { get; set; } = "IRT";

    [Required(ErrorMessage = "موجودی اولیه را وارد کنید.")]
    public string OpeningBalance { get; set; } = "0";

    [StringLength(1000, ErrorMessage = "یادداشت حداکثر ۱۰۰۰ نویسه است.")]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}

public class AccountIndexViewModel
{
    public required List<AccountListItem> Accounts { get; init; }

    public required List<CurrencyBalanceRow> TotalsByCurrency { get; init; }
}
