using FinanceManager.Core.Time;
using FinanceManager.Infrastructure.Services;
using FinanceManager.Web.Helpers;
using FinanceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Controllers;

public class DashboardController(ReportService reports, SettingsService settings) : Controller
{
    public async Task<IActionResult> Index(
        int period = (int)PeriodKind.ThisMonth,
        string? from = null,
        string? to = null,
        string? currency = null,
        CancellationToken ct = default)
    {
        var selection = PeriodSelection.Resolve(period, from, to);

        var effectiveCurrency = currency;
        if (string.IsNullOrWhiteSpace(effectiveCurrency))
        {
            effectiveCurrency = await settings.GetDefaultCurrencyAsync(ct);
        }

        var data = await reports.GetDashboardAsync(selection.Range, effectiveCurrency, ct: ct);

        return View(new DashboardViewModel
        {
            Data = data,
            Selection = selection,
        });
    }
}
