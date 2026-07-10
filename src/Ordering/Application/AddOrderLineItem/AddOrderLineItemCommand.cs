using MediatR;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Application.AddOrderLineItem;

public record AddOrderLineItemCommand(OrderId Id, MenuItemRefId MenuItemRefId)
    : IRequest<Result<OrderId, Error>>;