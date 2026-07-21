using Payments.Application.GetPaymentById;

namespace Payments.Application.Abstractions;

public interface IPaymentReader
{
    public Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
