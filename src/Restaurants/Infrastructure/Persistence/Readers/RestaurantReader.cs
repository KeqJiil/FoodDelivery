using Microsoft.EntityFrameworkCore;
using Restaurants.Application.Abstractions;
using Restaurants.Application.GetRestaurantById;
using Restaurants.Domain.Ids;

namespace Restaurants.Infrastructure.Persistence.Readers;

public class RestaurantReader(RestaurantsDbContext context) : IRestaurantReader
{
    public async Task<RestaurantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restaurantId = new RestaurantId(id);
        return await context.Restaurants.AsNoTracking().Where(r => r.Id == restaurantId).Select(r => new RestaurantDto(
            r.Id.Id,
            r.Name.Data,
            r.Description.Data,
            r.MinimalOrderPrice,
            r.Status,
            r.Schedule.OpeningWindows,
            r.MenuItems.Select(m => new MenuItemDto(m.Id.Id, m.Name.Data, m.Description.Data, m.Price)).ToList()
        )).FirstOrDefaultAsync(cancellationToken);
    }
}
