namespace SharedKernel.Domain;

public abstract class DomainEvent<T>(T aggregateId) where T : TypedId
{
    public T AggregateId { get; init; } = aggregateId;
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}