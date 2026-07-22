using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Domain.Aggregates;
using Payments.Domain.Ids;

namespace Payments.Infrastructure.Persistence.Repositories;

public class PaymentRepository(PaymentsDbContext ctx) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        return await ctx.Payments.Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Payment payment)
    {
        ctx.Add(payment);
    }
}
