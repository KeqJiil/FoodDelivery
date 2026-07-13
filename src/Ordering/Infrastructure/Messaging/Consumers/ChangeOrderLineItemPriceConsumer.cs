using MassTransit;
using MediatR;
using Ordering.Application.ChangeOrderLinePrice;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.IntegrationEvents;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class ChangeOrderLineItemPriceConsumer(ISender mediator) : IConsumer<MenuItemPriceChanged>
{
    public async Task Consume(ConsumeContext<MenuItemPriceChanged> context)
    {
        var msg = context.Message;
        var result = await mediator.Send(new ChangeOrderLinePriceCommand(
            new MenuItemRefId(msg.MenuItemId),
            new Money(msg.Currency, msg.Amount)));
        if (!result.IsSuccess && result.Error!.Type is not (ErrorEnum.Conflict or ErrorEnum.NotFound))
            throw new InvalidOperationException(
                $"Failed to change order line price with menu id {msg.MenuItemId}, Error: {result.Error.Message}");
    }
}