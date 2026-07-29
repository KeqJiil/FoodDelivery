using Microsoft.EntityFrameworkCore;
using Saga.Infrastructure.Persistence;

namespace Api.Modules;

public static class SagaModule
{
    public static IServiceCollection AddSagaModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SagaDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }

    public static async Task MigrateSagaDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<SagaDbContext>()
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            .Options;
        await using var context = new SagaDbContext(options);
        await context.Database.MigrateAsync();
    }
}