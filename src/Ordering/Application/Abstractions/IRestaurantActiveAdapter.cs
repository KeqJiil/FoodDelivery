using Ordering.Domain.Ids;

namespace Ordering.Application.Abstractions;

public interface IRestaurantActiveAdapter
{
    Task<bool> IsActiveAsync(RestaurantRefId id, CancellationToken cancellationToken = default);
}