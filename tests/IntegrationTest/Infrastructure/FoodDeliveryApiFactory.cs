using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FoodDelivery.IntegrationTest.Infrastructure;

public class FoodDeliveryApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly RabbitMqContainerFixture _rmq = new();
    private readonly MsSqlContainerFixture _msql = new();

    public string ConnectionString => _msql.ConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMq:Host"] = _rmq.Host, ["RabbitMq:Password"] = RabbitMqContainerFixture.Password,
                ["RabbitMq:Username"] = RabbitMqContainerFixture.Username,
                ["ConnectionStrings:DefaultConnection"] = _msql.ConnectionString
            });
        });
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll([_rmq.InitializeAsync(), _msql.InitializeAsync()]);
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll([_rmq.DisposeAsync(), _msql.DisposeAsync()]);
    }
}