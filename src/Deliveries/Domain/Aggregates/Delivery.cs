using Deliveries.Domain.Enums;
using Deliveries.Domain.Events;
using Deliveries.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Deliveries.Domain.Aggregates;

public class Delivery : AggregateRoot<DeliveryId>
{
    public DeliveryId Id { get; }
    public OrderRefId OrderRefId { get; }
    public DeliveryStatus Status { get; private set; }
    public string? FailureReason { get; private set; }

    private Delivery(DeliveryId id, OrderRefId orderRefId)
    {
        Id = id;
        OrderRefId = orderRefId;
        Status = DeliveryStatus.Pending;
    }

    private Delivery()
    {
    }

    public static Delivery Create(DeliveryId id, OrderRefId orderRefId)
    {
        var delivery = new Delivery(id, orderRefId);
        delivery.AddEvent(new DeliveryCreated(id));
        return delivery;
    }

    public Result<Error> MarkPickedUp()
    {
        if (Status != DeliveryStatus.Pending)
            return Result<Error>.Fail(Error.Conflict($"Cannot pick up a delivery that is already {Status}"));

        Status = DeliveryStatus.PickedUp;
        AddEvent(new DeliveryPickedUp(Id));
        return Result<Error>.Success();
    }

    public Result<Error> Complete()
    {
        if (Status != DeliveryStatus.PickedUp)
            return Result<Error>.Fail(Error.Conflict($"Cannot complete a delivery that is {Status}"));

        Status = DeliveryStatus.Delivered;
        AddEvent(new DeliveryCompleted(Id, OrderRefId));
        return Result<Error>.Success();
    }

    public Result<Error> Fail(string reason)
    {
        if (Status is not (DeliveryStatus.Pending or DeliveryStatus.PickedUp))
            return Result<Error>.Fail(Error.Conflict($"Cannot fail a delivery that is already {Status}"));

        Status = DeliveryStatus.Failed;
        FailureReason = reason;
        AddEvent(new DeliveryFailed(Id, OrderRefId, reason));
        return Result<Error>.Success();
    }
}
