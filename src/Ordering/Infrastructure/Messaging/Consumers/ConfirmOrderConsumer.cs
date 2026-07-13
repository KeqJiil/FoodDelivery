using MassTransit;
using MediatR;
using Ordering.Application.ConfirmOrder;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.IntegrationEvents;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class ConfirmOrderConsumer(ISender mediator) : IConsumer<ConfirmOrder>
{
    public async Task Consume(ConsumeContext<ConfirmOrder> context)
    {
        var msg = context.Message;
        var result = await mediator.Send(new ConfirmOrderCommand(new OrderId(msg.OrderId)));
        if (!result.IsSuccess && result.Error!.Type is not (ErrorEnum.Conflict or ErrorEnum.NotFound))
            throw new InvalidOperationException($"Failed to confirm order {msg.OrderId}, Error: {result.Error.Message}");
    }
}