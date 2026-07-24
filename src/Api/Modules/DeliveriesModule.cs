using Deliveries.Application.Abstractions;
using Deliveries.Domain.Events;
using Deliveries.Infrastructure.Messaging.Translators;
using Deliveries.Infrastructure.Persistence;
using Deliveries.Infrastructure.Persistence.Readers;
using Deliveries.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Interceptors;
using UnitOfWork = Deliveries.Infrastructure.Persistence.UnitOfWork;

namespace Api.Modules;

public static class DeliveriesModule
{
    public static IServiceCollection AddDeliveriesModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IDeliveryReader, DeliveryReader>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIntegrationEventTranslator<DeliveryCompleted>, DeliveryCompletedIntegrationEventTranslator>();
        services.AddScoped<IIntegrationEventTranslator<DeliveryFailed>, DeliveryFailedIntegrationEventTranslator>();

        services.AddDbContext<DeliveriesDbContext>((sp, options) =>
        {
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        return services;
    }

    public static IBusRegistrationConfigurator AddDeliveriesMessaging(
        this IBusRegistrationConfigurator busConfigurator)
    {
        busConfigurator.AddConsumers(typeof(DeliveriesDbContext).Assembly);
        busConfigurator.AddEntityFrameworkOutbox<DeliveriesDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            }
        );

        return busConfigurator;
    }

    public static async Task MigrateDeliveriesDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<DeliveriesDbContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .Options;
        await using var context = new DeliveriesDbContext(options);
        await context.Database.MigrateAsync();
    }
}
