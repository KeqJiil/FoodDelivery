using MassTransit;

namespace SharedKernel.Infrastructure.Messaging;

public class CorrelationSendObserver : ISendObserver
{
    public Task PreSend<T>(SendContext<T> context) where T : class
    {
        var correlationId = CorrelationContext.CorrelationId ?? Guid.NewGuid().ToString();
        context.Headers.Set("X-Correlation-Id", correlationId);

        return Task.CompletedTask;
    }

    public Task PostSend<T>(SendContext<T> context) where T : class => Task.CompletedTask;

    public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class => Task.CompletedTask;
}

public class CorrelationPublishObserver : IPublishObserver
{
    public Task PrePublish<T>(PublishContext<T> context) where T : class
    {
        var correlationId = CorrelationContext.CorrelationId ?? Guid.NewGuid().ToString();
        context.Headers.Set("X-Correlation-Id", correlationId);

        return Task.CompletedTask;
    }

    public Task PostPublish<T>(PublishContext<T> context) where T : class => Task.CompletedTask;

    public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class => Task.CompletedTask;
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