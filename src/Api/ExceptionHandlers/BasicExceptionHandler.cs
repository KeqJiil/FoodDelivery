using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Messaging;

namespace Api.ExceptionHandlers;

public class BasicExceptionHandler(ILogger<BasicExceptionHandler> logger, IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        var (status, title, detail) = exception switch
        {
            DbUpdateConcurrencyException => (409, "Optimistic concurrency violation",
                "The record was modified by another request. Please retry."),
            _ => (500, "An unhandled exception occurred", "An unexpected error occurred.")
        };

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = status,
            Detail = environment.IsDevelopment() ? exception.Message : detail,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["correlationId"] = CorrelationContext.CorrelationId;

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}