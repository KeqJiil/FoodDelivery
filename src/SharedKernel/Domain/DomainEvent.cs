namespace SharedKernel.Domain;

public abstract record DomainEvent<T>(T AggregateId) where T : TypedId
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}