using MediatR;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.CancelOrder;

public record CancelOrderCommand(OrderId Id) : IRequest<Result<OrderId, Error>>;