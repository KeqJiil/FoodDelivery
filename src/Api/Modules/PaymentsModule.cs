using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Domain.Events;
using Payments.Infrastructure.Adapters;
using Payments.Infrastructure.Messaging.Translators;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence.Readers;
using Payments.Infrastructure.Persistence.Repositories;
using SharedKernel.Infrastructure.Interceptors;
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

        services.AddDbContext<PaymentsDbContext>((sp, options) =>
        {
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        return services;
    }

    public static IBusRegistrationConfigurator AddPaymentsMessaging(this IBusRegistrationConfigurator busConfigurator)
    {
        busConfigurator.AddConsumers(typeof(PaymentsDbContext).Assembly);
        busConfigurator.AddEntityFrameworkOutbox<PaymentsDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            }
        );

        return busConfigurator;
    }

    public static async Task MigratePaymentsDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .Options;
        await using var context = new PaymentsDbContext(options);
        await context.Database.MigrateAsync();
    }
}
