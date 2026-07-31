using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.IntegrationTest.Infrastructure;

[Collection("Database")]
public abstract class IntegrationTestBase(MsSqlContainerFixture fixture) : IAsyncLifetime
{
    protected string ConnectionString => fixture.ConnectionString;

    public async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected TContext CreateContext<TContext>(string schema, Func<DbContextOptions<TContext>, TContext> createContext)
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(ConnectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema))
            .Options;

        return createContext(options);
    }
}