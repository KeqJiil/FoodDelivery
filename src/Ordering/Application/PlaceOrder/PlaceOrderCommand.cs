using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using Ordering.Domain.Ids;

namespace Ordering.Application.PlaceOrder;

public record PlaceOrderCommand(OrderId OrderId) : IRequest<Result<OrderId, Error>>;