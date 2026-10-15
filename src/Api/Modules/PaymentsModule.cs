using MassTransit;
using SharedKernel.Infrastructure.Interceptors;
using Payments.Application.Abstractions;
using Payments.Domain.Events;
using Payments.Infrastructure.Adapters;
using Payments.Infrastructure.Messaging.Consumers;
using Payments.Infrastructure.Messaging.Translators;
using SharedKernel.Infrastructure.Messaging;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence.Readers;
using Payments.Infrastructure.Persistence.Repositories;
using UnitOfWork = Payments.Infrastructure.Persistence.UnitOfWork;

namespace Api.Modules;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IPaymentReader, PaymentReader>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentGatewayAdapter, MockPaymentGatewayAdapter>();
        services.AddScoped<IIntegrationEventTranslator<PaymentSucceeded>, PaymentSucceededIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<PaymentFailed>, PaymentFailedIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<PaymentCancelled>, PaymentCancelledIntegrationTranslator>();

        return services;
    }

    public static IBusRegistrationConfigurator AddPaymentsMessaging(this IBusRegistrationConfigurator busConfigurator,
        IConfiguration configuration)
    {
        busConfigurator.AddConsumer<OrderConfirmedConsumer>()
            .Endpoint(e => e.Name = Queues.CreatePayment);
        busConfigurator.AddConsumer<CancelPaymentConsumer>()
            .Endpoint(e => e.Name = Queues.CancelPayment);

        return busConfigurator;
    }
}
