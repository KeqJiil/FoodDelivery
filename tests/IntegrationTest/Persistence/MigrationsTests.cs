using Api.Persistence;
using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.IntegrationTest.Persistence;

[Collection("Database")]
public class MigrationsTests(MsSqlContainerFixture fixture)
{
    [Fact]
    public async Task AppDbContext_ShouldHaveNoPendingMigrations()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(fixture.ConnectionString, PersistenceModule.ConfigureSqlServer)
            .Options;

        await using var context = new AppDbContext(options);

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        pendingMigrations.Should().BeEmpty();
    }
}