using FinanceManager.Core.Money;
using FinanceManager.Infrastructure;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

public class SettingsController(
    SettingsService settings,
    AuthService auth,
    BackupService backups,
    AppPaths paths) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        return View(new SettingsViewModel
        {
            DefaultCurrency = await settings.GetDefaultCurrencyAsync(ct),
            Currencies = CurrencyCatalog.All.ToList(),
            Backups = backups.ListBackups(),
            DataDirectory = paths.DataDirectory,
            DatabasePath = paths.DatabasePath,
        });
    }

    [HttpPost]
    public async Task<IActionResult> SetDefaultCurrency(string currencyCode, CancellationToken ct)
    {
        var result = await settings.SetDefaultCurrencyAsync(currencyCode ?? "", ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "ارز پیشفرض ذخیره شد." : result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "فرم گذرواژه کامل یا معتبر نیست.";
            return RedirectToAction(nameof(Index));
        }

        var result = await auth.ChangePasswordAsync(
            User.Identity?.Name ?? "",
            model.CurrentPassword,
            model.NewPassword,
            ct);

        TempData[result.Ok ? "Success" : "Error"] = result.Ok ? "گذرواژه با موفقیت تغییر کرد." : result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CreateBackup(CancellationToken ct)
    {
        var result = await backups.CreateBackupAsync(ct);
        TempData[result.Ok ? "Success" : "Error"] = result.Ok
            ? $"نسخه پشتیبان ساخته شد: {Path.GetFileName(result.Value)}"
            : result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult DownloadBackup(string name)
    {
        var path = backups.ResolveBackupPath(name);
        if (path is null)
        {
            return NotFound();
        }

        return PhysicalFile(path, "application/octet-stream", Path.GetFileName(path));
    }

    [HttpPost]
    public IActionResult DeleteBackup(string name)
    {
        var path = backups.ResolveBackupPath(name);
        if (path is null)
        {
            TempData["Error"] = "فایل پشتیبان یافت نشد.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            System.IO.File.Delete(path);
            TempData["Success"] = "فایل پشتیبان حذف شد.";
        }
        catch (IOException ex)
        {
            TempData["Error"] = "حذف فایل ناموفق بود: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
