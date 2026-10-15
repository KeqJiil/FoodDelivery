using Api.Persistence;
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

    protected AppDbContext CreateAppContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}