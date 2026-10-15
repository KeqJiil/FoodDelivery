using MassTransit;
using SharedKernel.Infrastructure.Interceptors;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.Events;
using Restaurants.Infrastructure.Messaging.Translators;
using Restaurants.Infrastructure.Persistence;
using Restaurants.Infrastructure.Persistence.Readers;
using Restaurants.Infrastructure.Persistence.Repositories;

namespace Api.Modules;

public static class RestaurantsModule
{
    public static IServiceCollection AddRestaurantsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRestaurantReader, RestaurantReader>();
        services.AddScoped<IIntegrationEventTranslator<MenuItemPriceChanged>, MenuItemPriceChangedIntegrationEventTranslator>();

        return services;
    }

    public static IBusRegistrationConfigurator AddRestaurantsMessaging(this IBusRegistrationConfigurator busConfigurator,
        IConfiguration configuration)
    {
        busConfigurator.AddConsumers(typeof(RestaurantsModel).Assembly);

        return busConfigurator;
    }
}
