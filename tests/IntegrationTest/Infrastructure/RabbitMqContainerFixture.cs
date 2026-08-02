using Testcontainers.RabbitMq;

namespace FoodDelivery.IntegrationTest.Infrastructure;

public class RabbitMqContainerFixture : IAsyncLifetime
{
    public string Host { get; private set; } = string.Empty;

    public ushort Port { get; private set; }
    public static string Username => "fooddelivery";
    public static string Password => "fooddelivery";

    private readonly RabbitMqContainer _container = new RabbitMqBuilder("masstransit/rabbitmq:4").WithUsername(Username)
        .WithPassword(Password).WithPortBinding(5672, true).Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Host = _container.Hostname;
        Port = _container.GetMappedPublicPort(5672);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}