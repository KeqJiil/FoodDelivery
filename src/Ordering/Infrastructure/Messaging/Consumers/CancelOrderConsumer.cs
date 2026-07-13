using MassTransit;
using MediatR;
using Ordering.Application.CancelOrder;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.IntegrationEvents;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class CancelOrderConsumer(ISender mediator) : IConsumer<CancelOrder>
{
    public async Task Consume(ConsumeContext<CancelOrder> context)
    {
        var msg = context.Message;
        var result = await mediator.Send(new CancelOrderCommand(new OrderId(msg.OrderId)));
        if (!result.IsSuccess && result.Error!.Type is not (ErrorEnum.Conflict or ErrorEnum.NotFound))
            throw new InvalidOperationException($"Failed to cancel order {msg.OrderId}, Error: {result.Error.Message}");
    }
}