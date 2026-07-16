using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using Ordering.Domain.Ids;

namespace Ordering.Application.RemoveOrderLineItem;

public record RemoveOrderLineItemCommand(OrderId Id, OrderLineId OrderLineId) : IRequest<Result<OrderId, Error>>;