using System.Collections.Concurrent;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDelivery.IntegrationTest.Messaging;

public record RetryProbe(Guid Id);

public record RetryProbeSideEffect(Guid Id);

public class RetryProbeConsumer : IConsumer<RetryProbe>
{
    public static readonly ConcurrentDictionary<Guid, int> Attempts = new();

    public static int FailUntilAttempt = int.MaxValue;

    public async Task Consume(ConsumeContext<RetryProbe> context)
    {
        var attempt = Attempts.AddOrUpdate(context.Message.Id, 1, (_, count) => count + 1);

        await context.Publish(new RetryProbeSideEffect(context.Message.Id));

        if (attempt < FailUntilAttempt)
            throw new InvalidOperationException($"Simulated failure on attempt {attempt}");
    }
}

[Collection("Database")]
public class ConsumerRetryTests
{
    private async Task<(ServiceProvider Provider, ITestHarness Harness)> StartHarnessAsync()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<RetryProbeConsumer>();
            x.UsingInMemory((context, cfg) =>
            {
                cfg.UseMessageRetry(r => r.Immediate(5));
                cfg.UseInMemoryOutbox(context);
                cfg.ConfigureEndpoints(context);
            });
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(10);
        await harness.Start();
        return (provider, harness);
    }

    [Fact]
    public async Task TransientFailure_ShouldRetryImmediately_AndEventuallySucceedWithoutFaulting()
    {
        RetryProbeConsumer.FailUntilAttempt = 3;
        var (provider, harness) = await StartHarnessAsync();
        await using var _ = provider;

        var messageId = Guid.CreateVersion7();
        await harness.Bus.Publish(new RetryProbe(messageId));

        (await harness.Consumed.Any<RetryProbe>(m => m.Context.Message.Id == messageId && m.Exception is null))
            .Should().BeTrue();
        (await harness.Published.Any<Fault<RetryProbe>>()).Should().BeFalse();
        RetryProbeConsumer.Attempts[messageId].Should().Be(3);
    }

    [Fact]
    public async Task PermanentFailure_ShouldExhaustRetriesAndFault()
    {
        RetryProbeConsumer.FailUntilAttempt = int.MaxValue;
        var (provider, harness) = await StartHarnessAsync();
        await using var _ = provider;

        var messageId = Guid.CreateVersion7();
        await harness.Bus.Publish(new RetryProbe(messageId));

        (await harness.Published.Any<Fault<RetryProbe>>(m => m.Context.Message.Message!.Id == messageId))
            .Should().BeTrue();
        RetryProbeConsumer.Attempts[messageId].Should().Be(6);
    }

    [Fact]
    public async Task FailedAttempt_ShouldNotPublishItsSideEffects_ThanksToInMemoryOutbox()
    {
        RetryProbeConsumer.FailUntilAttempt = 3;
        var (provider, harness) = await StartHarnessAsync();
        await using var _ = provider;

        var messageId = Guid.CreateVersion7();
        await harness.Bus.Publish(new RetryProbe(messageId));

        (await harness.Consumed.Any<RetryProbe>(m => m.Context.Message.Id == messageId && m.Exception is null))
            .Should().BeTrue();

        (await harness.Published.SelectAsync<RetryProbeSideEffect>()
                .Where(m => m.Context.Message.Id == messageId).CountAsync())
            .Should().Be(1);
    }
}
