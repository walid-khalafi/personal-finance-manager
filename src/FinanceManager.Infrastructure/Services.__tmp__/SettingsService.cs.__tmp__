using FinanceManager.Core.Entities;
using FinanceManager.Core.Money;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

public class SettingsService(AppDbContext db)
{
    public const string DefaultCurrencyKey = "DefaultCurrency";
    public const string FallbackDefaultCurrency = "IRT";

    public async Task<string> GetDefaultCurrencyAsync(CancellationToken ct = default)
    {
        var value = await db.Settings.AsNoTracking()
            .Where(s => s.Key == DefaultCurrencyKey)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct);

        return CurrencyCatalog.Contains(value) ? value! : FallbackDefaultCurrency;
    }

    public async Task<ServiceResult> SetDefaultCurrencyAsync(string currencyCode, CancellationToken ct = default)
    {
        if (!CurrencyCatalog.Contains(currencyCode))
        {
            return ServiceResult.Fail(ServiceErrorCode.Validation, "ارز انتخابشده پشتیبانی نمیشود.");
        }

        var code = currencyCode.Trim().ToUpperInvariant();
        var setting = await db.Settings.FirstOrDefaultAsync(s => s.Key == DefaultCurrencyKey, ct);
        if (setting is null)
        {
            db.Settings.Add(new AppSetting { Key = DefaultCurrencyKey, Value = code });
        }
        else
        {
            setting.Value = code;
        }

        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}
