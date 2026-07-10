using MediatR;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.ConfirmOrder;

public record ConfirmOrderCommand(OrderId Id) : IRequest<Result<OrderId, Error>>;