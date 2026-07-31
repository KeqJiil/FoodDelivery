using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Persistence.Repositories;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Persistence.Ordering;

public class OrderRepositoryTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetByMenuItemIdAsync_ShouldReturnOnlyDraftOrders_ContainingTheMenuItem()
    {
        var targetMenuItem = new MenuItemRefId(Guid.NewGuid());

        var draftOrderWithItem = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        draftOrderWithItem.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 5m).Ok!,
            targetMenuItem);

        var placedOrderWithItem = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        placedOrderWithItem.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 5m).Ok!,
            targetMenuItem);
        placedOrderWithItem.Place(Money.Create(Currency.Usd, 1m).Ok!);

        var draftOrderWithOtherItem = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        draftOrderWithOtherItem.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 5m).Ok!,
            new MenuItemRefId(Guid.NewGuid()));

        await using (var context = CreateOrderingContext())
        {
            var repository = new OrderRepository(context);
            repository.Add(draftOrderWithItem);
            repository.Add(placedOrderWithItem);
            repository.Add(draftOrderWithOtherItem);
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateOrderingContext();
        var readRepository = new OrderRepository(readContext);

        var result = await readRepository.GetByMenuItemIdAsync(targetMenuItem);

        result.Should().ContainSingle().Which.Id.Should().Be(draftOrderWithItem.Id);
    }

    private OrderingDbContext CreateOrderingContext()
    {
        return CreateContext<OrderingDbContext>("ordering", o => new OrderingDbContext(o));
    }
}