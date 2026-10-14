using FinanceManager.Infrastructure.Services;

namespace FinanceManager.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task CreateInitialUser_ValidatesInput()
    {
        using var db = TestDatabase.New();
        var auth = new AuthService(db.Context);

        Assert.False((await auth.CreateInitialUserAsync("ab", "password-123")).Ok);
        Assert.False((await auth.CreateInitialUserAsync("valid-user", "short")).Ok);

        var created = await auth.CreateInitialUserAsync("valid-user", "password-123");
        Assert.True(created.Ok, created.Message);

        // کاربر دوم مجاز نیست (نصب تکنفره)
        var second = await auth.CreateInitialUserAsync("another", "password-456");
        Assert.False(second.Ok);
    }

    [Fact]
    public async Task HasAnyUser_ReflectsCreation()
    {
        using var db = TestDatabase.New();
        var auth = new AuthService(db.Context);

        Assert.False(await auth.HasAnyUserAsync());
        await auth.CreateInitialUserAsync("valid-user", "password-123");
        Assert.True(await auth.HasAnyUserAsync());
    }

    [Fact]
    public async Task ValidateCredentials_VerifiesHashes()
    {
        using var db = TestDatabase.New();
        var auth = new AuthService(db.Context);
        await auth.CreateInitialUserAsync("valid-user", "password-123");

        Assert.Null(await auth.ValidateCredentialsAsync("valid-user", "wrong-password"));
        Assert.Null(await auth.ValidateCredentialsAsync("missing-user", "password-123"));

        var user = await auth.ValidateCredentialsAsync("valid-user", "password-123");
        Assert.NotNull(user);
        Assert.Equal("valid-user", user!.Username);
        Assert.NotNull(user.LastLoginUtc);
    }

    [Fact]
    public async Task ChangePassword_RequiresCurrentAndUpdatesHash()
    {
        using var db = TestDatabase.New();
        var auth = new AuthService(db.Context);
        await auth.CreateInitialUserAsync("valid-user", "password-123");

        var wrong = await auth.ChangePasswordAsync("valid-user", "wrong", "new-password-1");
        Assert.False(wrong.Ok);

        var shortNew = await auth.ChangePasswordAsync("valid-user", "password-123", "short");
        Assert.False(shortNew.Ok);

        var ok = await auth.ChangePasswordAsync("valid-user", "password-123", "new-password-1");
        Assert.True(ok.Ok, ok.Message);

        Assert.Null(await auth.ValidateCredentialsAsync("valid-user", "password-123"));
        Assert.NotNull(await auth.ValidateCredentialsAsync("valid-user", "new-password-1"));
    }
}
