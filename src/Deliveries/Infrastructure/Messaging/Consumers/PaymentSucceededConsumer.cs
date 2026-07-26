using Deliveries.Application.CreateDelivery;
using Deliveries.Domain.Ids;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace Deliveries.Infrastructure.Messaging.Consumers;

public class PaymentSucceededConsumer(ISender mediator, ILogger<PaymentSucceededConsumer> logger)
    : IConsumer<PaymentSucceededIntegration>
{
    public async Task Consume(ConsumeContext<PaymentSucceededIntegration> context)
    {
        var msg = context.Message;
        var result = await mediator.Send(new CreateDeliveryCommand(new OrderRefId(msg.OrderId)));
        if (!result.IsSuccess)
        {
            var error = result.Error!;
            if (error.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Ignored PaymentSucceeded for order {OrderId}: {Error}", msg.OrderId, error.Message);
                return;
            }

            logger.LogError("Failed to create delivery for order {OrderId}: {Error}", msg.OrderId, error.Message);
            throw new InvalidOperationException($"Could not create delivery for order {msg.OrderId}: {error.Message}");
        }

        logger.LogInformation("Delivery {DeliveryId} created for order {OrderId}", result.Ok, msg.OrderId);
    }
}