using Deliveries.Application.CreateDelivery;
using Deliveries.Domain.Ids;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.IntegrationEvents;

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
            logger.LogError("Failed to create delivery for order {OrderId}: {Error}", msg.OrderId, error.Message);
            throw new InvalidOperationException($"Could not create delivery for order {msg.OrderId}: {error.Message}");
        }

        logger.LogInformation("Delivery {DeliveryId} created for order {OrderId}", result.Ok, msg.OrderId);
    }
}
