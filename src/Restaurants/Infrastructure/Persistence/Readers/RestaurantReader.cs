using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Restaurants.Application.Abstractions;
using Restaurants.Application.GetRestaurantById;
using Restaurants.Application.GetRestaurantsList;
using Restaurants.Domain.Aggregates;
using Restaurants.Domain.Entities;
using Restaurants.Domain.Enums;
using Restaurants.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Infrastructure.Persistence.Readers;

public class RestaurantReader(RestaurantsDbContext context, IClock clock) : IRestaurantReader
{
    public async Task<RestaurantsListPagination> GetRestaurants(int page, int pageSize, CancellationToken ct)
    {
        var skip = (page - 1) * pageSize;
        var currentPage = await context.Restaurants.AsNoTracking()
            .Where(r => r.Status == RestaurantStatus.Active)
            .Where(IsOpenNowExpression(clock.Now))
            .Select(r => new RestaurantDto(
                r.Id.Id,
                r.Name.Data,
                r.Description.Data,
                r.MinimalOrderPrice,
                r.Status,
                r.Schedule.OpeningWindows,
                r.MenuItems.Select(m => new MenuItemDto(m.Id.Id, m.Name.Data, m.Description.Data, m.Price)).ToList()))
            .Skip(skip)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        var hasNextPage = currentPage.Count > pageSize;
        var hasPreviousPage = page > 1;

        return new RestaurantsListPagination(currentPage.Take(pageSize).ToList(), page, pageSize, hasNextPage,
            hasPreviousPage);
    }

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

    public async Task<Money?> GetMenuItemPriceByIdAsync(Guid menuItemId, CancellationToken cancellationToken = default)
    {
        var id = new MenuItemId(menuItemId);
        return await context.Set<MenuItem>().AsNoTracking().Where(m => m.Id == id).Select(m => m.Price)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool?> IsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restaurantId = new RestaurantId(id);
        var restaurant = await context.Restaurants.AsNoTracking().Where(x => x.Id == restaurantId)
            .FirstOrDefaultAsync(cancellationToken);

        return restaurant is not null && restaurant.IsActive() && restaurant.IsOpen(clock.Now);
    }

    private static Expression<Func<Restaurant, bool>> IsOpenNowExpression(DateTimeOffset now)
    {
        var nowWeekMinutes = (int)now.DayOfWeek * 1440 + now.Hour * 60 + now.Minute;

        return r => r.Schedule.OpeningWindows.Any(w =>
            (int)w.OpenDay * 1440 + w.OpenTime.Hour * 60 + w.OpenTime.Minute <=
            (int)w.CloseDay * 1440 + w.CloseTime.Hour * 60 + w.CloseTime.Minute
                ? nowWeekMinutes >= (int)w.OpenDay * 1440 + w.OpenTime.Hour * 60 + w.OpenTime.Minute
                  && nowWeekMinutes < (int)w.CloseDay * 1440 + w.CloseTime.Hour * 60 + w.CloseTime.Minute
                : nowWeekMinutes >= (int)w.OpenDay * 1440 + w.OpenTime.Hour * 60 + w.OpenTime.Minute
                  || nowWeekMinutes < (int)w.CloseDay * 1440 + w.CloseTime.Hour * 60 + w.CloseTime.Minute);
    }
}