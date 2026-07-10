using Ordering.Domain.Ids;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Entities;

public class OrderLine
{
    public OrderLineId Id { get; }
    public Money Price { get; private set; }
    public MenuItemRefId MenuItemRefId { get; }
    public byte Quantity { get; private set; }

    private OrderLine(OrderLineId id, Money price, MenuItemRefId refId, byte quantity)
    {
        Id = id;
        Price = price;
        MenuItemRefId = refId;
        Quantity = quantity;
    }

    public static OrderLine Create(OrderLineId id, Money price, MenuItemRefId refId, byte quantity)
    {
        return quantity == 0
            ? throw new InvalidOperationException($"Cannot create a new order line with 0 quantity")
            : new OrderLine(id, price, refId, quantity);
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