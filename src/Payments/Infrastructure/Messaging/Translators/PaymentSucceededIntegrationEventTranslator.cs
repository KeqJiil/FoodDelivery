using Payments.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Payments.Infrastructure.Messaging.Translators;

public class PaymentSucceededIntegrationEventTranslator : IIntegrationEventTranslator<PaymentSucceeded>
{
    public IntegrationEvent? Translate(PaymentSucceeded domainEvent)
    {
        return new PaymentSucceededIntegration(domainEvent.OrderRefId.Id);
    }
}