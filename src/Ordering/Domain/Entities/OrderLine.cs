using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

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

    public static Result<OrderLine, Error> Create(OrderLineId id, Money price, MenuItemRefId refId, byte quantity = 1)
    {
        if (quantity == 0)
            return Result<OrderLine, Error>.Fail(Error.Validation("Cannot create a new order line with 0 quantity"));

        var orderLine = new OrderLine(id, refId, quantity);
        orderLine.ChangePrice(price);
        return Result<OrderLine, Error>.Success(orderLine);
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