using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Enums;

namespace Ordering.Application.GetOrderById;

public record OrderDto(
    Guid Id,
    OrderStatus Status,
    Guid RestaurantRefId,
    Money? TotalPrice,
    IReadOnlyList<OrderLineDto> OrderLines,
    DateTime CreatedAt);