using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FoodDelivery.IntegrationTest.Infrastructure;

public class FoodDeliveryApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly RabbitMqContainerFixture _rmq = new();
    private readonly MsSqlContainerFixture _msql = new();
    private IHost? _host;

    public string ConnectionString => _msql.ConnectionString;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _host = base.CreateHost(builder);
        return _host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMq:Host"] = _rmq.Host, ["RabbitMq:Port"] = _rmq.Port.ToString(),
                ["RabbitMq:Password"] = RabbitMqContainerFixture.Password,
                ["RabbitMq:Username"] = RabbitMqContainerFixture.Username,
                ["ConnectionStrings:DefaultConnection"] = _msql.ConnectionString,
                ["Messaging:OutboxQueryDelayMs"] = "200"
            });
        });
    }

   async Task IAsyncLifetime.InitializeAsync()
    {
        await Task.WhenAll([_rmq.InitializeAsync(), _msql.InitializeAsync()]);
    }
    
   async Task IAsyncLifetime.DisposeAsync()
    {
        if (_host is not null)
            await _host.StopAsync();

        await base.DisposeAsync();
        await Task.WhenAll([_rmq.DisposeAsync(), _msql.DisposeAsync()]);
    }
}