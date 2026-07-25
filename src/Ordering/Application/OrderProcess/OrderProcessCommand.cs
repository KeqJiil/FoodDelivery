using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.OrderProcess;

public record OrderProcessCommand(Guid OrderId) : IRequest<Result<Error>>;