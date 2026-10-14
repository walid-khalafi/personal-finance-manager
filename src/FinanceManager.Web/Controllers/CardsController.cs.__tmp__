using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

public class CardsController(CardService cards, AccountService accounts) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await cards.GetCardsAsync(includeInactive: true, ct);
        return View(new CardIndexViewModel { Cards = list });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? accountId, CancellationToken ct)
    {
        ViewData["Accounts"] = await accounts.GetAllAsync(ct);
        return View(new CardFormViewModel { AccountId = accountId ?? 0 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CardFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Accounts"] = await accounts.GetAllAsync(ct);
            return View(model);
        }

        var result = await cards.CreateAsync(BuildInput(model), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            ViewData["Accounts"] = await accounts.GetAllAsync(ct);
            return View(model);
        }

        TempData["Success"] = "کارت با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var card = await cards.GetAsync(id, ct);
        if (card is null)
        {
            return NotFound();
        }

        ViewData["Accounts"] = await accounts.GetAllAsync(ct);
        return View(new CardFormViewModel
        {
            Id = card.Id,
            AccountId = card.AccountId,
            Nickname = card.Nickname,
            Last4 = card.Last4,
            ExpiryMonth = card.ExpiryMonth,
            ExpiryYear = card.ExpiryYear,
            Notes = card.Notes,
            IsActive = card.IsActive,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, CardFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Accounts"] = await accounts.GetAllAsync(ct);
            return View(model);
        }

        var result = await cards.UpdateAsync(id, BuildInput(model), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            ViewData["Accounts"] = await accounts.GetAllAsync(ct);
            return View(model);
        }

        TempData["Success"] = "کارت با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, bool active, CancellationToken ct)
    {
        var result = await cards.SetActiveAsync(id, active, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok
            ? (active ? "کارت فعال شد." : "کارت غیرفعال شد.")
            : result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await cards.DeleteAsync(id, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "کارت حذف شد." : result.Message;
        return RedirectToAction(nameof(Index));
    }

    private static CardInput BuildInput(CardFormViewModel model) => new()
    {
        AccountId = model.AccountId,
        Nickname = model.Nickname.Trim(),
        Last4 = model.Last4.Trim(),
        ExpiryMonth = model.ExpiryMonth,
        ExpiryYear = model.ExpiryYear,
        Notes = model.Notes,
        IsActive = model.IsActive,
    };
}
