using Deliveries.Application.Abstractions;
using MediatR;

namespace Deliveries.Application.GetDeliveryById;

public class GetDeliveryByIdHandler(IDeliveryReader reader) : IRequestHandler<GetDeliveryByIdQuery, DeliveryDto?>
{
    public Task<DeliveryDto?> Handle(GetDeliveryByIdQuery request, CancellationToken cancellationToken)
    {
        return reader.GetByIdAsync(request.Id, cancellationToken);
    }
}
