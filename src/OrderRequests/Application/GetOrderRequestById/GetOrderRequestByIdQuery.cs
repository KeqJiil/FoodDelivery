using MediatR;

namespace OrderRequests.Application.GetOrderRequestById;

public record GetOrderRequestByIdQuery(Guid Id) : IRequest<OrderRequestDto?>;