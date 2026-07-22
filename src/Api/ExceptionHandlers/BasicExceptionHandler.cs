using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.ExceptionHandlers;

public class BasicExceptionHandler(ILogger<BasicExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, $"Unhandled exception: {exception.Message}");

        var status = exception switch
        {
            DbUpdateConcurrencyException => 409,
            _ => 500
        };

        var problemDetails = new ProblemDetails
        {
            Title = status == 500 ? "An unhandled exception occurred" : "Optimistic concurrency violation",
            Status = status,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}