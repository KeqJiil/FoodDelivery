using MediatR;

namespace Ordering.Application.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto?>;