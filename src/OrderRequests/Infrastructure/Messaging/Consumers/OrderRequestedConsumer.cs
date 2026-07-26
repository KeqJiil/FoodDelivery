using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderRequests.Application.CreateOrder;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace OrderRequests.Infrastructure.Messaging.Consumers;

public class OrderRequestedConsumer(ISender mediator, ILogger<OrderRequestedConsumer> logger)
    : IConsumer<CreateRequest>
{
    public async Task Consume(ConsumeContext<CreateRequest> context)
    {
        var msg = context.Message;
        var result =
            await mediator.Send(new CreateOrderCommand(new OrderRefId(msg.OrderId),
                new RestaurantRefId(msg.RestaurantId)));
        if (!result.IsSuccess)
        {
            var error = result.Error ?? Error.Unexpected();
            if (error.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Ignored CreateRequest for order {OrderId}: {Error}", msg.OrderId, error.Message);
                return;
            }

            logger.LogError("Failed to create order request {OrderId}: {Error}", msg.OrderId, error.Message);
            throw new InvalidOperationException($"Could not process Order Request Creation: {error.Message}");
        }

        logger.LogInformation("Order Requested {OrderId}", msg.OrderId);
    }
}