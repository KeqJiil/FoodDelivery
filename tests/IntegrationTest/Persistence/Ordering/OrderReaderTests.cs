using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Ids;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Persistence.Readers;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Persistence.Ordering;

public class OrderReaderTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetByIdAsync_ShouldComputeTotalPrice_WhenOrderHasMultipleLines()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 10m).Ok!,
            new MenuItemRefId(Guid.NewGuid()), 2);
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), Money.Create(Currency.Usd, 5m).Ok!,
            new MenuItemRefId(Guid.NewGuid()), 3);

        await using (var context = CreateOrderingContext())
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateOrderingContext();
        var reader = new OrderReader(readContext);

        var dto = await reader.GetByIdAsync(order.Id.Id);

        dto.Should().NotBeNull();
        dto.Status.Should().Be(OrderStatus.Draft);
        dto.OrderLines.Should().HaveCount(2);
        dto.TotalPrice.Should().Be(Money.Create(Currency.Usd, 35m).Ok);
        dto.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullTotalPrice_WhenOrderHasNoLines()
    {
        var order = Order.Create(new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));

        await using (var context = CreateOrderingContext())
        {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateOrderingContext();
        var reader = new OrderReader(readContext);

        var dto = await reader.GetByIdAsync(order.Id.Id);

        dto.Should().NotBeNull();
        dto!.TotalPrice.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        await using var readContext = CreateOrderingContext();
        var reader = new OrderReader(readContext);

        var dto = await reader.GetByIdAsync(Guid.NewGuid());

        dto.Should().BeNull();
    }

    private OrderingDbContext CreateOrderingContext()
    {
        return CreateContext<OrderingDbContext>("ordering", o => new OrderingDbContext(o));
    }
}