using Microsoft.Extensions.Logging;
using Payments.Application.Abstractions;
using SharedKernel.Domain.ValueObjects;

namespace Payments.Infrastructure.Adapters;

public sealed class MockPaymentGatewayAdapter(ILogger<MockPaymentGatewayAdapter> logger) : IPaymentGatewayAdapter
{
    public Task<PaymentGatewayResult> ChargeAsync(Guid paymentId, Money amount,
        CancellationToken cancellationToken = default)
    {
        var cents = (int)(amount.Amount * 100m % 100m);
        var succeeded = cents != 13;

        logger.LogInformation("Mock gateway charged {Amount} {Currency} for payment {PaymentId}: {Result}",
            amount.Amount, amount.Currency, paymentId, succeeded ? "Succeeded" : "Declined");

        return Task.FromResult(succeeded
            ? new PaymentGatewayResult(true, null)
            : new PaymentGatewayResult(false, "Mock gateway declined the charge"));
    }
}
