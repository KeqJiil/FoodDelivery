using SharedKernel.Domain.ValueObjects;
using Ordering.Domain.Ids;

namespace Ordering.Application.Abstractions;

public interface IRestaurantMinimumOrderPriceAdapter
{
    public Task<Money?> GetMinimumPriceForOrderAsync(RestaurantRefId id, CancellationToken cancellationToken = default);
}