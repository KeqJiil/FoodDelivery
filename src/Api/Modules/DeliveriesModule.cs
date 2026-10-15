using Deliveries.Application.Abstractions;
using Deliveries.Domain.Events;
using Deliveries.Infrastructure.Messaging.Consumers;
using Deliveries.Infrastructure.Messaging.Translators;
using SharedKernel.Infrastructure.Messaging;
using Deliveries.Infrastructure.Persistence;
using Deliveries.Infrastructure.Persistence.Readers;
using Deliveries.Infrastructure.Persistence.Repositories;
using MassTransit;
using SharedKernel.Infrastructure.Interceptors;

namespace Api.Modules;

public static class DeliveriesModule
{
    public static IServiceCollection AddDeliveriesModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IDeliveryReader, DeliveryReader>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIntegrationEventTranslator<DeliveryCreated>, DeliveryPlacedTranslator>();

        return services;
    }

    public static IBusRegistrationConfigurator AddDeliveriesMessaging(
        this IBusRegistrationConfigurator busConfigurator, IConfiguration configuration)
    {
        busConfigurator.AddConsumer<CreateDeliveryConsumer>()
            .Endpoint(e => e.Name = Queues.CreateDelivery);

        return busConfigurator;
    }
}