using MassTransit;
using Microsoft.EntityFrameworkCore;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.Events;
using Restaurants.Infrastructure.Messaging.Translators;
using Restaurants.Infrastructure.Persistence;
using Restaurants.Infrastructure.Persistence.Readers;
using Restaurants.Infrastructure.Persistence.Repositories;
using SharedKernel.Infrastructure.Interceptors;

namespace Api.Modules;

public static class RestaurantsModule
{
    public static IServiceCollection AddRestaurantsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRestaurantReader, RestaurantReader>();
        services.AddScoped<IIntegrationEventTranslator<MenuItemPriceChanged>, MenuItemPriceChangedIntegrationEventTranslator>();

        services.AddDbContext<RestaurantsDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        return services;
    }

    public static IBusRegistrationConfigurator AddRestaurantsMessaging(this IBusRegistrationConfigurator busConfigurator)
    {
        busConfigurator.AddConsumers(typeof(RestaurantsDbContext).Assembly);
        busConfigurator.AddEntityFrameworkOutbox<RestaurantsDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        return busConfigurator;
    }

    public static async Task MigrateRestaurantsDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<RestaurantsDbContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .Options;
        await using var context = new RestaurantsDbContext(options);
        await context.Database.MigrateAsync();
    }
}
