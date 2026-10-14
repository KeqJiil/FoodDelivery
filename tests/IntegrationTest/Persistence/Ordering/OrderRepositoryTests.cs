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
    public async Task GetDraftIdsByMenuItemIdAsync_ShouldReturnOnlyDraftOrders_ContainingTheMenuItem()
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

        var result = await readRepository.GetDraftIdsByMenuItemIdAsync(targetMenuItem, 10);

        result.Should().ContainSingle().Which.Should().Be(draftOrderWithItem.Id);
    }

    [Fact]
    public async Task GetDraftIdsByMenuItemIdAsync_ShouldPageThroughAllDrafts_UsingAfterIdCursor()
    {
        var menuItem = new MenuItemRefId(Guid.NewGuid());
        var orders = Enumerable.Range(0, 5).Select(_ =>
        {
            var order = Order.Create(new OrderId(), new RestaurantRefId(Guid.NewGuid()));
            order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 5m).Ok!, menuItem);
            return order;
        }).ToList();

        await using (var context = CreateOrderingContext())
        {
            var repository = new OrderRepository(context);
            orders.ForEach(repository.Add);
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateOrderingContext();
        var readRepository = new OrderRepository(readContext);

        var firstPage = await readRepository.GetDraftIdsByMenuItemIdAsync(menuItem, 2);
        var secondPage = await readRepository.GetDraftIdsByMenuItemIdAsync(menuItem, 2, firstPage[^1]);
        var thirdPage = await readRepository.GetDraftIdsByMenuItemIdAsync(menuItem, 2, secondPage[^1]);

        firstPage.Should().HaveCount(2);
        secondPage.Should().HaveCount(2);
        thirdPage.Should().HaveCount(1);
        firstPage.Concat(secondPage).Concat(thirdPage).Should().BeEquivalentTo(orders.Select(o => o.Id));
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnRequestedOrders_WithOrderLines()
    {
        var menuItem = new MenuItemRefId(Guid.NewGuid());
        var first = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        first.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 5m).Ok!, menuItem);
        var second = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        second.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 5m).Ok!, menuItem);
        var notRequested = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));

        await using (var context = CreateOrderingContext())
        {
            var repository = new OrderRepository(context);
            repository.Add(first);
            repository.Add(second);
            repository.Add(notRequested);
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateOrderingContext();
        var readRepository = new OrderRepository(readContext);

        var result = await readRepository.GetByIdsAsync([first.Id, second.Id]);

        result.Select(o => o.Id).Should().BeEquivalentTo([first.Id, second.Id]);
        result.Should().OnlyContain(o => o.OrderLines.Count == 1);
    }

    private OrderingDbContext CreateOrderingContext()
    {
        return CreateContext<OrderingDbContext>("ordering", o => new OrderingDbContext(o));
    }
}