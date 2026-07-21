using SharedKernel.Domain.ValueObjects;

namespace Payments.Application.Abstractions;

public interface IPaymentGatewayAdapter
{
    public Task<PaymentGatewayResult> ChargeAsync(Guid paymentId, Money amount,
        CancellationToken cancellationToken = default);
}

public record PaymentGatewayResult(bool Succeeded, string? FailureReason);
