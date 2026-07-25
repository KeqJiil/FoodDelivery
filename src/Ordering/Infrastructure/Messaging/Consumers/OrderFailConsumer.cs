using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.OrderFail;
using Ordering.Application.OrderProcess;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class OrderFailConsumer(ISender mediatr, ILogger<OrderFailConsumer> logger) : IConsumer<OrderFail>
{
    public async Task Consume(ConsumeContext<OrderFail> context)
    {
        var msg = context.Message;
        var result = await mediatr.Send(new OrderFailCommand(msg.OrderId), context.CancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogError("Failed to change status on order {OrderId} : {Error}",
                msg.OrderId, result.Error!.Message);
            throw new InvalidOperationException(
                $"Failed to change order status with id {msg.OrderId}, Error: {result.Error!.Message}");
        }

        logger.LogInformation("Order processing finished {OrderId}", msg.OrderId);
    }
}