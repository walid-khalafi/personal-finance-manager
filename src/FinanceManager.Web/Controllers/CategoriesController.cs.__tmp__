using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

public class CategoriesController(CategoryService categories) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await categories.GetCategoriesAsync(includeInactive: true, ct);
        return View(new CategoryIndexViewModel { Categories = list });
    }

    [HttpGet]
    public IActionResult Create() => View(new CategoryFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await categories.CreateAsync(BuildInput(model), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            return View(model);
        }

        TempData["Success"] = "دستهبندی با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var category = await categories.GetAsync(id, ct);
        if (category is null)
        {
            return NotFound();
        }

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Kind = category.Kind,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await categories.UpdateAsync(id, BuildInput(model), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            return View(model);
        }

        TempData["Success"] = "دستهبندی با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, bool active, CancellationToken ct)
    {
        var result = await categories.SetActiveAsync(id, active, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok
            ? (active ? "دستهبندی فعال شد." : "دستهبندی غیرفعال شد.")
            : result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await categories.DeleteAsync(id, ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok
            ? "دستهبندی حذف شد؛ تراکنشهای مرتبط بدون دستهبندی شدند."
            : result.Message;
        return RedirectToAction(nameof(Index));
    }

    private static CategoryInput BuildInput(CategoryFormViewModel model) => new()
    {
        Name = model.Name.Trim(),
        Kind = model.Kind,
        SortOrder = model.SortOrder,
        IsActive = model.IsActive,
    };
}
