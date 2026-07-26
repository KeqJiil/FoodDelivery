using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payments.Application.CreatePayment;
using Payments.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Payments.Infrastructure.Messaging.Consumers;

public class OrderConfirmedConsumer(ISender mediator, ILogger<OrderConfirmedConsumer> logger)
    : IConsumer<CreatePayment>
{
    public async Task Consume(ConsumeContext<CreatePayment> context)
    {
        var msg = context.Message;
        var result =
            await mediator.Send(new CreatePaymentCommand(new OrderRefId(msg.OrderId), msg.Amount, msg.Currency));
        if (!result.IsSuccess)
        {
            var error = result.Error!;
            if (error.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Ignored CreatePayment for order {OrderId}: {Error}", msg.OrderId, error.Message);
                return;
            }

            logger.LogError("Failed to create payment for order {OrderId}: {Error}", msg.OrderId, error.Message);
            throw new InvalidOperationException($"Could not create payment for order {msg.OrderId}: {error.Message}");
        }

        logger.LogInformation("Payment {PaymentId} created for order {OrderId}", result.Ok, msg.OrderId);
    }
}