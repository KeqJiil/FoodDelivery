using SharedKernel.Domain;

namespace Deliveries.Domain.Ids;

public record DeliveryId : TypedId
{
    public DeliveryId()
    {
    }

    public DeliveryId(Guid id)
        : base(id)
    {
    }
};
