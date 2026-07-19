using OrderRequests.Domain.Enums;

namespace OrderRequests.Application.GetOrderRequestById;

public record class OrderRequestDto(
    Guid OrderRequestId,
    Guid RestaurantId,
    Guid OrderId,
    OrderRequestStatus Status,
    DateTime CreatedAt
);