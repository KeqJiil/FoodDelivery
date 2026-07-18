using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Abstractions;
using Ordering.Domain.Events;
using Ordering.Infrastructure.Adapters;
using Ordering.Infrastructure.Messaging.Translators;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Persistence.Readers;
using Ordering.Infrastructure.Persistence.Repositories;
using SharedKernel.Infrastructure.Interceptors;

namespace Api.Modules;

public static class OrderingModule
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderReader, OrderReader>();
        services.AddScoped<IRestaurantMinimumOrderPriceAdapter, RestaurantMinimumOrderPriceAdapter>();
        services.AddScoped<IMenuPriceForOrderLineAdapter, MenuPriceForOrderLineAdapter>();
        services.AddScoped<IIntegrationEventTranslator<OrderPlaced>, OrderPlacedIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<OrderConfirmed>, OrderConfirmedIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<OrderCancelled>, OrderCancelledIntegrationEventTranslator>();

        services.AddDbContext<OrderingDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        return services;
    }

    public static IBusRegistrationConfigurator AddOrderingMessaging(this IBusRegistrationConfigurator busConfigurator)
    {
        busConfigurator.AddConsumers(typeof(OrderingDbContext).Assembly);
        busConfigurator.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        return busConfigurator;
    }

    public static async Task MigrateOrderingDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .Options;
        await using var context = new OrderingDbContext(options);
        await context.Database.MigrateAsync();
    }
}
