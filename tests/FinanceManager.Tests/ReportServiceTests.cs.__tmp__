using FinanceManager.Core.Entities;
using FinanceManager.Core.Time;
using FinanceManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Tests;

/// <summary>
/// داده مشترک: دو حساب دلاری (الف با موجودی اولیه ۱۰۰۰ و ب) و یک حساب تومانی (ج با ۵۰۰٬۰۰۰)
/// </summary>
public class ReportServiceTests
{
    private static readonly DateOnly MehrStart = new(2024, 9, 22);
    private static readonly DateOnly MehrEnd = new(2024, 10, 21);
    private static readonly DateOnly Today = new(2024, 10, 13);

    private sealed class Fixture : IDisposable
    {
        public TestDatabase Db { get; } = TestDatabase.New();
        public int AccountA { get; private set; }
        public int AccountB { get; private set; }
        public int AccountC { get; private set; }

        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            var db = fixture.Db.Context;

            fixture.AccountA = await Seed.AccountAsync(db, title: "الف", currency: "USD", opening: 1000_00);
            fixture.AccountB = await Seed.AccountAsync(db, title: "ب", currency: "USD", opening: 0);
            fixture.AccountC = await Seed.AccountAsync(db, title: "ج", currency: "IRT", opening: 500_000);

            var food = db.Categories.AsNoTracking().Single(c => c.Name == "غذا").Id;

            // مهر ۱۴۰۳ (در بازه): واریز ۵۰۰، برداشت ۲۰۰ (دسته غذا)، برداشت ۵۰ (بدون دسته)
            await Seed.TransactionAsync(db, fixture.AccountA, TransactionType.Deposit, 500_00, new DateOnly(2024, 9, 25), "حقوق مهر");
            await Seed.TransactionAsync(db, fixture.AccountA, TransactionType.Withdrawal, 200_00, new DateOnly(2024, 10, 5), "خرید غذا", categoryId: food);
            await Seed.TransactionAsync(db, fixture.AccountA, TransactionType.Withdrawal, 50_00, new DateOnly(2024, 10, 6), "برداشت بدون دسته");

            // شهریور ۱۴۰۳ (پیش از بازه): واریز ۳۰۰
            await Seed.TransactionAsync(db, fixture.AccountA, TransactionType.Deposit, 300_00, new DateOnly(2024, 8, 25), "واریز شهریور");

            // انتقال ۱۵۰ از الف به ب در بازه
            await Seed.TransferAsync(db, fixture.AccountA, fixture.AccountB, 150_00, new DateOnly(2024, 10, 1));

            // حساب تومانی: واریز ۱۰۰٬۰۰۰ در بازه
            await Seed.TransactionAsync(db, fixture.AccountC, TransactionType.Deposit, 100_000, new DateOnly(2024, 10, 1), "واریز تومانی");

            return fixture;
        }

        public void Dispose() => Db.Dispose();
    }

    [Fact]
    public async Task PeriodTotals_ExcludeTransfers_AndKeepCurrenciesSeparate()
    {
        using var fixture = await Fixture.CreateAsync();
        var reports = new ReportService(fixture.Db.Context);

        var totals = await reports.GetPeriodTotalsAsync(new DateRange(MehrStart, MehrEnd));

        var usd = totals.Single(t => t.CurrencyCode == "USD");
        Assert.Equal(500_00, usd.DepositsMinor);
        Assert.Equal(250_00, usd.WithdrawalsMinor);
        Assert.Equal(250_00, usd.NetMinor);

        var irt = totals.Single(t => t.CurrencyCode == "IRT");
        Assert.Equal(100_000, irt.DepositsMinor);
        Assert.Equal(0, irt.WithdrawalsMinor);
    }

    [Fact]
    public async Task MonthlySeries_BucketsByPersianMonth()
    {
        using var fixture = await Fixture.CreateAsync();
        var reports = new ReportService(fixture.Db.Context);

        var points = await reports.GetMonthlySeriesAsync(new DateRange(MehrStart, MehrEnd), "USD");

        var mehr = Assert.Single(points);
        Assert.Equal((1403, 7), (mehr.Year, mehr.Month));
        Assert.Equal(500_00, mehr.DepositsMinor);
        Assert.Equal(250_00, mehr.WithdrawalsMinor);
    }

    [Fact]
    public async Task CategoryDistribution_SumsWithdrawals_WithShares()
    {
        using var fixture = await Fixture.CreateAsync();
        var reports = new ReportService(fixture.Db.Context);

        var report = await reports.GetCategoryReportAsync(new DateRange(MehrStart, MehrEnd), "USD", TransactionType.Withdrawal);

        Assert.Equal(250_00, report.TotalMinor);
        Assert.Equal(2, report.TotalCount);

        var food = report.Rows.Single(r => r.Name == "غذا");
        Assert.Equal(200_00, food.TotalMinor);
        Assert.Equal(0.8m, food.Share);

        var uncategorized = report.Rows.Single(r => r.CategoryId is null);
        Assert.Equal("بدون دستهبندی", uncategorized.Name);
        Assert.Equal(50_00, uncategorized.TotalMinor);
    }

    [Fact]
    public async Task Summary_Reconciles_OpeningPlusFlowsEqualsClosing()
    {
        using var fixture = await Fixture.CreateAsync();
        var reports = new ReportService(fixture.Db.Context);

        var rows = await reports.GetSummaryAsync(new DateRange(MehrStart, MehrEnd));

        var usd = rows.Single(r => r.CurrencyCode == "USD");
        // آغاز = موجودیهای اولیه (۱۰۰۰) + خالص دفتر پیش از مهر (۳۰۰) = ۱۳۰۰
        Assert.Equal(1300_00, usd.OpeningMinor);
        Assert.Equal(500_00, usd.DepositsMinor);
        Assert.Equal(250_00, usd.WithdrawalsMinor);
        Assert.Equal(1550_00, usd.ClosingMinor);
        Assert.Equal(usd.OpeningMinor + usd.DepositsMinor - usd.WithdrawalsMinor, usd.ClosingMinor);

        var irt = rows.Single(r => r.CurrencyCode == "IRT");
        Assert.Equal(500_000, irt.OpeningMinor);
        Assert.Equal(600_000, irt.ClosingMinor);

        // تطبیق با موجودی واقعی حسابها در پایان دوره
        var balances = await reports.GetAccountBalancesAsync();
        Assert.Equal(balances.Where(b => b.CurrencyCode == "USD").Sum(b => b.BalanceMinor), usd.ClosingMinor);
    }

    [Fact]
    public async Task AccountReport_ColumnsReconcile_AndShowTransfers()
    {
        using var fixture = await Fixture.CreateAsync();
        var reports = new ReportService(fixture.Db.Context);

        var rows = await reports.GetAccountReportAsync(new DateRange(MehrStart, MehrEnd), "USD");

        var a = rows.Single(r => r.AccountId == fixture.AccountA);
        Assert.Equal(1300_00, a.OpeningMinor);   // ۱۰۰۰ + ۳۰۰ شهریور
        Assert.Equal(500_00, a.DepositsMinor);
        Assert.Equal(250_00, a.WithdrawalsMinor);
        Assert.Equal(0, a.TransfersInMinor);
        Assert.Equal(150_00, a.TransfersOutMinor);
        Assert.Equal(100_00, a.NetMinor);        // ۵۰۰ − ۲۵۰ − ۱۵۰
        Assert.Equal(1400_00, a.ClosingMinor);   // ۱۳۰۰ + ۱۰۰

        var b = rows.Single(r => r.AccountId == fixture.AccountB);
        Assert.Equal(0, b.OpeningMinor);
        Assert.Equal(150_00, b.TransfersInMinor);
        Assert.Equal(150_00, b.NetMinor);
        Assert.Equal(150_00, b.ClosingMinor);

        // تطبیق با دفتر کل: هر قلم کل دوره (پیش و داخل) بازیابی شود
        var expectedA = 1000_00 + 300_00 + 500_00 - 200_00 - 50_00 - 150_00;
        Assert.Equal(expectedA, a.ClosingMinor);
    }

    [Fact]
    public async Task MultiCurrency_IsNeverAggregatedTogether()
    {
        using var fixture = await Fixture.CreateAsync();
        var reports = new ReportService(fixture.Db.Context);

        var rows = await reports.GetSummaryAsync(new DateRange(MehrStart, MehrEnd));
        Assert.Equal(2, rows.Count);
        Assert.Equal(["IRT", "USD"], rows.Select(r => r.CurrencyCode).ToArray());

        var balances = await reports.GetAccountBalancesAsync();
        var byCurrency = balances.GroupBy(b => b.CurrencyCode).ToList();
        Assert.Equal(2, byCurrency.Count);
        Assert.All(byCurrency, group => Assert.Single(group.Select(x => x.CurrencyCode).Distinct()));
    }

    [Fact]
    public async Task Dashboard_ResolvesChartCurrency_AndBuildsSeries()
    {
        using var fixture = await Fixture.CreateAsync();
        var reports = new ReportService(fixture.Db.Context);

        var data = await reports.GetDashboardAsync(new DateRange(MehrStart, MehrEnd), "USD", today: Today);

        Assert.Equal("USD", data.ChartCurrencyCode);
        Assert.Equal(["IRT", "USD"], data.AvailableCurrencies.ToArray());

        Assert.Equal(2, data.BalancesByCurrency.Count);
        Assert.Equal(600_000, data.BalancesByCurrency.Single(b => b.CurrencyCode == "IRT").TotalMinor);

        // سری ششماهه منتهی به مهر ۱۴۰۳
        Assert.Equal(6, data.MonthlySeries.Count);
        var mehr = data.MonthlySeries.Single(p => p.Month == 7 && p.Year == 1403);
        Assert.Equal(500_00, mehr.DepositsMinor);
        var shahrivar = data.MonthlySeries.Single(p => p.Month == 6 && p.Year == 1403);
        Assert.Equal(300_00, shahrivar.DepositsMinor);

        // توزیع دستهبندیها = برداشتهای دوره
        Assert.Equal(250_00, data.CategoryDistribution.Sum(c => c.TotalMinor));

        // تراکنشهای اخیر شامل قلمهای انتقال هم میشود
        Assert.Equal(7, data.RecentTransactions.Count);
    }

    [Fact]
    public async Task Dashboard_FallsBackToFirstCurrency_WhenRequestedMissing()
    {
        using var fixture = await Fixture.CreateAsync();
        var reports = new ReportService(fixture.Db.Context);

        var data = await reports.GetDashboardAsync(new DateRange(MehrStart, MehrEnd), "EUR", today: Today);
        Assert.Equal("IRT", data.ChartCurrencyCode);
    }
}
