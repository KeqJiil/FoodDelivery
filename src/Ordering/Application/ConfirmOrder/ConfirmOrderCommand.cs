using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using Ordering.Domain.Ids;

namespace Ordering.Application.ConfirmOrder;

public record ConfirmOrderCommand(OrderId Id) : IRequest<Result<OrderId, Error>>;