namespace FinanceManager.Core.Money;

/// <summary>
/// قاعده واحد و مرجع برای محاسبه موجودی حساب از روی دفتر کل:
/// موجودی = موجودی اولیه + جمع واریزها − جمع برداشتها (همه به واحد خرد یک ارز).
/// هیچ موجودی ذخیرهشدهٔ جداگانهای نگهداری نمیشود.
/// </summary>
public static class LedgerMath
{
    public static long ComputeBalance(long openingBalanceMinor, long depositsMinor, long withdrawalsMinor)
        => checked(openingBalanceMinor + depositsMinor - withdrawalsMinor);
}
