using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Application.GetPaymentById;

namespace Payments.Infrastructure.Persistence.Readers;

public class PaymentReader(PaymentsDbContext context) : IPaymentReader
{
    public async Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Payments.AsNoTracking().Where(x => x.Id.Id == id).Select(x =>
            new PaymentDto(x.Id.Id, x.OrderRefId.Id, x.Amount, x.Status, x.FailureReason,
                EF.Property<DateTime>(x, "CreatedAt"))).FirstOrDefaultAsync(cancellationToken);
    }
}
