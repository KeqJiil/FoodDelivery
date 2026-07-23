using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Ids;

namespace Deliveries.Application.Abstractions;

public interface IDeliveryRepository
{
    public Task<Delivery?> GetByIdAsync(DeliveryId id, CancellationToken cancellationToken = default);
    public void Add(Delivery delivery);
}
