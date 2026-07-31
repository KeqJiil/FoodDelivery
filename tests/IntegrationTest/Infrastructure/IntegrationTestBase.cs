namespace FoodDelivery.IntegrationTest.Infrastructure;

[Collection("Database")]
public abstract class IntegrationTestBase(MsSqlContainerFixture fixture) : IAsyncLifetime
{
    protected string ConnectionString => fixture.ConnectionString;

    public async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}