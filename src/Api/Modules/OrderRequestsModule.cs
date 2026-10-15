using MassTransit;
using SharedKernel.Infrastructure.Interceptors;
using OrderRequests.Application.Abstractions;
using OrderRequests.Domain.Events;
using OrderRequests.Infrastructure.Messaging.Consumers;
using OrderRequests.Infrastructure.Messaging.Translators;
using SharedKernel.Infrastructure.Messaging;
using OrderRequests.Infrastructure.Persistence;
using OrderRequests.Infrastructure.Persistence.Readers;
using OrderRequests.Infrastructure.Persistence.Repositories;
using UnitOfWork = OrderRequests.Infrastructure.Persistence.UnitOfWork;

namespace Api.Modules;

public static class OrderRequestsModule
{
    public static IServiceCollection AddOrderRequestsModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IOrderRequestReader, OrderRequestReader>();
        services.AddScoped<IOrderRequestRepository, OrderRequestRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIntegrationEventTranslator<OrderApproved>, OrderApprovedIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<OrderCancelled>, OrderRequestCancelledIntegrationTranslator>();
        services.AddScoped<IIntegrationEventTranslator<OrderRejected>, OrderRejectedIntegrationEventTranslator>();

        return services;
    }

    public static IBusRegistrationConfigurator AddOrderRequestsMessaging(
        this IBusRegistrationConfigurator busConfigurator, IConfiguration configuration)
    {
        busConfigurator.AddConsumer<OrderRequestedConsumer>()
            .Endpoint(e => e.Name = Queues.CreateRequest);
        busConfigurator.AddConsumer<OrderCancelConsumer>()
            .Endpoint(e => e.Name = Queues.CancelOrderRequest);

        return busConfigurator;
    }
}