using System.Text.Json.Serialization;
using Api.ExceptionHandlers;
using Api.Middleware;
using Api.Modules;
using Deliveries.Infrastructure.Persistence;
using MassTransit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering.Infrastructure.Persistence;
using OrderRequests.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence;
using Restaurants.Infrastructure.Persistence;
using Saga.Application;
using Saga.Infrastructure.Persistence;
using Serilog;
using SharedKernel.Infrastructure.Interceptors;
using SharedKernel.Infrastructure.Messaging;
using SharedKernel.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<BasicExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddScoped<DomainEventPublishInterceptor>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderingDbContext>("ordering-db")
    .AddDbContextCheck<RestaurantsDbContext>("restaurants-db")
    .AddDbContextCheck<OrderRequestsDbContext>("order-requests-db")
    .AddDbContextCheck<PaymentsDbContext>("payments-db")
    .AddDbContextCheck<DeliveriesDbContext>("deliveries-db")
    .AddDbContextCheck<SagaDbContext>("saga-db");

builder.Services.AddOpenTelemetry().ConfigureResource(resource =>
        resource.AddService(Environment.GetEnvironmentVariable("OpenTelemetryServiceName") ?? "FoodDelivery.Api"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddSource("MassTransit");
        tracing.AddOtlpExporter(x => x.Endpoint = new Uri(Environment.GetEnvironmentVariable("JaegerEndpoint")!));
        tracing.AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddConsoleExporter();
    });

builder.Services.AddOrderingModule(builder.Configuration);
builder.Services.AddRestaurantsModule(builder.Configuration);
builder.Services.AddOrderRequestsModule(builder.Configuration);
builder.Services.AddPaymentsModule(builder.Configuration);
builder.Services.AddDeliveriesModule(builder.Configuration);
builder.Services.AddSagaModule(builder.Configuration);

builder.Services.AddOptions<SagaOptions>().Bind(builder.Configuration.GetSection(SagaOptions.SectionName))
    .ValidateDataAnnotations().ValidateOnStart();

Queues.RegisterEndpointConventions();

builder.Services.AddMassTransit(x =>
{
    x.AddOrderingMessaging(builder.Configuration);
    x.AddRestaurantsMessaging(builder.Configuration);
    x.AddOrderRequestsMessaging(builder.Configuration);
    x.AddPaymentsMessaging(builder.Configuration);
    x.AddDeliveriesMessaging(builder.Configuration);

    x.AddHealthChecks();

    x.AddConfigureEndpointsCallback((context, _, cfg) =>
    {
        cfg.UseDelayedRedelivery(r =>
            r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
        cfg.UseMessageRetry(r => r.Immediate(5));
        cfg.UseInMemoryOutbox(context);
    });

    x.AddDelayedMessageScheduler();

    if (builder.Environment.IsDevelopment())
        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitMqPort = builder.Configuration.GetValue<ushort?>("RabbitMq:Port") ?? 5672;
            cfg.Host(builder.Configuration["RabbitMq:Host"], rabbitMqPort, "/", h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
            });
            cfg.UseDelayedMessageScheduler();
            cfg.ConfigureEndpoints(context);

            cfg.UseSendFilter(typeof(CorrelationSendFilter<>), context);
            cfg.UsePublishFilter(typeof(CorrelationPublishFilter<>), context);
            cfg.UseConsumeFilter(typeof(CorrelationConsumeFilter<>), context);
        });
    else
        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(builder.Configuration.GetConnectionString("AzureServiceBus"));
            cfg.UseServiceBusMessageScheduler();
            cfg.ConfigureEndpoints(context);

            cfg.UseSendFilter(typeof(CorrelationSendFilter<>), context);
            cfg.UsePublishFilter(typeof(CorrelationPublishFilter<>), context);
            cfg.UseConsumeFilter(typeof(CorrelationConsumeFilter<>), context);
        });

    x.AddSagaStateMachine<OrderSaga, OrderState>().EntityFrameworkRepository(r =>
    {
        r.ConcurrencyMode = ConcurrencyMode.Optimistic;
        r.ExistingDbContext<SagaDbContext>();
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(OrderingDbContext).Assembly,
    typeof(RestaurantsDbContext).Assembly,
    typeof(OrderRequestsDbContext).Assembly,
    typeof(PaymentsDbContext).Assembly,
    typeof(DeliveriesDbContext).Assembly
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
    app.MapSwaggerUI(setupAction: options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

await app.MigrateOrderingDatabaseAsync(builder.Configuration);
await app.MigrateRestaurantsDatabaseAsync(builder.Configuration);
await app.MigrateOrderRequestsDatabaseAsync(builder.Configuration);
await app.MigratePaymentsDatabaseAsync(builder.Configuration);
await app.MigrateDeliveriesDatabaseAsync(builder.Configuration);
await app.MigrateSagaDatabaseAsync(builder.Configuration);

app.UseCors("AllowAll");
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseMiddleware<CorrelationMiddleware>();
app.MapControllers();

app.Run();

public partial class Program;