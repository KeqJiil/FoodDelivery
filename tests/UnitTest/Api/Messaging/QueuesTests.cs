using System.Reflection;
using FluentAssertions;
using MassTransit;
using SharedKernel.Infrastructure.Messaging;

namespace Api.UnitTest.Messaging;

public class QueuesTests
{
    public QueuesTests() => Queues.RegisterEndpointConventions();

    private static readonly Assembly[] ModuleAssemblies =
    [
        typeof(Ordering.Infrastructure.Messaging.Consumers.OrderProcessConsumer).Assembly,
        typeof(OrderRequests.Infrastructure.Messaging.Consumers.OrderRequestedConsumer).Assembly,
        typeof(Payments.Infrastructure.Messaging.Consumers.CancelPaymentConsumer).Assembly,
        typeof(Deliveries.Infrastructure.Messaging.Consumers.CreateDeliveryConsumer).Assembly,
        typeof(Restaurants.Application.Abstractions.IRestaurantReader).Assembly
    ];

    private static List<(Type Consumer, Type Message)> DiscoverConsumers() =>
        ModuleAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>))
                .Select(i => (Consumer: t, Message: i.GetGenericArguments()[0])))
            .ToList();

    [Fact]
    public void EveryCommandTheSagaSends_ShouldHaveASendConvention()
    {
        foreach (var (messageType, queue) in Queues.CommandQueues)
        {
            EndpointConvention.TryGetDestinationAddress(messageType, out var address)
                .Should().BeTrue($"{messageType.Name} is sent by the saga and needs a destination");
            address!.ToString().Should().Be($"queue:{queue}");
        }
    }

    [Fact]
    public void CommandQueueNames_ShouldBeUnique()
    {
        Queues.CommandQueues.Values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void EveryCommandTheSagaSends_ShouldHaveExactlyOneConsumer()
    {
        var consumers = DiscoverConsumers();

        foreach (var command in Queues.CommandQueues.Keys)
            consumers.Should().ContainSingle(c => c.Message == command,
                $"{command.Name} is sent by the saga and must be consumed by exactly one consumer");
    }

    [Fact]
    public void EveryConsumedCommand_ShouldBeListedInQueues()
    {
        var sagaCommandNamespace = typeof(SharedKernel.Infrastructure.IntegrationEvents.Incoming.OrderProcess)
            .Namespace;

        var unmapped = DiscoverConsumers()
            .Where(c => c.Message.Namespace == sagaCommandNamespace)
            .Select(c => c.Message)
            .Where(m => !Queues.CommandQueues.ContainsKey(m))
            .Select(m => m.Name)
            .ToList();

        unmapped.Should().BeEmpty();
    }
}
