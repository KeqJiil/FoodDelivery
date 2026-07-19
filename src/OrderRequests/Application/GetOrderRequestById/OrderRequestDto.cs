using OrderRequests.Domain.Enums;

namespace OrderRequests.Application.GetOrderRequestById;

public record class GetOrderRequestByIdRequestDto(
    Guid OrderRequestId,
    Guid RestaurantId,
    Guid OrderId,
    OrderRequestStatus Status,
    DateTime CreatedAt
);