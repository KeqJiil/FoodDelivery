using MediatR;
using Restaurants.Application.GetRestaurantById;
using SharedKernel.Domain;

namespace Restaurants.Application.GetRestaurantsList;

public record GetRestaurantsListQuery(int Page, int PageSize) : IRequest<RestaurantsListPagination>;

public record RestaurantsListPagination(
    IReadOnlyList<RestaurantDto> Restaurants,
    int Page,
    int PageSize,
    bool HasNextPage,
    bool HasPreviousPage) : PagedResult<RestaurantDto>(Restaurants, Page, PageSize, HasNextPage, HasPreviousPage);