using MediatR;
using Ordering.Domain.Aggregates;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using Ordering.Domain.Ids;

namespace Ordering.Application.CreateOrder;

public record CreateOrderCommand(RestaurantRefId RestaurantRefId) : IRequest<Result<OrderId, Error>>;