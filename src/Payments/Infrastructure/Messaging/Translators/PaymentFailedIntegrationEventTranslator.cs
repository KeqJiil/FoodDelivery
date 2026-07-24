using Payments.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Payments.Infrastructure.Messaging.Translators;

public class PaymentFailedIntegrationEventTranslator : IIntegrationEventTranslator<PaymentFailed>
{
    public IntegrationEvent? Translate(PaymentFailed domainEvent)
    {
        return new PaymentFailedIntegration(domainEvent.OrderRefId.Id, domainEvent.Reason);
    }
}
