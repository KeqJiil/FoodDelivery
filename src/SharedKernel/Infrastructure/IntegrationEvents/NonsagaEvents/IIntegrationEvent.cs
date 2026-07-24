namespace SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

public abstract record IntegrationEvent
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}