using Ordering.Domain.Ids;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Application.Abstractions;

public interface IRestaurantMinimumOrderPriceAdapter
{
    public Task<Money?> GetMinimumPriceForOrderAsync(RestaurantRefId id, CancellationToken cancellationToken = default);
}