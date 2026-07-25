using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.CancelOrder;

public record CancelOrderCommand(Guid Id) : IRequest<Result<Error>>;