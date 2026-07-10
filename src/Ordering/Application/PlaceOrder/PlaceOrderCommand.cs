using MediatR;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.PlaceOrder;

public record PlaceOrderCommand(OrderId OrderId) : IRequest<Result<OrderId, Error>>;