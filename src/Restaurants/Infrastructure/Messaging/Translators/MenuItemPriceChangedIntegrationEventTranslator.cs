using Restaurants.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Restaurants.Infrastructure.Messaging.Translators;

public class MenuItemPriceChangedIntegrationEventTranslator : IIntegrationEventTranslator<MenuItemPriceChanged>
{
    public IntegrationEvent? Translate(MenuItemPriceChanged domainEvent)
    {
        throw new NotImplementedException();
    }
}