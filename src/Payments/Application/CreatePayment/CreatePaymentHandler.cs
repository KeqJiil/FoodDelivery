using MediatR;
using Microsoft.Extensions.Logging;
using Payments.Application.Abstractions;
using Payments.Domain.Aggregates;
using Payments.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Payments.Application.CreatePayment;

public class CreatePaymentHandler(
    IPaymentGatewayAdapter paymentGateway,
    IPaymentRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<CreatePaymentHandler> logger)
    : IRequestHandler<CreatePaymentCommand, Result<PaymentId, Error>>
{
    public async Task<Result<PaymentId, Error>> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        var amountResult = Money.Create(request.Currency, request.Amount);
        if (!amountResult.IsSuccess)
            return Result<PaymentId, Error>.Fail(amountResult.Error!);
        var amount = amountResult.Ok!;

        var payment = Payment.Create(new PaymentId(), request.OrderRefId, amount);

        var gatewayResult = await paymentGateway.ChargeAsync(payment.Id.Id, amount, ct);
        if (gatewayResult.Succeeded)
            payment.Succeed();
        else
            payment.Fail(gatewayResult.FailureReason ?? "Payment declined");

        repository.Add(payment);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Payment {PaymentId} for order {OrderId} completed with status {Status}",
            payment.Id, request.OrderRefId, payment.Status);

        return Result<PaymentId, Error>.Success(payment.Id);
    }
}
