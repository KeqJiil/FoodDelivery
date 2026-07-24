using Deliveries.Application.Abstractions;
using Deliveries.Application.GetDeliveryById;
using Microsoft.EntityFrameworkCore;

namespace Deliveries.Infrastructure.Persistence.Readers;

public class DeliveryReader(DeliveriesDbContext context) : IDeliveryReader
{
    public async Task<DeliveryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Deliveries.AsNoTracking().Where(x => x.Id.Id == id).Select(x =>
            new DeliveryDto(x.Id.Id, x.OrderRefId.Id, x.Status, x.FailureReason,
                EF.Property<DateTime>(x, "CreatedAt"))).FirstOrDefaultAsync(cancellationToken);
    }
}
