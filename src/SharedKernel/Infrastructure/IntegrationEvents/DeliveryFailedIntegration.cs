namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record DeliveryFailedIntegration(Guid OrderId, string Reason) : IntegrationEvent;
