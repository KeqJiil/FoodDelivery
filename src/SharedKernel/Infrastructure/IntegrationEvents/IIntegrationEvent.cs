namespace SharedKernel.Infrastructure.IntegrationEvents;

public abstract record IntegrationEvent
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}