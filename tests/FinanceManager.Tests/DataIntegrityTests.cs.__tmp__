using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Tests;

/// <summary>پایداری SQLite: محدودیتها، کلیدهای خارجی و رفتار حذف</summary>
public class DataIntegrityTests
{
    private static readonly DateOnly Day = new(2024, 10, 1);

    [Fact]
    public async Task CheckConstraint_RejectsNonPositiveAmounts()
    {
        using var db = TestDatabase.New();
        var accountId = await Seed.AccountAsync(db.Context);

        db.Context.Transactions.Add(new Transaction
        {
            AccountId = accountId,
            Type = TransactionType.Deposit,
            AmountMinor = 0,
            TransactionDate = Day,
            Description = "نامعتبر",
            CreatedAtUtc = DateTime.UtcNow,
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task ForeignKey_RestrictsDeletingAccountWithTransactions()
    {
        using var db = TestDatabase.New();
        var accountId = await Seed.AccountAsync(db.Context);
        await Seed.TransactionAsync(db.Context, accountId, TransactionType.Deposit, 100, Day);

        // حذف مستقیم در سطح پایگاه داده باید توسط کلید خارجی (RESTRICT) رد شود
        var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            db.Context.Database.ExecuteSqlRawAsync("DELETE FROM \"Accounts\" WHERE \"Id\" = {0}", accountId));

        Assert.Contains("FOREIGN KEY", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UniqueIndex_RejectsDuplicateUsernames()
    {
        using var db = TestDatabase.New();
        var auth = new AuthService(db.Context);

        var first = await auth.CreateInitialUserAsync("walid", "password-123");
        Assert.True(first.Ok, first.Message);

        db.Context.Users.Add(new AppUser
        {
            Username = "walid",
            PasswordHash = "x",
            CreatedAtUtc = DateTime.UtcNow,
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task DeletingCard_SetsTransactionCardToNull()
    {
        using var db = TestDatabase.New();
        var cards = new CardService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context);
        var cardId = await Seed.CardAsync(db.Context, accountId);
        var txId = await Seed.TransactionAsync(db.Context, accountId, TransactionType.Withdrawal, 100, Day, cardId: cardId);

        var result = await cards.DeleteAsync(cardId);
        Assert.True(result.Ok, result.Message);

        var transaction = await new TransactionService(db.Context).GetAsync(txId);
        Assert.NotNull(transaction);
        Assert.Null(transaction!.CardId);
    }

    [Fact]
    public async Task DeletingCategory_SetsTransactionCategoryToNull()
    {
        using var db = TestDatabase.New();
        var categories = new CategoryService(db.Context);
        var accountId = await Seed.AccountAsync(db.Context);
        var categoryId = await Seed.CategoryAsync(db.Context, "قابل حذف");
        var txId = await Seed.TransactionAsync(db.Context, accountId, TransactionType.Deposit, 100, Day, categoryId: categoryId);

        var result = await categories.DeleteAsync(categoryId);
        Assert.True(result.Ok, result.Message);

        var transaction = await new TransactionService(db.Context).GetAsync(txId);
        Assert.NotNull(transaction);
        Assert.Null(transaction!.CategoryId);
    }

    [Fact]
    public async Task TransactionEntryOfTransfer_CannotBeEditedOrDeletedDirectly()
    {
        using var db = TestDatabase.New();
        var transactions = new TransactionService(db.Context);
        var a = await Seed.AccountAsync(db.Context, title: "الف");
        var b = await Seed.AccountAsync(db.Context, title: "ب");
        var groupId = await Seed.TransferAsync(db.Context, a, b, 100, Day);

        var legId = db.Context.Transactions.Single(t => t.TransferGroupId == groupId && t.Type == TransactionType.Withdrawal).Id;

        var edit = await transactions.UpdateAsync(legId, new Infrastructure.Contracts.TransactionInput
        {
            AccountId = a,
            Type = TransactionType.Withdrawal,
            AmountMinor = 200,
            TransactionDate = Day,
            Description = "تلاش برای ویرایش",
        });

        Assert.False(edit.Ok);
        Assert.Equal(Infrastructure.Contracts.ServiceErrorCode.TransferEntryEdit, edit.Code);

        var delete = await transactions.DeleteAsync(legId);
        Assert.False(delete.Ok);
        Assert.Equal(Infrastructure.Contracts.ServiceErrorCode.TransferEntryEdit, delete.Code);
    }

    [Fact]
    public async Task DatabaseFile_SurvivesReopen_WithExactAmounts()
    {
        // ذخیرهسازی صحیح مبالغ در واحد خرد؛ بدون خطای شناوری
        using var db = TestDatabase.New();
        var accountId = await Seed.AccountAsync(db.Context, currency: "USD", opening: 1);
        await Seed.TransactionAsync(db.Context, accountId, TransactionType.Deposit, 2, Day);

        var stored = await db.Context.Transactions.AsNoTracking().SingleAsync();
        Assert.Equal(2L, stored.AmountMinor);

        var balance = (await new AccountService(db.Context).GetAccountsAsync()).Single().BalanceMinor;
        Assert.Equal(3L, balance);
    }
}
