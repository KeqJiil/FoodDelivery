using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Abstractions;
using Ordering.Infrastructure.Adapters;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Persistence.Interceptors;
using Ordering.Infrastructure.Persistence.Readers;
using Ordering.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<DomainEventPublishInterceptor>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderReader, OrderReader>();
builder.Services.AddScoped<IRestaurantMinimumOrderPriceAdapter, StubRestaurantMinimumOrderPriceAdapter>();
builder.Services.AddScoped<IMenuPriceForOrderLineAdapter, StubMenuPriceForOrderLineAdapter>();
builder.Services.AddDbContext<OrderingDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(OrderingDbContext).Assembly);

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
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

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OrderingDbContext).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    var migrationOptions = new DbContextOptionsBuilder<OrderingDbContext>()
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .Options;
    await using var migrationContext = new OrderingDbContext(migrationOptions);
    await migrationContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();