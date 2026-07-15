using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.ConfirmOrder;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.IntegrationEvents;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class ConfirmOrderConsumer(ISender mediator, ILogger<ConfirmOrderConsumer> logger) : IConsumer<ConfirmOrder>
{
    public async Task Consume(ConsumeContext<ConfirmOrder> context)
    {
        var msg = context.Message;
        var result = await mediator.Send(new ConfirmOrderCommand(new OrderId(msg.OrderId)));
        if (!result.IsSuccess)
        {
            if (result.Error!.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Ignored ConfirmOrder for {OrderId}: {Error}", msg.OrderId, result.Error.Message);
                return;
            }

            logger.LogError("Failed to confirm order {OrderId}: {Error}", msg.OrderId, result.Error.Message);
            throw new InvalidOperationException($"Failed to confirm order {msg.OrderId}, Error: {result.Error.Message}");
        }

        logger.LogInformation("Consumed ConfirmOrder for {OrderId}", msg.OrderId);
    }
}