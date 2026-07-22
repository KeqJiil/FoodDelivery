using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payments.Application.CreatePayment;
using Payments.Domain.Ids;
using SharedKernel.Infrastructure.IntegrationEvents;

namespace Payments.Infrastructure.Messaging.Consumers;

public class OrderConfirmedConsumer(ISender mediator, ILogger<OrderConfirmedConsumer> logger)
    : IConsumer<OrderConfirmedIntegration>
{
    public async Task Consume(ConsumeContext<OrderConfirmedIntegration> context)
    {
        var msg = context.Message;
        var result = await mediator.Send(new CreatePaymentCommand(new OrderRefId(msg.Id), msg.Amount, msg.Currency));
        if (!result.IsSuccess)
        {
            var error = result.Error!;
            logger.LogError("Failed to create payment for order {OrderId}: {Error}", msg.Id, error.Message);
            throw new InvalidOperationException($"Could not create payment for order {msg.Id}: {error.Message}");
        }

        logger.LogInformation("Payment {PaymentId} created for order {OrderId}", result.Ok, msg.Id);
    }
}
