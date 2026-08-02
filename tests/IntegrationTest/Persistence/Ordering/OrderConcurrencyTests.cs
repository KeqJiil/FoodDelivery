using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Persistence.Ordering;

public class OrderConcurrencyTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SecondSave_ShouldThrow_WhenBothContextsLoadedTheSameStaleOrder()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));

        await using (var writeContext = CreateOrderingContext())
        {
            writeContext.Orders.Add(order);
            await writeContext.SaveChangesAsync();
        }

        await using var firstContext = CreateOrderingContext();
        await using var secondContext = CreateOrderingContext();

        var firstCopy = await firstContext.Orders.Include(o => o.OrderLines).FirstAsync(o => o.Id == order.Id);
        var secondCopy = await secondContext.Orders.Include(o => o.OrderLines).FirstAsync(o => o.Id == order.Id);

        firstCopy.Place(Money.Create(Currency.Usd, 1m).Ok!);
        await firstContext.SaveChangesAsync();

        secondCopy.Cancel();

        DbUpdateConcurrencyException? caughtException = null;
        try
        {
            await secondContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            caughtException = ex;
        }

        caughtException.Should().NotBeNull();
    }

    private OrderingDbContext CreateOrderingContext()
    {
        return CreateContext<OrderingDbContext>("ordering", o => new OrderingDbContext(o));
    }
}