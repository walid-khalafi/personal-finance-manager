using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Core.Text;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.Helpers;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

public class TransactionsController(
    TransactionService transactions,
    AccountService accounts,
    CategoryService categories,
    CardService cards) : Controller
{
    public async Task<IActionResult> Index(
        int? accountId = null,
        int? type = null,
        int? categoryId = null,
        string? from = null,
        string? to = null,
        string? q = null,
        int sort = (int)TransactionSort.DateDesc,
        int page = 1,
        CancellationToken ct = default)
    {
        var filter = ListQueries.BuildTransactionFilter(accountId, type, categoryId, from, to, q, sort, page, out var filterError);
        var paged = await transactions.SearchAsync(filter, ct);

        return View(new TransactionListViewModel
        {
            Page = paged,
            Filter = filter,
            Accounts = await accounts.GetAllAsync(ct),
            Categories = await categories.GetCategoriesAsync(includeInactive: true, ct),
            FromText = from,
            ToText = to,
            FilterError = filterError,
            PageBaseUrl = ListQueries.BuildPageBaseUrl(Request),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? accountId = null, int? type = null, CancellationToken ct = default)
    {
        await LoadSelectionsAsync(ct);
        return View(new TransactionFormViewModel
        {
            AccountId = accountId ?? 0,
            Type = type is int t && Enum.IsDefined(typeof(TransactionType), t) ? (TransactionType)t : TransactionType.Withdrawal,
            Date = PersianText.FormatDate(DateOnly.FromDateTime(DateTime.Now)),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(TransactionFormViewModel model, CancellationToken ct = default)
    {
        var input = await BuildInputAsync(model, ct);
        if (!ModelState.IsValid)
        {
            await LoadSelectionsAsync(ct);
            return View(model);
        }

        var result = await transactions.CreateAsync(input!, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            await LoadSelectionsAsync(ct);
            return View(model);
        }

        TempData["Success"] = "تراکنش با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var transaction = await transactions.GetAsync(id, ct);
        if (transaction is null)
        {
            return NotFound();
        }

        if (transaction.TransferGroupId is not null)
        {
            TempData["Error"] = "این تراکنش بخشی از یک انتقال است؛ برای ویرایش از بخش «انتقالها» استفاده کنید.";
            return RedirectToAction("Edit", "Transfers", new { id = transaction.TransferGroupId });
        }

        var account = await accounts.GetAsync(transaction.AccountId, ct);
        var model = new TransactionFormViewModel
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            Type = transaction.Type,
            Amount = Inputs.FormatMoneyForInput(transaction.AmountMinor, account?.CurrencyCode ?? "IRT"),
            Date = PersianText.FormatDate(transaction.TransactionDate),
            CategoryId = transaction.CategoryId,
            CardId = transaction.CardId,
            Description = transaction.Description,
            ReferenceNumber = transaction.ReferenceNumber,
        };

        await LoadSelectionsAsync(ct);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, TransactionFormViewModel model, CancellationToken ct = default)
    {
        var input = await BuildInputAsync(model, ct);
        if (!ModelState.IsValid)
        {
            await LoadSelectionsAsync(ct);
            return View(model);
        }

        var result = await transactions.UpdateAsync(id, input!, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            await LoadSelectionsAsync(ct);
            return View(model);
        }

        TempData["Success"] = "تراکنش با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var item = await transactions.GetListItemAsync(id, ct);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await transactions.DeleteAsync(id, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "تراکنش حذف شد." : result.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectionsAsync(CancellationToken ct)
    {
        ViewData["Accounts"] = await accounts.GetAllAsync(ct);
        ViewData["Categories"] = await categories.GetCategoriesAsync(includeInactive: true, ct);
        ViewData["Cards"] = await cards.GetForSelectionAsync(includeInactive: true, ct);
    }

    private async Task<TransactionInput?> BuildInputAsync(TransactionFormViewModel model, CancellationToken ct)
    {
        if (!Enum.IsDefined(model.Type))
        {
            ModelState.AddModelError(nameof(model.Type), "نوع تراکنش نامعتبر است.");
            return null;
        }

        var account = await accounts.GetAsync(model.AccountId, ct);
        if (account is null)
        {
            ModelState.AddModelError(nameof(model.AccountId), "حساب انتخابشده یافت نشد.");
            return null;
        }

        if (!Inputs.TryParseMoney(model.Amount, account.CurrencyCode, out var amountMinor, out var moneyError))
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

        return new TransactionInput
        {
            AccountId = model.AccountId,
            Type = model.Type,
            AmountMinor = amountMinor,
            TransactionDate = date,
            CategoryId = model.CategoryId,
            Description = model.Description.Trim(),
            ReferenceNumber = model.ReferenceNumber,
            CardId = model.CardId,
        };
    }
}
