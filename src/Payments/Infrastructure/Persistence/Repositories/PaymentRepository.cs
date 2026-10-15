using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Domain.Aggregates;
using Payments.Domain.Ids;

namespace Payments.Infrastructure.Persistence.Repositories;

public class PaymentRepository(DbContext ctx) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        return await ctx.Set<Payment>().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Payment?> GetByOrderIdAsync(OrderRefId id, CancellationToken cancellationToken = default)
    {
        return await ctx.Set<Payment>().Where(x => x.OrderRefId == id).FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Payment payment)
    {
        ctx.Add(payment);
    }
}