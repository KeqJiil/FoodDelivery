namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record DeliveryCompletedIntegration(Guid OrderId) : IntegrationEvent;
