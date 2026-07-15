using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.ChangeOrderLinePrice;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.IntegrationEvents;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class ChangeOrderLineItemPriceConsumer(ISender mediator, ILogger<ChangeOrderLineItemPriceConsumer> logger)
    : IConsumer<MenuItemPriceChanged>
{
    public async Task Consume(ConsumeContext<MenuItemPriceChanged> context)
    {
        var msg = context.Message;
        var result = await mediator.Send(new ChangeOrderLinePriceCommand(
            new MenuItemRefId(msg.MenuItemId),
            new Money(msg.Currency, msg.Amount)));
        if (!result.IsSuccess)
        {
            if (result.Error!.Type is ErrorEnum.Conflict or ErrorEnum.NotFound)
            {
                logger.LogWarning("Ignored MenuItemPriceChanged for menu item {MenuItemId}: {Error}",
                    msg.MenuItemId, result.Error.Message);
                return;
            }

            logger.LogError("Failed to change order line price for menu item {MenuItemId}: {Error}",
                msg.MenuItemId, result.Error.Message);
            throw new InvalidOperationException(
                $"Failed to change order line price with menu id {msg.MenuItemId}, Error: {result.Error.Message}");
        }

        logger.LogInformation("Consumed MenuItemPriceChanged for menu item {MenuItemId}", msg.MenuItemId);
    }
}