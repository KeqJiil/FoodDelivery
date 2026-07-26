using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payments.Application.CancelPayment;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.Incoming;

namespace Payments.Infrastructure.Messaging.Consumers;

public class CancelPaymentConsumer(ILogger<CancelPaymentConsumer> logger, ISender mediatr) : IConsumer<CancelPayment>
{
    public async Task Consume(ConsumeContext<CancelPayment> context)
    {
        var msg = context.Message;
        var result = await mediatr.Send(new CancelPaymentByOrderIdCommand(msg.OrderId));
        if (!result.IsSuccess)
        {
            if (result.Error.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Payment cancel conflict or not not found msg:{msg}", result.Error.Message);
                return;
            }

            logger.LogError("Payment cancel unexpected error msg:{msg}", result.Error.Message);
            throw new InvalidOperationException("Payment cancel unexpected error");
        }

        logger.LogInformation("Payment {OrderId} cancelled", msg.OrderId);
    }
}