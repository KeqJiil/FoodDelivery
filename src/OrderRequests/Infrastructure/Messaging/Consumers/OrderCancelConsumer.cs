using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderRequests.Application.CancelOrder;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace OrderRequests.Infrastructure.Messaging.Consumers;

public class OrderCancelConsumer(ISender mediatr, ILogger<OrderCancelConsumer> logger) : IConsumer<CancelOrderRequest>
{
    public async Task Consume(ConsumeContext<CancelOrderRequest> context)
    {
        var msg = context.Message;
        var result = await mediatr.Send(new CancelOrderCommand(msg.OrderId), context.CancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error!.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Ignored cancellation for order {OrderId}: {Error}", msg.OrderId, result.Error.Message);
                return;
            }
            throw new InvalidOperationException($"Failed to cancel order request: {result.Error.Message}");
        }
        
        logger.LogInformation("Order cancelled {OrderId}", msg.OrderId);
    }
}