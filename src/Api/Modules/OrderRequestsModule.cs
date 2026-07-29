using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderRequests.Application.Abstractions;
using OrderRequests.Domain.Events;
using OrderRequests.Infrastructure.Messaging.Consumers;
using OrderRequests.Infrastructure.Messaging.Translators;
using SharedKernel.Infrastructure.Messaging;
using OrderRequests.Infrastructure.Persistence;
using OrderRequests.Infrastructure.Persistence.Readers;
using OrderRequests.Infrastructure.Persistence.Repositories;
using SharedKernel.Infrastructure.Interceptors;
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

        services.AddDbContext<OrderRequestsDbContext>((sp, options) =>
        {
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        return services;
    }

    public static IBusRegistrationConfigurator AddOrderRequestsMessaging(
        this IBusRegistrationConfigurator busConfigurator)
    {
        busConfigurator.AddConsumer<OrderRequestedConsumer>()
            .Endpoint(e => e.Name = Queues.CreateRequest);
        busConfigurator.AddConsumer<OrderCancelConsumer>()
            .Endpoint(e => e.Name = Queues.CancelOrderRequest);
        busConfigurator.AddEntityFrameworkOutbox<OrderRequestsDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            }
        );

        return busConfigurator;
    }

    public static async Task MigrateOrderRequestsDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<OrderRequestsDbContext>()
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            .Options;
        await using var context = new OrderRequestsDbContext(options);
        await context.Database.MigrateAsync();
    }
}