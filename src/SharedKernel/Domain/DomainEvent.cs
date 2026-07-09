namespace SharedKernel.Domain;

public abstract class DomainEvent<T> where T : TypedId
{
    public T AggregateId { get; init; }
    public DateTime OccurredOn  { get; init; }

    protected DomainEvent(T aggregateId)
    {
        AggregateId = aggregateId;
        OccurredOn = DateTime.UtcNow;
    }
}