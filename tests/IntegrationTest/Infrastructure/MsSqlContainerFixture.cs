using Deliveries.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ordering.Infrastructure.Persistence;
using OrderRequests.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence;
using Restaurants.Infrastructure.Persistence;
using Saga.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace FoodDelivery.IntegrationTest.Infrastructure;

public class MsSqlContainerFixture : IAsyncLifetime
{
    private const string DatabaseName = "FoodDeliveryTests";

    private readonly MsSqlContainer _container =
        new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString { get; private set; } = string.Empty;
    
    private DatabaseResetter _resetter = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await _container.ExecScriptAsync($"CREATE DATABASE [{DatabaseName}]");

        var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = DatabaseName
        };

        ConnectionString = builder.ConnectionString;
        await MigrateAsync<OrderingDbContext>("ordering", o => new OrderingDbContext(o));
        await MigrateAsync<RestaurantsDbContext>("restaurants", o => new RestaurantsDbContext(o));
        await MigrateAsync<OrderRequestsDbContext>("order_requests", o => new OrderRequestsDbContext(o));
        await MigrateAsync<PaymentsDbContext>("payments", o => new PaymentsDbContext(o));
        await MigrateAsync<DeliveriesDbContext>("deliveries", o => new DeliveriesDbContext(o));
        await MigrateAsync<SagaDbContext>("saga", o => new SagaDbContext(o));
        _resetter = new DatabaseResetter(ConnectionString);
        await _resetter.InitializeAsync();
    }

    public Task ResetDatabaseAsync() => _resetter.ResetAsync();

    private async Task MigrateAsync<TContext>(string schema, Func<DbContextOptions<TContext>, TContext> createContext)
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(ConnectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema))
            .Options;

        await using var context = createContext(options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}