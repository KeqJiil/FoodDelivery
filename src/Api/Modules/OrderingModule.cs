using MassTransit;
using SharedKernel.Infrastructure.Interceptors;
using Ordering.Application.Abstractions;
using Ordering.Domain.Events;
using Ordering.Infrastructure.Adapters;
using Ordering.Infrastructure.Messaging.Consumers;
using Ordering.Infrastructure.Messaging.Translators;
using SharedKernel.Infrastructure.Messaging;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Persistence.Readers;
using Ordering.Infrastructure.Persistence.Repositories;

namespace Api.Modules;

public static class OrderingModule
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderReader, OrderReader>();
        services.AddScoped<IRestaurantMinimumOrderPriceAdapter, RestaurantMinimumOrderPriceAdapter>();
        services.AddScoped<IRestaurantActiveAdapter, RestaurantActiveAdapter>();
        services.AddScoped<IMenuPriceForOrderLineAdapter, MenuPriceForOrderLineAdapter>();
        services.AddScoped<IIntegrationEventTranslator<OrderPlaced>, OrderPlacedIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<OrderConfirmed>, OrderConfirmedIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<OrderCancelled>, OrderCancelledIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<OrderFailed>, OrderFailedIntegrationTranslator>();
        services
            .AddScoped<IIntegrationEventTranslator<OrderStartedProcessing>,
                OrderStartedProcessingIntegrationTranslator>();

        return services;
    }

    public static IBusRegistrationConfigurator AddOrderingMessaging(this IBusRegistrationConfigurator busConfigurator,
        IConfiguration configuration)
    {
        busConfigurator.AddConsumer<OrderProcessConsumer>()
            .Endpoint(e => e.Name = Queues.OrderProcess);
        busConfigurator.AddConsumer<OrderFailConsumer>()
            .Endpoint(e => e.Name = Queues.OrderFail);
        busConfigurator.AddConsumer<ConfirmOrderConsumer>()
            .Endpoint(e => e.Name = Queues.ConfirmOrder);
        busConfigurator.AddConsumer<ChangeOrderLineItemPriceConsumer>()
            .Endpoint(e => e.Name = Queues.MenuItemPriceChanged);

        return busConfigurator;
    }
}