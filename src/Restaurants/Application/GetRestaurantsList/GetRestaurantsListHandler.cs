using MediatR;
using Restaurants.Application.Abstractions;

namespace Restaurants.Application.GetRestaurantsList;

public class GetRestaurantsListHandler(IRestaurantReader reader)
    : IRequestHandler<GetRestaurantsListQuery, RestaurantsListPagination>
{
    public Task<RestaurantsListPagination> Handle(GetRestaurantsListQuery request, CancellationToken ct)
    {
        return reader.GetRestaurants(request.Page, request.PageSize, ct);
    }
}