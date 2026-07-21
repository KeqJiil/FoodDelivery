namespace SharedKernel.Infrastructure.IntegrationEvents;

public sealed record PaymentFailedIntegration(Guid OrderId, string Reason) : IntegrationEvent;
