using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Core.Time;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Tests;

public class TransferServiceTests
{
    private static readonly DateOnly Day = new(2024, 10, 1);

    [Fact]
    public async Task Create_RecordsTwoLinkedEntries_AndMovesBalances()
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var accounts = new AccountService(db.Context);
        var accountA = await Seed.AccountAsync(db.Context, title: "مبدأ", currency: "USD", opening: 1000_00);
        var accountB = await Seed.AccountAsync(db.Context, title: "مقصد", currency: "USD", opening: 0);

        var result = await service.CreateAsync(new TransferInput
        {
            FromAccountId = accountA,
            ToAccountId = accountB,
            AmountMinor = 250_00,
            TransactionDate = Day,
        });

        Assert.True(result.Ok, result.Message);
        var groupId = result.Value;

        var entries = db.Context.Transactions.Where(t => t.TransferGroupId == groupId).ToList();
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.Type == TransactionType.Withdrawal && e.AccountId == accountA && e.AmountMinor == 250_00);
        Assert.Contains(entries, e => e.Type == TransactionType.Deposit && e.AccountId == accountB && e.AmountMinor == 250_00);

        var balances = await accounts.GetAccountsAsync();
        Assert.Equal(750_00, balances.Single(a => a.Id == accountA).BalanceMinor);
        Assert.Equal(250_00, balances.Single(a => a.Id == accountB).BalanceMinor);
    }

    [Fact]
    public async Task Create_RejectsDifferentCurrencies()
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var usd = await Seed.AccountAsync(db.Context, title: "دلاری", currency: "USD");
        var irt = await Seed.AccountAsync(db.Context, title: "تومانی", currency: "IRT");

        var result = await service.CreateAsync(new TransferInput
        {
            FromAccountId = usd,
            ToAccountId = irt,
            AmountMinor = 100,
            TransactionDate = Day,
        });

        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.CurrencyMismatch, result.Code);
    }

    [Fact]
    public async Task Create_RejectsSameAccount()
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var account = await Seed.AccountAsync(db.Context);

        var result = await service.CreateAsync(new TransferInput
        {
            FromAccountId = account,
            ToAccountId = account,
            AmountMinor = 100,
            TransactionDate = Day,
        });

        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.SameAccountTransfer, result.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task Create_RejectsNonPositiveAmount(long amount)
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var a = await Seed.AccountAsync(db.Context, title: "الف");
        var b = await Seed.AccountAsync(db.Context, title: "ب");

        var result = await service.CreateAsync(new TransferInput
        {
            FromAccountId = a,
            ToAccountId = b,
            AmountMinor = amount,
            TransactionDate = Day,
        });

        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.AmountInvalid, result.Code);
    }

    [Fact]
    public async Task Create_RejectsInactiveAccounts_ButAllowsKeepingThemOnEdit()
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var accounts = new AccountService(db.Context);
        var first = await Seed.AccountAsync(db.Context, title: "فعال");
        var second = await Seed.AccountAsync(db.Context, title: "دوم");

        // انتقال معتبر بساز، سپس حساب مقصد را غیرفعال کن
        var created = await service.CreateAsync(new TransferInput
        {
            FromAccountId = first,
            ToAccountId = second,
            AmountMinor = 100,
            TransactionDate = Day,
        });
        Assert.True(created.Ok, created.Message);
        Assert.True((await accounts.SetActiveAsync(second, false)).Ok);

        // ویرایش با حسابهای بدون تغییر، حتی وقتی مقصد غیرفعال شده، مجاز است
        var edit = await service.UpdateAsync(created.Value, new TransferInput
        {
            FromAccountId = first,
            ToAccountId = second,
            AmountMinor = 200,
            TransactionDate = Day,
        });
        Assert.True(edit.Ok, edit.Message);

        // اما انتقال جدید به حساب غیرفعال مجاز نیست
        var third = await Seed.AccountAsync(db.Context, title: "سوم");
        var blocked = await service.CreateAsync(new TransferInput
        {
            FromAccountId = third,
            ToAccountId = second,
            AmountMinor = 100,
            TransactionDate = Day,
        });
        Assert.False(blocked.Ok);
        Assert.Equal(ServiceErrorCode.AccountInactive, blocked.Code);
    }

    [Fact]
    public async Task Update_UpdatesBothLegsAtomically()
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var a = await Seed.AccountAsync(db.Context, title: "الف");
        var b = await Seed.AccountAsync(db.Context, title: "ب");
        var groupId = await Seed.TransferAsync(db.Context, a, b, 100, Day);

        var result = await service.UpdateAsync(groupId, new TransferInput
        {
            FromAccountId = a,
            ToAccountId = b,
            AmountMinor = 350,
            TransactionDate = Day.AddDays(1),
            Description = "شرح جدید",
        });

        Assert.True(result.Ok, result.Message);

        var legs = db.Context.Transactions.Where(t => t.TransferGroupId == groupId).ToList();
        Assert.Equal(2, legs.Count);
        Assert.All(legs, leg => Assert.Equal(350, leg.AmountMinor));
        Assert.All(legs, leg => Assert.Equal(Day.AddDays(1), leg.TransactionDate));
        Assert.All(legs, leg => Assert.Equal("شرح جدید", leg.Description));
    }

    [Fact]
    public async Task Delete_RemovesBothLegs()
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var accounts = new AccountService(db.Context);
        var a = await Seed.AccountAsync(db.Context, title: "الف", opening: 500);
        var b = await Seed.AccountAsync(db.Context, title: "ب");
        var groupId = await Seed.TransferAsync(db.Context, a, b, 200, Day);

        var result = await service.DeleteAsync(groupId);
        Assert.True(result.Ok, result.Message);

        Assert.Empty(db.Context.Transactions.Where(t => t.TransferGroupId == groupId));
        var balances = await accounts.GetAccountsAsync();
        Assert.Equal(500, balances.Single(x => x.Id == a).BalanceMinor);
        Assert.Equal(0, balances.Single(x => x.Id == b).BalanceMinor);
    }

    [Fact]
    public async Task Get_ReturnsBothLinkedLegs()
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var a = await Seed.AccountAsync(db.Context, title: "الف");
        var b = await Seed.AccountAsync(db.Context, title: "ب");
        var groupId = await Seed.TransferAsync(db.Context, a, b, 300, Day);

        var detail = await service.GetAsync(groupId);

        Assert.NotNull(detail);
        Assert.Equal(a, detail!.From.AccountId);
        Assert.Equal(b, detail.To.AccountId);
        Assert.Equal(300, detail.From.AmountMinor);
        Assert.Equal(TransactionType.Withdrawal, detail.From.Type);
        Assert.Equal(TransactionType.Deposit, detail.To.Type);
    }

    [Fact]
    public async Task Search_ListsGroupedTransfers_WithAccountFilter()
    {
        using var db = TestDatabase.New();
        var service = new TransferService(db.Context);
        var a = await Seed.AccountAsync(db.Context, title: "الف");
        var b = await Seed.AccountAsync(db.Context, title: "ب");
        var c = await Seed.AccountAsync(db.Context, title: "ج");

        await Seed.TransferAsync(db.Context, a, b, 100, Day);
        await Seed.TransferAsync(db.Context, b, c, 200, Day.AddDays(1));
        await Seed.TransferAsync(db.Context, c, a, 300, Day.AddDays(2));

        var all = await service.SearchAsync(new TransferFilter());
        Assert.Equal(3, all.TotalCount);
        Assert.All(all.Items, item => Assert.Equal(2, db.Context.Transactions.Count(t => t.TransferGroupId == item.GroupId)));

        var forB = await service.SearchAsync(new TransferFilter { AccountId = b });
        Assert.Equal(2, forB.TotalCount);

        var page = await service.SearchAsync(new TransferFilter { PageSize = 5, Page = 1 });
        Assert.Equal(3, page.Items.Count);
    }

    [Fact]
    public async Task Transfers_ExcludedFromIncomeAndExpenseTotals()
    {
        using var db = TestDatabase.New();
        var reports = new ReportService(db.Context);
        var a = await Seed.AccountAsync(db.Context, title: "الف", currency: "USD", opening: 1000_00);
        var b = await Seed.AccountAsync(db.Context, title: "ب", currency: "USD");

        await Seed.TransactionAsync(db.Context, a, TransactionType.Deposit, 500_00, Day);
        await Seed.TransactionAsync(db.Context, a, TransactionType.Withdrawal, 200_00, Day);
        await Seed.TransferAsync(db.Context, a, b, 300_00, Day);

        var range = new DateRange(Day.AddDays(-5), Day.AddDays(5));
        var totals = await reports.GetPeriodTotalsAsync(range);
        var usd = totals.Single(t => t.CurrencyCode == "USD");

        // انتقال داخلی نه واریز شمرده میشود و نه برداشت
        Assert.Equal(500_00, usd.DepositsMinor);
        Assert.Equal(200_00, usd.WithdrawalsMinor);

        // و مجموع موجودیها تغییر نکرده: انتقال فقط بین حسابها جابهجا میکند
        var dashboardAccounts = await reports.GetAccountBalancesAsync();
        var total = dashboardAccounts.Sum(x => x.BalanceMinor);
        Assert.Equal(1000_00 + 500_00 - 200_00, total);
    }

    [Fact]
    public async Task AtomicWrite_FailedSecondLeg_RollsBackFirstLeg()
    {
        using var db = TestDatabase.New();
        var account = await Seed.AccountAsync(db.Context);
        var groupId = Guid.NewGuid();

        await using var trx = await db.Context.Database.BeginTransactionAsync();

        db.Context.Transactions.Add(new Transaction
        {
            AccountId = account,
            Type = TransactionType.Withdrawal,
            AmountMinor = 100,
            TransactionDate = Day,
            Description = "قلم اول",
            CreatedAtUtc = DateTime.UtcNow,
            TransferGroupId = groupId,
        });
        await db.Context.SaveChangesAsync();

        // قلم دوم نامعتبر است (مبلغ منفی → نقض محدودیت CHECK)
        db.Context.Transactions.Add(new Transaction
        {
            AccountId = account,
            Type = TransactionType.Deposit,
            AmountMinor = -100,
            TransactionDate = Day,
            Description = "قلم دوم نامعتبر",
            CreatedAtUtc = DateTime.UtcNow,
            TransferGroupId = groupId,
        });

        await Assert.ThrowsAnyAsync<Exception>(() => db.Context.SaveChangesAsync());
        await trx.RollbackAsync();

        Assert.Equal(0, db.Context.Transactions.Count());
    }
}
