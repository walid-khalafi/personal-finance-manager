using FinanceManager.Core.Common;
using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Services;

namespace FinanceManager.Tests;

public class TransactionServiceTests
{
    private static readonly DateOnly Day = new(2024, 10, 1);

    [Fact]
    public async Task Create_DepositAndWithdrawal_UpdatesBalance()
    {
        using var db = TestDatabase.New();
        var service = new TransactionService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context, currency: "USD", opening: 0);

        var deposit = await service.CreateAsync(Input(accountId, TransactionType.Deposit, 500_00));
        var withdrawal = await service.CreateAsync(Input(accountId, TransactionType.Withdrawal, 200_00));

        Assert.True(deposit.Ok);
        Assert.True(withdrawal.Ok);

        var account = (await new AccountService(db.Context).GetAccountsAsync()).Single();
        Assert.Equal(300_00, account.BalanceMinor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Create_RejectsNonPositiveAmounts(long amount)
    {
        using var db = TestDatabase.New();
        var service = new TransactionService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context);

        var result = await service.CreateAsync(Input(accountId, TransactionType.Deposit, amount));

        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.AmountInvalid, result.Code);
    }

    [Fact]
    public async Task Create_RejectsCardFromAnotherAccount()
    {
        using var db = TestDatabase.New();
        var service = new TransactionService(db.Context);
        var accountA = await Seed.AccountAsync(db.Context, title: "الف");
        var accountB = await Seed.AccountAsync(db.Context, title: "ب");
        var cardOfA = await Seed.CardAsync(db.Context, accountA);

        var result = await service.CreateAsync(new TransactionInput
        {
            AccountId = accountB,
            Type = TransactionType.Withdrawal,
            AmountMinor = 100,
            TransactionDate = Day,
            Description = "تست",
            CardId = cardOfA,
        });

        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.CardAccountMismatch, result.Code);
    }

    [Fact]
    public async Task Create_EnforcesCategoryKindCompatibility()
    {
        using var db = TestDatabase.New();
        var service = new TransactionService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context);
        var depositOnly = await Seed.CategoryAsync(db.Context, "فقط واریز", CategoryKind.Deposit);

        var result = await service.CreateAsync(new TransactionInput
        {
            AccountId = accountId,
            Type = TransactionType.Withdrawal,
            AmountMinor = 100,
            TransactionDate = Day,
            Description = "تست",
            CategoryId = depositOnly,
        });

        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.Validation, result.Code);

        var okResult = await service.CreateAsync(new TransactionInput
        {
            AccountId = accountId,
            Type = TransactionType.Deposit,
            AmountMinor = 100,
            TransactionDate = Day,
            Description = "تست",
            CategoryId = depositOnly,
        });
        Assert.True(okResult.Ok, okResult.Message);
    }

    [Fact]
    public async Task Create_BlockedForInactiveAccount()
    {
        using var db = TestDatabase.New();
        var service = new TransactionService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context, active: false);

        var result = await service.CreateAsync(Input(accountId, TransactionType.Deposit, 100));

        Assert.False(result.Ok);
        Assert.Equal(ServiceErrorCode.AccountInactive, result.Code);
    }

    [Fact]
    public async Task Edit_And_Delete_KeepLedgerConsistent()
    {
        using var db = TestDatabase.New();
        var service = new TransactionService(db.Context);
        var accounts = new AccountService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context, opening: 1000);
        var txId = await Seed.TransactionAsync(db.Context, accountId, TransactionType.Deposit, 600, Day);

        Assert.Equal(1600, (await accounts.GetAccountsAsync()).Single().BalanceMinor);

        var edit = await service.UpdateAsync(txId, new TransactionInput
        {
            AccountId = accountId,
            Type = TransactionType.Deposit,
            AmountMinor = 700,
            TransactionDate = Day,
            Description = "ویرایششده",
        });
        Assert.True(edit.Ok, edit.Message);
        Assert.Equal(1700, (await accounts.GetAccountsAsync()).Single().BalanceMinor);

        var delete = await service.DeleteAsync(txId);
        Assert.True(delete.Ok, delete.Message);
        Assert.Equal(1000, (await accounts.GetAccountsAsync()).Single().BalanceMinor);
    }

    [Fact]
    public async Task Search_FiltersAndPaginatesServerSide()
    {
        using var db = TestDatabase.New();
        var service = new TransactionService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context);
        var categoryId = await Seed.CategoryAsync(db.Context, "دسته تست", CategoryKind.Both);

        for (var i = 1; i <= 25; i++)
        {
            await Seed.TransactionAsync(
                db.Context,
                accountId,
                i % 2 == 0 ? TransactionType.Deposit : TransactionType.Withdrawal,
                100 * i,
                new DateOnly(2024, 10, 1).AddDays(i),
                description: $"تراکنش شماره {i}",
                categoryId: categoryId);
        }

        var page1 = await service.SearchAsync(new TransactionFilter { Page = 1, PageSize = 20 });
        Assert.Equal(25, page1.TotalCount);
        Assert.Equal(20, page1.Items.Count);
        Assert.Equal(2, page1.TotalPages);
        Assert.True(page1.HasNext);

        var page2 = await service.SearchAsync(new TransactionFilter { Page = 2, PageSize = 20 });
        Assert.Equal(5, page2.Items.Count);
        Assert.False(page2.HasNext);

        var depositsOnly = await service.SearchAsync(new TransactionFilter { Type = TransactionType.Deposit });
        Assert.All(depositsOnly.Items, item => Assert.Equal(TransactionType.Deposit, item.Type));
        Assert.Equal(12, depositsOnly.TotalCount);

        var search = await service.SearchAsync(new TransactionFilter { Search = "شماره 1" });
        Assert.True(search.TotalCount >= 1);
        Assert.All(search.Items, item => Assert.Contains("شماره 1", item.Description));

        var byCategory = await service.SearchAsync(new TransactionFilter { CategoryId = categoryId });
        Assert.Equal(25, byCategory.TotalCount);

        var dateRange = await service.SearchAsync(new TransactionFilter
        {
            From = new DateOnly(2024, 10, 10),
            To = new DateOnly(2024, 10, 15),
        });
        Assert.Equal(6, dateRange.TotalCount);
    }

    [Fact]
    public async Task Search_SortByAmountDescending()
    {
        using var db = TestDatabase.New();
        var service = new TransactionService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context);

        await Seed.TransactionAsync(db.Context, accountId, TransactionType.Deposit, 100, Day, "کم");
        await Seed.TransactionAsync(db.Context, accountId, TransactionType.Deposit, 900, Day, "زیاد");
        await Seed.TransactionAsync(db.Context, accountId, TransactionType.Deposit, 500, Day, "متوسط");

        var result = await service.SearchAsync(new TransactionFilter { Sort = TransactionSort.AmountDesc });
        Assert.Equal(["زیاد", "متوسط", "کم"], result.Items.Select(i => i.Description).ToArray());
    }

    private static TransactionInput Input(int accountId, TransactionType type, long amountMinor) => new()
    {
        AccountId = accountId,
        Type = type,
        AmountMinor = amountMinor,
        TransactionDate = Day,
        Description = "تست",
    };
}
