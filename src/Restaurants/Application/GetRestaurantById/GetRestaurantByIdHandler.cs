using MediatR;
using Restaurants.Application.Abstractions;

namespace Restaurants.Application.GetRestaurantById;

public class GetRestaurantByIdHandler(IRestaurantReader reader)
    : IRequestHandler<GetRestaurantByIdQuery, RestaurantDto?>
{
    public async Task<RestaurantDto?> Handle(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
    {
        return await reader.GetByIdAsync(request.Id, cancellationToken);
    }
}