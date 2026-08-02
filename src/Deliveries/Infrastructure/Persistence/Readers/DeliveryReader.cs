using Deliveries.Application.Abstractions;
using Deliveries.Application.GetDeliveryById;
using Deliveries.Domain.Ids;
using Microsoft.EntityFrameworkCore;

namespace Deliveries.Infrastructure.Persistence.Readers;

public class DeliveryReader(DeliveriesDbContext context) : IDeliveryReader
{
    public async Task<DeliveryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deliveryId = new DeliveryId(id);
        return await context.Deliveries.AsNoTracking().Where(x => x.Id == deliveryId).Select(x =>
            new DeliveryDto(x.Id.Id, x.OrderRefId.Id, x.Status, x.FailureReason,
                EF.Property<DateTime>(x, "CreatedAt"))).FirstOrDefaultAsync(cancellationToken);
    }
}
