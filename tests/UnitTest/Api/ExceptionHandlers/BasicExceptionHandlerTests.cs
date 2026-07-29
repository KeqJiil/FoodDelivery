using System.Text.Json;
using Api.ExceptionHandlers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Api.UnitTest.ExceptionHandlers;

public class BasicExceptionHandlerTests
{
    private readonly BasicExceptionHandler _handler =
        new(Mock.Of<Microsoft.Extensions.Logging.ILogger<BasicExceptionHandler>>());

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
    public async Task TryHandleAsync_ShouldLeakExceptionMessage_AsDetail()
    {
        var context = CreateContext();

        await _handler.TryHandleAsync(context, new InvalidOperationException("connection string is bad"),
            CancellationToken.None);

        var problem = await ReadBody(context);
        problem.Detail.Should().Be("connection string is bad");
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
