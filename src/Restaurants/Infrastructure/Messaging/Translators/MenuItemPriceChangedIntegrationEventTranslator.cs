using Restaurants.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Restaurants.Infrastructure.Messaging.Translators;

public class MenuItemPriceChangedIntegrationEventTranslator : IIntegrationEventTranslator<MenuItemPriceChanged>
{
    public IntegrationEvent? Translate(MenuItemPriceChanged domainEvent)
    {
        return new MenuItemPriceChangedIntegration(domainEvent.MenuId.Id,
            domainEvent.NewPrice.Amount, domainEvent.NewPrice.Currency);
    }
}