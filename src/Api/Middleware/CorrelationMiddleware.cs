using SharedKernel.Infrastructure.Messaging;

namespace Api.Middleware;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers.TryGetValue("x-correlation-id", out var value) && !string.IsNullOrEmpty(value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers["x-correlation-id"] = id;
        
        CorrelationContext.CorrelationId = id;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", id))
        {
            await _next(context);
        }
    }
}