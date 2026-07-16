using Microsoft.EntityFrameworkCore;
using Restaurants.Application.Abstractions;
using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Ids;

namespace Restaurants.Infrastructure.Persistence.Repositories;

public class RestaurantRepository(RestaurantsDbContext context) : IRestaurantRepository
{
    public async Task<Restaurant?> GetById(RestaurantId id, CancellationToken cancellationToken = default)
    {
        return await context.Restaurants.Where(r => r.Id == id).Include(r => r.MenuItems)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Restaurant restaurant)
    {
        context.Restaurants.Add(restaurant);
    }
}