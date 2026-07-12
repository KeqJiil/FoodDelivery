using Ordering.Domain.Ids;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Entities;

public class OrderLine
{
    public OrderLineId Id { get; }
    public Money Price { get; private set; } = null!;
    public MenuItemRefId MenuItemRefId { get; }
    public byte Quantity { get; private set; }

    private OrderLine(OrderLineId id, MenuItemRefId menuItemRefId, byte quantity)
    {
        Id = id;
        MenuItemRefId = menuItemRefId;
        Quantity = quantity;
    }

    public static OrderLine Create(OrderLineId id, Money price, MenuItemRefId refId, byte quantity)
    {
        if (quantity == 0)
            throw new InvalidOperationException("Cannot create a new order line with 0 quantity");

        var orderLine = new OrderLine(id, refId, quantity);
        orderLine.ChangePrice(price);
        return orderLine;
    }

    public void ChangePrice(Money price)
    {
        Price = price;
    }

    public Money GetTotalPrice()
    {
        return Price * Quantity;
    }

    public void IncreaseQuantity(byte quantity = 1)
    {
        Quantity += quantity;
    }

    public void DecreaseQuantity()
    {
        if (Quantity == 1) throw new InvalidOperationException("Cannot decrease quantity of one item");
        Quantity--;
    }
}