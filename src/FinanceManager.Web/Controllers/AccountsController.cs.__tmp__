using FinanceManager.Core.Entities;
using FinanceManager.Core.Money;
using FinanceManager.Core.Text;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.Helpers;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

public class AccountsController(AccountService accounts, SettingsService settings) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await accounts.GetAccountsAsync(includeInactive: true, ct);

        var totals = list
            .Where(a => a.IsActive)
            .GroupBy(a => a.CurrencyCode, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CurrencyBalanceRow
            {
                CurrencyCode = g.Key,
                TotalMinor = g.Sum(a => a.BalanceMinor),
                AccountCount = g.Count(),
            })
            .OrderByDescending(t => t.TotalMinor)
            .ToList();

        return View(new AccountIndexViewModel { Accounts = list, TotalsByCurrency = totals });
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var defaultCurrency = await settings.GetDefaultCurrencyAsync(ct);
        return View(new AccountFormViewModel { CurrencyCode = defaultCurrency, OpeningBalance = "0" });
    }

    [HttpPost]
    public async Task<IActionResult> Create(AccountFormViewModel model, CancellationToken ct)
    {
        var input = BuildInput(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await accounts.CreateAsync(input!, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            return View(model);
        }

        TempData["Success"] = "حساب با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var account = await accounts.GetAsync(id, ct);
        if (account is null)
        {
            return NotFound();
        }

        return View(new AccountFormViewModel
        {
            Id = account.Id,
            BankName = account.BankName,
            Title = account.Title,
            Type = account.Type,
            AccountNumber = account.AccountNumber,
            Iban = account.Iban,
            CurrencyCode = account.CurrencyCode,
            OpeningBalance = Inputs.FormatMoneyForInput(account.OpeningBalanceMinor, account.CurrencyCode),
            Notes = account.Notes,
            IsActive = account.IsActive,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AccountFormViewModel model, CancellationToken ct)
    {
        var input = BuildInput(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await accounts.UpdateAsync(id, input!, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            return View(model);
        }

        TempData["Success"] = "حساب با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, bool active, CancellationToken ct)
    {
        var result = await accounts.SetActiveAsync(id, active, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok
            ? (active ? "حساب فعال شد." : "حساب غیرفعال شد.")
            : result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await accounts.DeleteAsync(id, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "حساب حذف شد." : result.Message;
        return RedirectToAction(nameof(Index));
    }

    private AccountInput? BuildInput(AccountFormViewModel model)
    {
        if (!Inputs.TryParseMoney(model.OpeningBalance, model.CurrencyCode, out var openingMinor, out var moneyError))
        {
            ModelState.AddModelError(nameof(model.OpeningBalance), moneyError!);
        }

        if (!ModelState.IsValid)
        {
            return null;
        }

        return new AccountInput
        {
            BankName = model.BankName.Trim(),
            Title = model.Title.Trim(),
            Type = model.Type,
            AccountNumber = model.AccountNumber,
            Iban = model.Iban,
            CurrencyCode = model.CurrencyCode,
            OpeningBalanceMinor = openingMinor,
            Notes = model.Notes,
            IsActive = model.IsActive,
        };
    }
}
