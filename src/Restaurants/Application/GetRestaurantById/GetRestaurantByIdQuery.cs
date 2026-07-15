using MediatR;

namespace Restaurants.Application.GetRestaurantById;

public record GetRestaurantByIdQuery(Guid Id) : IRequest<RestaurantDto?>;
