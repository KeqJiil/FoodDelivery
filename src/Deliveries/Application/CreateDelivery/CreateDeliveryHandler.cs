using Deliveries.Application.Abstractions;
using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Ids;
using MediatR;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Application.CreateDelivery;

public class CreateDeliveryHandler(IDeliveryRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDeliveryCommand, Result<DeliveryId, Error>>
{
    public async Task<Result<DeliveryId, Error>> Handle(CreateDeliveryCommand request, CancellationToken ct)
    {
        var delivery = Delivery.Create(new DeliveryId(), request.OrderRefId);

        repository.Add(delivery);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<DeliveryId, Error>.Success(delivery.Id);
    }
}
