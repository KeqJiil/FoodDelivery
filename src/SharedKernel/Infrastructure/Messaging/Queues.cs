using MassTransit;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace SharedKernel.Infrastructure.Messaging;

public static class Queues
{
    public const string CreateRequest = "create-request";
    public const string CancelOrderRequest = "cancel-order-request";
    public const string OrderProcess = "order-process";
    public const string OrderFail = "order-fail";
    public const string ConfirmOrder = "confirm-order";
    public const string CreatePayment = "create-payment";
    public const string CancelPayment = "cancel-payment";
    public const string CreateDelivery = "create-delivery";

    public const string MenuItemPriceChanged = "menu-item-price-changed";

    public static IReadOnlyDictionary<Type, string> CommandQueues { get; } = new Dictionary<Type, string>
    {
        [typeof(CreateRequest)] = CreateRequest,
        [typeof(CancelOrderRequest)] = CancelOrderRequest,
        [typeof(OrderProcess)] = OrderProcess,
        [typeof(OrderFail)] = OrderFail,
        [typeof(ConfirmOrder)] = ConfirmOrder,
        [typeof(CreatePayment)] = CreatePayment,
        [typeof(CancelPayment)] = CancelPayment,
        [typeof(CreateDelivery)] = CreateDelivery
    };

    public static Uri AddressOf(string queue) => new($"queue:{queue}");
    
    public static void RegisterEndpointConventions()
    {
        Map<CreateRequest>(CreateRequest);
        Map<CancelOrderRequest>(CancelOrderRequest);
        Map<OrderProcess>(OrderProcess);
        Map<OrderFail>(OrderFail);
        Map<ConfirmOrder>(ConfirmOrder);
        Map<CreatePayment>(CreatePayment);
        Map<CancelPayment>(CancelPayment);
        Map<CreateDelivery>(CreateDelivery);
    }

    private static void Map<TMessage>(string queue) where TMessage : class
    {
        if (EndpointConvention.TryGetDestinationAddress<TMessage>(out _)) return;
        EndpointConvention.Map<TMessage>(AddressOf(queue));
    }
}
