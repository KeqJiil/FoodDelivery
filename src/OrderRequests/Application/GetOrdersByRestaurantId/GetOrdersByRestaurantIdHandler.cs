using MediatR;
using OrderRequests.Application.Abstractions;
using OrderRequests.Application.GetOrderRequestById;

namespace OrderRequests.Application.GetOrdersByRestaurantId;

public class GetOrdersByRestaurantIdHandler(IOrderRequestReader reader)
    : IRequestHandler<GetOrdersByRestaurantIdQuery, IEnumerable<OrderRequestDto>>
{
    public async Task<IEnumerable<OrderRequestDto>> Handle(GetOrdersByRestaurantIdQuery query,
        CancellationToken cancellationToken)
    {
        return await reader.GetAllByRestaurantIdAsync(query.RestaurantId, query.Cursor, query.Limit,
            query.StatusFilter, cancellationToken);
    }
}