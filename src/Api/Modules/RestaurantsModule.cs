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
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions => { sqlOptions.EnableRetryOnFailure(); sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "restaurants"); });
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        return services;
    }

    public static IBusRegistrationConfigurator AddRestaurantsMessaging(this IBusRegistrationConfigurator busConfigurator)
    {
        busConfigurator.AddConsumers(typeof(RestaurantsDbContext).Assembly);
        // No UseBusOutbox() here: MassTransit 8.x supports only one bus outbox per bus (see OrderingModule).
        busConfigurator.AddEntityFrameworkOutbox<RestaurantsDbContext>(o =>
        {
            o.UseSqlServer();
        });

        return busConfigurator;
    }

    public static async Task MigrateRestaurantsDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<RestaurantsDbContext>()
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions => { sqlOptions.EnableRetryOnFailure(); sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "restaurants"); })
            .Options;
        await using var context = new RestaurantsDbContext(options);
        await context.Database.MigrateAsync();
    }
}
