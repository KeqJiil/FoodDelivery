using Ordering.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Application.GetOrderById;

public record OrderDto(
    Guid Id,
    OrderStatus Status,
    Guid RestaurantRefId,
    Money? TotalPrice,
    IReadOnlyList<OrderLineDto> OrderLines,
    DateTime CreatedAt);