using MediatR;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace OrderRequests.Application.CreateOrder;

public record CreateOrderCommand(OrderRefId OrderRefId, RestaurantRefId RestaurantRefId)
    : IRequest<Result<OrderRequestId, Error>>;