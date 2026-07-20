using MediatR;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Domain.Enums;

namespace OrderRequests.Application.GetOrdersByRestaurantId;

public record GetOrdersByRestaurantIdQuery(
    Guid RestaurantId,
    Guid? Cursor,
    byte Limit,
    OrderRequestStatus? StatusFilter) : IRequest<IEnumerable<OrderRequestDto>>;