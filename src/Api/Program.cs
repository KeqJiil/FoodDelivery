using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Abstractions;
using Ordering.Infrastructure.Adapters;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Messaging.Translators;
using Ordering.Infrastructure.Persistence.Readers;
using Ordering.Infrastructure.Persistence.Repositories;
using Restaurants.Infrastructure.Messaging.Translators;
using Restaurants.Infrastructure.Persistence;
using Restaurants.Infrastructure.Persistence.Readers;
using Restaurants.Infrastructure.Persistence.Repositories;
using Serilog;
using SharedKernel.Infrastructure.Interceptors;
using SharedKernel.Infrastructure.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<DomainEventPublishInterceptor>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, Ordering.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<IOrderReader, OrderReader>();
builder.Services.AddScoped<IRestaurantMinimumOrderPriceAdapter, StubRestaurantMinimumOrderPriceAdapter>();
builder.Services.AddScoped<IMenuPriceForOrderLineAdapter, StubMenuPriceForOrderLineAdapter>();
builder.Services
    .AddScoped<IIntegrationEventTranslator<Ordering.Domain.Events.OrderPlaced>,
        OrderPlacedIntegrationEventTranslator>();
builder.Services
    .AddScoped<IIntegrationEventTranslator<Ordering.Domain.Events.OrderConfirmed>,
        OrderConfirmedIntegrationEventTranslator>();
builder.Services
    .AddScoped<IIntegrationEventTranslator<Ordering.Domain.Events.OrderCancelled>,
        OrderCancelledIntegrationEventTranslator>();
builder.Services.AddDbContext<OrderingDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
});

builder.Services.AddScoped<Restaurants.Application.Abstractions.IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<Restaurants.Application.Abstractions.IUnitOfWork, Restaurants.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<Restaurants.Application.Abstractions.IRestaurantReader, RestaurantReader>();
builder.Services
    .AddScoped<IIntegrationEventTranslator<Restaurants.Domain.Events.MenuItemPriceChanged>,
        MenuItemPriceChangedIntegrationEventTranslator>();
builder.Services.AddDbContext<RestaurantsDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(OrderingDbContext).Assembly);
    x.AddConsumers(typeof(RestaurantsDbContext).Assembly);

    x.AddConfigureEndpointsCallback((context, _, cfg) =>
    {
        cfg.UseDelayedRedelivery(r =>
            r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
        cfg.UseMessageRetry(r => r.Immediate(5));
        cfg.UseInMemoryOutbox(context);
    });

    x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddEntityFrameworkOutbox<RestaurantsDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(OrderingDbContext).Assembly,
    typeof(RestaurantsDbContext).Assembly));

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    var orderingMigrationOptions = new DbContextOptionsBuilder<OrderingDbContext>()
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .Options;
    await using var orderingMigrationContext = new OrderingDbContext(orderingMigrationOptions);
    await orderingMigrationContext.Database.MigrateAsync();

    var restaurantsMigrationOptions = new DbContextOptionsBuilder<RestaurantsDbContext>()
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .Options;
    await using var restaurantsMigrationContext = new RestaurantsDbContext(restaurantsMigrationOptions);
    await restaurantsMigrationContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();