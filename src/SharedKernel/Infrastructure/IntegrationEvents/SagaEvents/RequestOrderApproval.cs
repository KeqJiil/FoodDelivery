using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;

namespace SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

public record RequestOrderApproval(Guid OrderId, Guid RestaurantId) : IntegrationEvent;