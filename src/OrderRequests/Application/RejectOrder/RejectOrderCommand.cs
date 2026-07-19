using MediatR;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.RejectOrder;

public record RejectOrderCommand(OrderRequestId Id) : IRequest<Result<Error>>;