using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.UnitTest.TestHelpers;

internal static class OrderFactory
{
    private static readonly Money LinePrice = Money.Create(Currency.Usd, 10m).Ok!;
    private static readonly Money MinimalPrice = Money.Create(Currency.Usd, 1m).Ok!;

    public static Order Draft(OrderId? id = null)
    {
        var order = Order.Create(id ?? new OrderId(Guid.NewGuid()), new RestaurantRefId(Guid.NewGuid()));
        order.AddOrderLineItem(new OrderLineId(Guid.NewGuid()), LinePrice, new MenuItemRefId(Guid.NewGuid()));
        return order;
    }

    public static Order Pending(OrderId? id = null)
    {
        var order = Draft(id);
        order.Place(MinimalPrice);
        return order;
    }

    public static Order Processing(OrderId? id = null)
    {
        var order = Pending(id);
        order.StartProcessing();
        return order;
    }

    public static Order Confirmed(OrderId? id = null)
    {
        var order = Processing(id);
        order.Confirm();
        return order;
    }

    public static Order Failed(OrderId? id = null)
    {
        var order = Pending(id);
        order.Fail();
        return order;
    }

    public static Order Cancelled(OrderId? id = null)
    {
        var order = Pending(id);
        order.Cancel();
        return order;
    }
}
