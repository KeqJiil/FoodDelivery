using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderRequests.Application.CreateOrder;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents;

namespace OrderRequests.Infrastructure.Messaging.Consumers;

public class OrderRequestedConsumer(ISender mediator, ILogger<OrderRequestedConsumer> logger)
    : IConsumer<OrderPlacedIntegration>
{
    public async Task Consume(ConsumeContext<OrderPlacedIntegration> context)
    {
        var msg = context.Message;
        var result =
            await mediator.Send(new CreateOrderCommand(new OrderRefId(msg.OrderId),
                new RestaurantRefId(msg.RestaurantId)));
        if (!result.IsSuccess)
        {
            var error = result.Error ?? Error.Unexpected();
            logger.LogError("Failed to create order request {OrderId}: {Error}", msg.OrderId, error.Message);
            throw new InvalidOperationException($"Could not process Order Request Creation: {error.Message}");
        }

        logger.LogInformation("Order Requested {OrderId}", msg.OrderId);
    }
}