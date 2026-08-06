using System.Text.Json;
using Api.ExceptionHandlers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Moq;
using SharedKernel.Infrastructure.Messaging;

namespace Api.UnitTest.ExceptionHandlers;

public class BasicExceptionHandlerTests
{
    private readonly BasicExceptionHandler _handler = CreateHandler(Environments.Development);

    private static BasicExceptionHandler CreateHandler(string environmentName)
    {
        var environment = Mock.Of<IHostEnvironment>(e => e.EnvironmentName == environmentName);
        return new BasicExceptionHandler(Mock.Of<Microsoft.Extensions.Logging.ILogger<BasicExceptionHandler>>(),
            environment);
    }

    private static DefaultHttpContext CreateContext(string path = "/api/orders")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ProblemDetails> ReadBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        problem.Should().NotBeNull();
        return problem!;
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnTrue_SoThePipelineStopsHere()
    {
        var handled = await _handler.TryHandleAsync(CreateContext(), new Exception("boom"), CancellationToken.None);

        handled.Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_ShouldMapConcurrencyException_To409()
    {
        var context = CreateContext();

        await _handler.TryHandleAsync(context, new DbUpdateConcurrencyException("row version mismatch"),
            CancellationToken.None);

        context.Response.StatusCode.Should().Be(409);
        var problem = await ReadBody(context);
        problem.Status.Should().Be(409);
        problem.Title.Should().Be("Optimistic concurrency violation");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldMapUnknownException_To500()
    {
        var context = CreateContext();

        await _handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        context.Response.StatusCode.Should().Be(500);
        var problem = await ReadBody(context);
        problem.Status.Should().Be(500);
        problem.Title.Should().Be("An unhandled exception occurred");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldMapPlainDbUpdateException_To500()
    {
        // Only the concurrency subtype is special-cased; a generic persistence failure stays a 500.
        var context = CreateContext();

        await _handler.TryHandleAsync(context, new DbUpdateException("constraint violated"), CancellationToken.None);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeExceptionMessage_AsDetail_InDevelopment()
    {
        var context = CreateContext();

        await _handler.TryHandleAsync(context, new InvalidOperationException("connection string is bad"),
            CancellationToken.None);

        var problem = await ReadBody(context);
        problem.Detail.Should().Be("connection string is bad");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldHideExceptionMessage_OutsideDevelopment()
    {
        var handler = CreateHandler(Environments.Production);
        var context = CreateContext();

        await handler.TryHandleAsync(context, new InvalidOperationException("connection string is bad"),
            CancellationToken.None);

        var problem = await ReadBody(context);
        problem.Detail.Should().NotBeNullOrEmpty().And.NotContain("connection string");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldHideConcurrencyExceptionMessage_OutsideDevelopment()
    {
        var handler = CreateHandler(Environments.Production);
        var context = CreateContext();

        await handler.TryHandleAsync(context, new DbUpdateConcurrencyException("row version mismatch"),
            CancellationToken.None);

        var problem = await ReadBody(context);
        problem.Detail.Should().NotBeNullOrEmpty().And.NotContain("row version mismatch");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeCorrelationId_InExtensions()
    {
        var context = CreateContext();
        CorrelationContext.CorrelationId = "test-correlation-id";

        await _handler.TryHandleAsync(context, new Exception("boom"), CancellationToken.None);

        var problem = await ReadBody(context);
        problem.Extensions["correlationId"]!.ToString().Should().Be("test-correlation-id");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldUseRequestPath_AsInstance()
    {
        var context = CreateContext("/api/payments/17");

        await _handler.TryHandleAsync(context, new Exception("boom"), CancellationToken.None);

        var problem = await ReadBody(context);
        problem.Instance.Should().Be("/api/payments/17");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldStillRespond_WhenExceptionMessageIsEmpty()
    {
        var context = CreateContext();

        var handled = await _handler.TryHandleAsync(context, new Exception(string.Empty), CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldHandleAggregateException_As500()
    {
        var context = CreateContext();
        var exception = new AggregateException(new DbUpdateConcurrencyException("inner"));

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // The switch matches the outer type only, so a wrapped concurrency failure is not detected as 409.
        context.Response.StatusCode.Should().Be(500);
    }
}
