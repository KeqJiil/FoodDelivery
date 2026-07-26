using System.Windows.Input;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Payments.Application.CancelPayment;

public record CancelPaymentByOrderIdCommand(Guid OrderId) : IRequest<Result<Error>>;