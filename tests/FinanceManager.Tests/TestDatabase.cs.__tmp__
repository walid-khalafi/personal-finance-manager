using FinanceManager.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Tests;

/// <summary>پایگاه داده SQLite واقعی در حافظه برای هر تست (با اجرای مایگریشنها)</summary>
internal sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    private TestDatabase(SqliteConnection connection, AppDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public static TestDatabase New()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.Migrate();

        return new TestDatabase(connection, context);
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
