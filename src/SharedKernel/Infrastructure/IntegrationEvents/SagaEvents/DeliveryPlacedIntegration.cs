using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public record DeliveryPlacedIntegration(Guid OrderId) : IntegrationEvent;