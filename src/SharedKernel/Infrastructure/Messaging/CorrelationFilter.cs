using MassTransit;

namespace SharedKernel.Infrastructure.Messaging;

public class CorrelationSendFilter<T> : IFilter<SendContext<T>> where T : class
{
    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        var correlationId = CorrelationContext.CorrelationId ?? Guid.NewGuid().ToString();
        context.Headers.Set("X-Correlation-Id", correlationId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlation");
    }
}

public class CorrelationPublishFilter<T> : IFilter<PublishContext<T>> where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        var correlationId = CorrelationContext.CorrelationId ?? Guid.NewGuid().ToString();
        context.Headers.Set("X-Correlation-Id", correlationId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlation");
    }
}

public class CorrelationConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId = context.Headers.Get<string>("X-Correlation-Id") ?? Guid.NewGuid().ToString();

        CorrelationContext.CorrelationId = correlationId;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlation");
    }
}