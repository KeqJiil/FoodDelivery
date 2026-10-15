using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Infrastructure.Interceptors;

namespace Api.Persistence;

public static class PersistenceModule
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<DomainEventPublishInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), ConfigureSqlServer);
            options.AddInterceptors(sp.GetRequiredService<DomainEventPublishInterceptor>());
        });

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }

    public static IBusRegistrationConfigurator AddPersistenceOutbox(this IBusRegistrationConfigurator busConfigurator,
        IConfiguration configuration)
    {
        busConfigurator.AddEntityFrameworkOutbox<AppDbContext>(o =>
        {
            o.UseSqlServer();
            o.QueryDelay = TimeSpan.FromMilliseconds(configuration.GetValue("Messaging:OutboxQueryDelayMs", 10000));
            o.UseBusOutbox();
        });

        return busConfigurator;
    }

    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }

    public static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sqlOptions)
    {
        sqlOptions.EnableRetryOnFailure();
        sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", AppDbContext.MessagingSchema);
    }
}