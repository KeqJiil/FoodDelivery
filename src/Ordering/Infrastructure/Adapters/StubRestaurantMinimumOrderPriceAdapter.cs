using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Infrastructure.Adapters;

public sealed class StubRestaurantMinimumOrderPriceAdapter : IRestaurantMinimumOrderPriceAdapter
{
    public Task<Money?> GetMinimumPriceForOrderAsync(RestaurantRefId id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Money?>(new Money(Currency.Usd, 0m));
    }
}
