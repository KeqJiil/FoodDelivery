using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.CancelOrder;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.IntegrationEvents;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class CancelOrderConsumer(ISender mediator, ILogger<CancelOrderConsumer> logger) : IConsumer<CancelOrder>
{
    public async Task Consume(ConsumeContext<CancelOrder> context)
    {
        var msg = context.Message;
        var result = await mediator.Send(new CancelOrderCommand(new OrderId(msg.OrderId)));
        if (!result.IsSuccess)
        {
            if (result.Error!.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Ignored CancelOrder for {OrderId}: {Error}", msg.OrderId, result.Error.Message);
                return;
            }

            logger.LogError("Failed to cancel order {OrderId}: {Error}", msg.OrderId, result.Error.Message);
            throw new InvalidOperationException($"Failed to cancel order {msg.OrderId}, Error: {result.Error.Message}");
        }

        logger.LogInformation("Consumed CancelOrder for {OrderId}", msg.OrderId);
    }
}