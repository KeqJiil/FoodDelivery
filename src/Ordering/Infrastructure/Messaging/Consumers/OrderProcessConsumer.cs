using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.OrderProcess;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class OrderProcessConsumer(ISender mediatr, ILogger<OrderProcessConsumer> logger) : IConsumer<OrderProcess>
{
    public async Task Consume(ConsumeContext<OrderProcess> context)
    {
        var msg = context.Message;
        var result = await mediatr.Send(new OrderProcessCommand(msg.OrderId), context.CancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error!.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Ignored OrderProcess for {OrderId}: {Error}", msg.OrderId, result.Error.Message);
                return;
            }

            logger.LogError("Failed to change status on order {OrderId} : {Error}",
                msg.OrderId, result.Error!.Message);
            throw new InvalidOperationException(
                $"Failed to change order status with id {msg.OrderId}, Error: {result.Error!.Message}");
        }

        logger.LogInformation("Order processing finished {OrderId}", msg.OrderId);
    }
}