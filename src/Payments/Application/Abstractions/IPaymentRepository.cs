using Payments.Domain.Aggregates;
using Payments.Domain.Ids;

namespace Payments.Application.Abstractions;

public interface IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default);
    public void Add(Payment payment);
}
