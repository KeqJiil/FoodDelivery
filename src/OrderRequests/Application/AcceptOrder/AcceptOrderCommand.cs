using MediatR;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.AcceptOrder;

public record AcceptOrderCommand(OrderRequestId Id) : IRequest<Result<Error>>;