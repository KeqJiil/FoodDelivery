using MediatR;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Ordering.Application.CreateOrder;

public record CreateOrderCommand(RestaurantRefId RestaurantRefId) : IRequest<Result<OrderId, Error>>;