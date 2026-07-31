using MediatR;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Domain.Enums;

namespace OrderRequests.Application.GetOrdersByRestaurantId;

public record GetOrdersByRestaurantIdQuery(
    Guid RestaurantId,
    DateTime? CursorCreatedAt,
    Guid? CursorId,
    byte Limit,
    OrderRequestStatus? StatusFilter) : IRequest<IEnumerable<OrderRequestDto>>;