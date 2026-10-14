using FinanceManager.Core.Common;
using FinanceManager.Core.Text;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.Helpers;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

public class TransfersController(TransferService transfers, AccountService accounts) : Controller
{
    public async Task<IActionResult> Index(
        int? accountId = null,
        string? from = null,
        string? to = null,
        int page = 1,
        CancellationToken ct = default)
    {
        var filter = new TransferFilter { AccountId = accountId, Page = page, PageSize = 20 };
        string? filterError = null;

        if (!string.IsNullOrWhiteSpace(from))
        {
            if (PersianText.TryParseDate(from, out var fromDate))
            {
                filter.From = fromDate;
            }
            else
            {
                filterError = "تاریخ «از» معتبر نیست؛ نمونه صحیح: ۱۴۰۳/۰۷/۲۲";
            }
        }

        if (!string.IsNullOrWhiteSpace(to))
        {
            if (PersianText.TryParseDate(to, out var toDate))
            {
                filter.To = toDate;
            }
            else if (filterError is null)
            {
                filterError = "تاریخ «تا» معتبر نیست؛ نمونه صحیح: ۱۴۰۳/۰۷/۲۲";
            }
        }

        var paged = await transfers.SearchAsync(filter, ct);

        return View(new TransferListViewModel
        {
            Page = paged,
            Filter = filter,
            Accounts = await accounts.GetAllAsync(ct),
            FromText = from,
            ToText = to,
            FilterError = filterError,
            PageBaseUrl = ListQueries.BuildPageBaseUrl(Request),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? fromAccountId = null, CancellationToken ct = default)
    {
        ViewData["Accounts"] = await accounts.GetAllAsync(ct);
        return View(new TransferFormViewModel
        {
            FromAccountId = fromAccountId ?? 0,
            Date = PersianText.FormatDate(DateOnly.FromDateTime(DateTime.Now)),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(TransferFormViewModel model, CancellationToken ct = default)
    {
        var input = await BuildInputAsync(model, ct);
        if (!ModelState.IsValid)
        {
            ViewData["Accounts"] = await accounts.GetAllAsync(ct);
            return View(model);
        }

        var result = await transfers.CreateAsync(input!, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            ViewData["Accounts"] = await accounts.GetAllAsync(ct);
            return View(model);
        }

        TempData["Success"] = "انتقال با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var detail = await transfers.GetAsync(id, ct);
        if (detail is null)
        {
            return NotFound();
        }

        ViewData["Accounts"] = await accounts.GetAllAsync(ct);

        var generatedDescription = $"انتقال به {detail.To.AccountTitle}";
        var description = string.Equals(detail.From.Description, generatedDescription, StringComparison.Ordinal)
            ? ""
            : detail.From.Description;

        return View(new TransferFormViewModel
        {
            GroupId = id,
            FromAccountId = detail.From.AccountId,
            ToAccountId = detail.To.AccountId,
            Amount = Inputs.FormatMoneyForInput(detail.From.AmountMinor, detail.From.CurrencyCode),
            Date = PersianText.FormatDate(detail.From.TransactionDate),
            Description = description,
            FromTransactionId = detail.From.TransactionId,
            ToTransactionId = detail.To.TransactionId,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TransferFormViewModel model, CancellationToken ct = default)
    {
        if (model.GroupId == Guid.Empty)
        {
            return NotFound();
        }

        var input = await BuildInputAsync(model, ct);
        if (!ModelState.IsValid)
        {
            ViewData["Accounts"] = await accounts.GetAllAsync(ct);
            return View(model);
        }

        var result = await transfers.UpdateAsync(model.GroupId, input!, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            ViewData["Accounts"] = await accounts.GetAllAsync(ct);
            return View(model);
        }

        TempData["Success"] = "انتقال با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var result = await transfers.DeleteAsync(id, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "انتقال حذف شد." : result.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task<TransferInput?> BuildInputAsync(TransferFormViewModel model, CancellationToken ct)
    {
        var fromAccount = await accounts.GetAsync(model.FromAccountId, ct);
        if (fromAccount is null)
        {
            ModelState.AddModelError(nameof(model.FromAccountId), "حساب مبدأ یافت نشد.");
            return null;
        }

        if (!Inputs.TryParseMoney(model.Amount, fromAccount.CurrencyCode, out var amountMinor, out var moneyError))
        {
            ModelState.AddModelError(nameof(model.Amount), moneyError!);
        }

        if (!Inputs.TryParseDate(model.Date, out var date, out var dateError))
        {
            ModelState.AddModelError(nameof(model.Date), dateError!);
        }

        if (!ModelState.IsValid)
        {
            return null;
        }

        return new TransferInput
        {
            FromAccountId = model.FromAccountId,
            ToAccountId = model.ToAccountId,
            AmountMinor = amountMinor,
            TransactionDate = date,
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
        };
    }
}
