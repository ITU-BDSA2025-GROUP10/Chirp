using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace Chirp.Tests;

public sealed class TestSqliteFactory<TContext> : IAsyncLifetime
    where TContext : DbContext
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly Func<DbContextOptions<TContext>, TContext> _ctxCtor;

    public TestSqliteFactory(Func<DbContextOptions<TContext>, TContext> ctxCtor)
        => _ctxCtor = ctxCtor;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        using var ctx = CreateContext();
        // If you use Migrations in production, replace EnsureCreated with:
        // await ctx.Database.MigrateAsync();
        await ctx.Database.EnsureCreatedAsync();
    }

    public TContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlite(_connection)               // real provider
            .EnableSensitiveDataLogging()
            .Options;

        return _ctxCtor(options);
    }

    public async Task DisposeAsync() => await _connection.DisposeAsync();
}
