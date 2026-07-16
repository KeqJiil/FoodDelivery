using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Domain;

namespace SharedKernel.Infrastructure.Interceptors;

public sealed class DomainEventPublishInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await PublishDomainEventsAsync(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishDomainEventsAsync(DbContext context, CancellationToken ct)
    {
        var aggregates = context.ChangeTracker.Entries<IHasDomainEvents>().Select(e => e.Entity)
            .Where(e => e.GetDomainEvents().Count > 0).ToList();

        if (aggregates.Count == 0) return;

        var publishEndpoint = serviceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var aggregate in aggregates)
        {
            var domainEvents = aggregate.GetDomainEvents().ToList();
            foreach (var domainEvent in domainEvents)
            {
                var integrationEvent = TranslateToIntegrationEvent(domainEvent);
                if (integrationEvent is not null)
                    await publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), ct);
            }

            aggregate.ClearDomainEvents();
        }
    }

    private object? TranslateToIntegrationEvent(object domainEvent)
    {
        var translatorType = typeof(IIntegrationEventTranslator<>).MakeGenericType(domainEvent.GetType());
        var translator = serviceProvider.GetService(translatorType);
        if (translator is null) return null;

        return translatorType.GetMethod(nameof(IIntegrationEventTranslator<object>.Translate))!
            .Invoke(translator, [domainEvent]);
    }
}