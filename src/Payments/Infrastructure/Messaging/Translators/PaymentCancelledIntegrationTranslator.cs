using Payments.Domain.Events;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;
using SharedKernel.Infrastructure.Interceptors;

namespace Payments.Infrastructure.Messaging.Translators;

public class PaymentCancelledIntegrationTranslator : IIntegrationEventTranslator<PaymentCancelled>
{
    public IntegrationEvent Translate(PaymentCancelled context)
    {
        return new PaymentCancelledIntegration(context.OrderRefId.Id);
    }
}