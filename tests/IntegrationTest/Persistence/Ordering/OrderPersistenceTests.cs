using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Persistence.Ordering;

public class OrderPersistenceTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SaveAndReload_ShouldPersistOrderWithLines()
    {
        var restaurantId = new RestaurantRefId(Guid.NewGuid());
        var order = Order.Create(new OrderId(Guid.NewGuid()), restaurantId);
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 12.50m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));

        await using (var writeContext = CreateOrderingContext())
        {
            writeContext.Orders.Add(order);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateOrderingContext();
        var reloaded = await readContext.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        reloaded.Should().NotBeNull();
        reloaded!.Status.Should().Be(OrderStatus.Draft);
        reloaded.RestaurantRefId.Should().Be(restaurantId);
        reloaded.OrderLines.Should().HaveCount(1);
        reloaded.OrderLines[0].Price.Should().Be(Money.Create(Currency.Usd, 12.50m).Ok!);
    }

    private OrderingDbContext CreateOrderingContext()
    {
        return CreateContext<OrderingDbContext>("ordering", o => new OrderingDbContext(o));
    }
}