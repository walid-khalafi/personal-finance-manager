using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;

namespace FinanceManager.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task Create_And_Read_Account()
    {
        using var db = TestDatabase.New();
        var service = new AccountService(db.Context);
        var id = await Seed.AccountAsync(db.Context, title: "حساب حقوق", currency: "IRT", opening: 50_000);

        var accounts = await service.GetAccountsAsync();
        var account = Assert.Single(accounts);
        Assert.Equal(id, account.Id);
        Assert.Equal("حساب حقوق", account.Title);
        Assert.Equal("IRT", account.CurrencyCode);
        Assert.Equal(50_000, account.BalanceMinor);
    }

    [Fact]
    public async Task Balance_FollowsLedgerRule_1000_500_200_1300()
    {
        using var db = TestDatabase.New();
        var accounts = new AccountService(db.Context);

        var usdId = await Seed.AccountAsync(db.Context, title: "دلاری", currency: "USD", opening: 1000_00);
        await Seed.TransactionAsync(db.Context, usdId, TransactionType.Deposit, 500_00, new DateOnly(2024, 10, 1));
        await Seed.TransactionAsync(db.Context, usdId, TransactionType.Withdrawal, 200_00, new DateOnly(2024, 10, 2));

        var account = (await accounts.GetAccountsAsync()).Single();
        Assert.Equal(1300_00, account.BalanceMinor);
    }

    [Fact]
    public async Task OpeningBalance_IsNotTreatedAsDeposit()
    {
        using var db = TestDatabase.New();
        var reports = new ReportService(db.Context);
        await Seed.AccountAsync(db.Context, currency: "USD", opening: 1000_00);

        var range = new Core.Time.DateRange(new DateOnly(2024, 10, 1), new DateOnly(2024, 10, 31));
        var totals = await reports.GetPeriodTotalsAsync(range);

        // موجودی اولیه در جمع واریزهای دوره نمیآید
        Assert.DoesNotContain(totals, t => t.CurrencyCode == "USD" && t.DepositsMinor > 0);
    }

    [Fact]
    public async Task Update_BlocksCurrencyChangeWhenTransactionsExist()
    {
        using var db = TestDatabase.New();
        var service = new AccountService(db.Context);
        var id = await Seed.AccountAsync(db.Context, currency: "USD");
        await Seed.TransactionAsync(db.Context, id, TransactionType.Deposit, 100, new DateOnly(2024, 10, 1));

        var result = await service.UpdateAsync(id, new AccountInput
        {
            BankName = "بانک تست",
            Title = "حساب اصلی",
            CurrencyCode = "IRT",
            OpeningBalanceMinor = 0,
        });

        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.Validation, result.Code);
    }

    [Fact]
    public async Task Update_AllowsCurrencyChangeWithoutTransactions()
    {
        using var db = TestDatabase.New();
        var service = new AccountService(db.Context);
        var id = await Seed.AccountAsync(db.Context, currency: "USD");

        var result = await service.UpdateAsync(id, new AccountInput
        {
            BankName = "بانک تست",
            Title = "حساب اصلی",
            CurrencyCode = "EUR",
            OpeningBalanceMinor = 0,
        });

        Assert.True(result.Ok, result.Message);
        var account = await service.GetAsync(id);
        Assert.Equal("EUR", account!.CurrencyCode);
    }

    [Fact]
    public async Task Delete_BlockedWhenTransactionsOrCardsExist()
    {
        using var db = TestDatabase.New();
        var service = new AccountService(db.Context);

        var withTx = await Seed.AccountAsync(db.Context, title: "با تراکنش");
        await Seed.TransactionAsync(db.Context, withTx, TransactionType.Deposit, 100, new DateOnly(2024, 10, 1));
        var result = await service.DeleteAsync(withTx);
        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.InUse, result.Code);

        var withCard = await Seed.AccountAsync(db.Context, title: "با کارت");
        await Seed.CardAsync(db.Context, withCard);
        var cardResult = await service.DeleteAsync(withCard);
        Assert.False(cardResult.Ok);
        Assert.Equal(ServiceErrorCode.InUse, cardResult.Code);

        var empty = await Seed.AccountAsync(db.Context, title: "خالی");
        var okResult = await service.DeleteAsync(empty);
        Assert.True(okResult.Ok, okResult.Message);
        Assert.Null(await service.GetAsync(empty));
    }

    [Fact]
    public async Task Create_RejectsUnknownCurrency()
    {
        using var db = TestDatabase.New();
        var service = new AccountService(db.Context);

        var result = await service.CreateAsync(new AccountInput
        {
            BankName = "بانک",
            Title = "حساب",
            CurrencyCode = "BITCOIN",
            OpeningBalanceMinor = 0,
        });

        Assert.False(result.Ok);
    }

    [Fact]
    public async Task SetActive_TogglesStatus()
    {
        using var db = TestDatabase.New();
        var service = new AccountService(db.Context);
        var id = await Seed.AccountAsync(db.Context);

        Assert.True((await service.SetActiveAsync(id, false)).Ok);
        Assert.False((await service.GetAsync(id))!.IsActive);
        Assert.True((await service.SetActiveAsync(id, true)).Ok);
        Assert.True((await service.GetAsync(id))!.IsActive);
    }
}
