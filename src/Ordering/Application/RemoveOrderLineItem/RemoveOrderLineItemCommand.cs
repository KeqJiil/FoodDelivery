using MediatR;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.RemoveOrderLineItem;

public record RemoveOrderLineItemCommand(OrderId Id, OrderLineId OrderLineId) : IRequest<Result<OrderId, Error>>;