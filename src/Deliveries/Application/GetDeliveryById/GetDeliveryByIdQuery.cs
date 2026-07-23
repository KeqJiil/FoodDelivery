using MediatR;

namespace Deliveries.Application.GetDeliveryById;

public record GetDeliveryByIdQuery(Guid Id) : IRequest<DeliveryDto?>;
