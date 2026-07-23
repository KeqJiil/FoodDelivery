using Deliveries.Application.GetDeliveryById;

namespace Deliveries.Application.Abstractions;

public interface IDeliveryReader
{
    public Task<DeliveryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
