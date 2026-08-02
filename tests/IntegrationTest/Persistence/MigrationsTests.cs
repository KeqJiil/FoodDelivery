using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Ordering.Infrastructure.Persistence;

namespace FoodDelivery.IntegrationTest.Persistence;

[Collection("Database")]
public class MigrationsTests(MsSqlContainerFixture fixture)
{
    [Fact]
    public async Task OrderingDbContext_ShouldHaveNoPendingMigrations()
    {
        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlServer(fixture.ConnectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering"))
            .Options;

        await using var context = new OrderingDbContext(options);

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        pendingMigrations.Should().BeEmpty();
    }
}