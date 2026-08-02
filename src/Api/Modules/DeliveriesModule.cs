using Deliveries.Application.Abstractions;
using Deliveries.Domain.Events;
using Deliveries.Infrastructure.Messaging.Consumers;
using Deliveries.Infrastructure.Messaging.Translators;
using SharedKernel.Infrastructure.Messaging;
using Deliveries.Infrastructure.Persistence;
using Deliveries.Infrastructure.Persistence.Readers;
using Deliveries.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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

        services.AddDbContext<DeliveriesDbContext>((sp, options) =>
        {
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"), sqlOptions => { sqlOptions.EnableRetryOnFailure(); sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "deliveries"); });
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        return services;
    }

    public static IBusRegistrationConfigurator AddDeliveriesMessaging(
        this IBusRegistrationConfigurator busConfigurator, IConfiguration configuration)
    {
        busConfigurator.AddConsumer<CreateDeliveryConsumer>()
            .Endpoint(e => e.Name = Queues.CreateDelivery);
        busConfigurator.AddEntityFrameworkOutbox<DeliveriesDbContext>(o =>
            {
                o.UseSqlServer();
                o.QueryDelay = TimeSpan.FromMilliseconds(configuration.GetValue("Messaging:OutboxQueryDelayMs", 10000));
            }
        );

        return busConfigurator;
    }

    public static async Task MigrateDeliveriesDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<DeliveriesDbContext>()
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions => { sqlOptions.EnableRetryOnFailure(); sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "deliveries"); })
            .Options;
        await using var context = new DeliveriesDbContext(options);
        await context.Database.MigrateAsync();
    }
}