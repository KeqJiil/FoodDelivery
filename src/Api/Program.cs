using Api.Modules;
using MassTransit;
using Ordering.Infrastructure.Persistence;
using OrderRequests.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence;
using Restaurants.Infrastructure.Persistence;
using Serilog;
using SharedKernel.Infrastructure.Interceptors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<DomainEventPublishInterceptor>();

builder.Services.AddOrderingModule(builder.Configuration);
builder.Services.AddRestaurantsModule(builder.Configuration);
builder.Services.AddOrderRequestsModule(builder.Configuration);
builder.Services.AddPaymentsModule(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.AddOrderingMessaging();
    x.AddRestaurantsMessaging();
    x.AddOrderRequestsMessaging();
    x.AddPaymentsMessaging();

    x.AddConfigureEndpointsCallback((context, _, cfg) =>
    {
        cfg.UseDelayedRedelivery(r =>
            r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
        cfg.UseMessageRetry(r => r.Immediate(5));
        cfg.UseInMemoryOutbox(context);
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
    typeof(RestaurantsDbContext).Assembly,
    typeof(OrderRequestsDbContext).Assembly,
    typeof(PaymentsDbContext).Assembly
));

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

    await app.MigrateOrderingDatabaseAsync(builder.Configuration);
    await app.MigrateRestaurantsDatabaseAsync(builder.Configuration);
    await app.MigrateOrderRequestsDatabaseAsync(builder.Configuration);
    await app.MigratePaymentsDatabaseAsync(builder.Configuration);
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();