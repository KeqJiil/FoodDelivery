using MediatR;
using Payments.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Payments.Application.CreatePayment;

public record CreatePaymentCommand(OrderRefId OrderRefId, decimal Amount, Currency Currency)
    : IRequest<Result<PaymentId, Error>>;
