using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;
using Ordering.Application.Abstractions;
using Ordering.Domain.Ids;

namespace Ordering.Infrastructure.Adapters;

public sealed class StubRestaurantMinimumOrderPriceAdapter : IRestaurantMinimumOrderPriceAdapter
{
    public Task<Money?> GetMinimumPriceForOrderAsync(RestaurantRefId id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Money?>(Money.Create(Currency.Usd, 0m).Ok);
    }
}