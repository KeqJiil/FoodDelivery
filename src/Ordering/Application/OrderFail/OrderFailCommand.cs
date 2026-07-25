using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.OrderFail;

public record OrderFailCommand(Guid OrderId) : IRequest<Result<Error>>;