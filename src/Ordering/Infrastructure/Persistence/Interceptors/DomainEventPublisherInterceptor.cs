using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Domain;

namespace Ordering.Infrastructure.Persistence.Interceptors;

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
                await publishEndpoint.Publish(domainEvent, domainEvent.GetType(), ct);

            aggregate.ClearDomainEvents();
        }
    }
}
