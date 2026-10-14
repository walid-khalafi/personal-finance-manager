using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using FinanceManager.Infrastructure.Services;

namespace FinanceManager.Tests;

/// <summary>دادهسازی کمکی برای تستها</summary>
internal static class Seed
{
    public static async Task<int> AccountAsync(
        AppDbContext db,
        string title = "حساب اصلی",
        string currency = "USD",
        long opening = 0,
        bool active = true)
    {
        var service = new AccountService(db);
        var result = await service.CreateAsync(new AccountInput
        {
            BankName = "بانک تست",
            Title = title,
            Type = AccountType.Checking,
            CurrencyCode = currency,
            OpeningBalanceMinor = opening,
            IsActive = active,
        });

        Assert.True(result.Ok, result.Message);
        return result.Value;
    }

    public static async Task<int> TransactionAsync(
        AppDbContext db,
        int accountId,
        TransactionType type,
        long amountMinor,
        DateOnly date,
        string description = "تراکنش تست",
        int? categoryId = null,
        int? cardId = null)
    {
        var service = new TransactionService(db);
        var result = await service.CreateAsync(new TransactionInput
        {
            AccountId = accountId,
            Type = type,
            AmountMinor = amountMinor,
            TransactionDate = date,
            Description = description,
            CategoryId = categoryId,
            CardId = cardId,
        });

        Assert.True(result.Ok, result.Message);
        return result.Value;
    }

    public static async Task<int> CardAsync(AppDbContext db, int accountId, string last4 = "1234")
    {
        var service = new CardService(db);
        var result = await service.CreateAsync(new CardInput
        {
            AccountId = accountId,
            Nickname = "کارت تست",
            Last4 = last4,
        });

        Assert.True(result.Ok, result.Message);
        return result.Value;
    }

    public static async Task<int> CategoryAsync(AppDbContext db, string name, CategoryKind kind = CategoryKind.Both)
    {
        var service = new CategoryService(db);
        var result = await service.CreateAsync(new CategoryInput
        {
            Name = name,
            Kind = kind,
            SortOrder = 500,
        });

        Assert.True(result.Ok, result.Message);
        return result.Value;
    }

    public static async Task<Guid> TransferAsync(
        AppDbContext db,
        int fromAccountId,
        int toAccountId,
        long amountMinor,
        DateOnly date,
        string? description = null)
    {
        var service = new TransferService(db);
        var result = await service.CreateAsync(new TransferInput
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            AmountMinor = amountMinor,
            TransactionDate = date,
            Description = description,
        });

        Assert.True(result.Ok, result.Message);
        return result.Value;
    }
}
