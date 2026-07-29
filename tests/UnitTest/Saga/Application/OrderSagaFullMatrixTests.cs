using FluentAssertions;
using MassTransit.Testing;
using Saga.Application;
using Saga.UnitTest.TestHelpers;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace Saga.UnitTest.Application;

public class OrderSagaFullMatrixTests : OrderSagaTestBase
{
    private static readonly string[] States =
    [
        "AwaitingApproval", "AwaitingProcessing", "AwaitingPayment", "AwaitingConfirmation",
        "AwaitingDelivery", "CompensatingRequest", "CompensatingOrder", "CompensatingPayment"
    ];

    private static readonly string[] Events =
    [
        "OrderPlaced", "OrderApproved", "OrderRejected", "OrderFailed", "OrderStartedProcessing",
        "OrderConfirmed", "OrderCancelled", "OrderRequestCancelled", "PaymentSucceeded",
        "PaymentFailed", "PaymentCancelled", "DeliveryPlaced"
    ];

    public static IEnumerable<object[]> Cells =>
        from state in States from evt in Events select new object[] { state, evt };

    private Task<Guid> Arrange(string stateName)
    {
        var id = Guid.NewGuid();
        Task<OrderState> Given() => stateName switch
        {
            "AwaitingApproval" => GivenAwaitingApproval(id),
            "AwaitingProcessing" => GivenAwaitingProcessing(id),
            "AwaitingPayment" => GivenAwaitingPayment(id),
            "AwaitingConfirmation" => GivenAwaitingConfirmation(id),
            "AwaitingDelivery" => GivenAwaitingDelivery(id),
            "CompensatingRequest" => GivenCompensatingRequest(id),
            "CompensatingOrder" => GivenCompensatingOrder(id),
            "CompensatingPayment" => GivenCompensatingPayment(id),
            _ => throw new ArgumentOutOfRangeException(nameof(stateName))
        };
        return Given().ContinueWith(_ => id);
    }
    
    private async Task<bool> PublishAndCheckFaulted(string eventName, Guid id)
    {
        switch (eventName)
        {
            case "OrderPlaced":
                await Harness.Bus.Publish(Placed(id));
                await SagaHarness.Consumed.Any<OrderPlacedIntegration>(x => x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<OrderPlacedIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            case "OrderApproved":
                await Harness.Bus.Publish(new OrderApprovedIntegration(id));
                await SagaHarness.Consumed.Any<OrderApprovedIntegration>(x => x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<OrderApprovedIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            case "OrderRejected":
                await Harness.Bus.Publish(new OrderRejectedIntegration(id));
                await SagaHarness.Consumed.Any<OrderRejectedIntegration>(x => x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<OrderRejectedIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            case "OrderFailed":
                await Harness.Bus.Publish(new OrderFailedIntegration(id));
                await SagaHarness.Consumed.Any<OrderFailedIntegration>(x => x.Context.Message.Id == id);
                return await SagaHarness.Consumed.Any<OrderFailedIntegration>(x =>
                    x.Context.Message.Id == id && x.Exception is not null);
            case "OrderStartedProcessing":
                await Harness.Bus.Publish(new OrderStartedProcessingIntegration(id));
                await SagaHarness.Consumed.Any<OrderStartedProcessingIntegration>(x =>
                    x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<OrderStartedProcessingIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            case "OrderConfirmed":
                await Harness.Bus.Publish(new OrderConfirmedIntegration(id));
                await SagaHarness.Consumed.Any<OrderConfirmedIntegration>(x => x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<OrderConfirmedIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            case "OrderCancelled":
                await Harness.Bus.Publish(new OrderCancelledIntegration(id));
                await SagaHarness.Consumed.Any<OrderCancelledIntegration>(x => x.Context.Message.Id == id);
                return await SagaHarness.Consumed.Any<OrderCancelledIntegration>(x =>
                    x.Context.Message.Id == id && x.Exception is not null);
            case "OrderRequestCancelled":
                await Harness.Bus.Publish(new OrderRequestCancelledIntegration(id));
                await SagaHarness.Consumed.Any<OrderRequestCancelledIntegration>(x => x.Context.Message.Id == id);
                return await SagaHarness.Consumed.Any<OrderRequestCancelledIntegration>(x =>
                    x.Context.Message.Id == id && x.Exception is not null);
            case "PaymentSucceeded":
                await Harness.Bus.Publish(new PaymentSucceededIntegration(id));
                await SagaHarness.Consumed.Any<PaymentSucceededIntegration>(x => x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<PaymentSucceededIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            case "PaymentFailed":
                await Harness.Bus.Publish(new PaymentFailedIntegration(id, "declined"));
                await SagaHarness.Consumed.Any<PaymentFailedIntegration>(x => x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<PaymentFailedIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            case "PaymentCancelled":
                await Harness.Bus.Publish(new PaymentCancelledIntegration(id));
                await SagaHarness.Consumed.Any<PaymentCancelledIntegration>(x => x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<PaymentCancelledIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            case "DeliveryPlaced":
                await Harness.Bus.Publish(new DeliveryPlacedIntegration(id));
                await SagaHarness.Consumed.Any<DeliveryPlacedIntegration>(x => x.Context.Message.OrderId == id);
                return await SagaHarness.Consumed.Any<DeliveryPlacedIntegration>(x =>
                    x.Context.Message.OrderId == id && x.Exception is not null);
            default:
                throw new ArgumentOutOfRangeException(nameof(eventName));
        }
    }

    [Theory]
    [MemberData(nameof(Cells))]
    public async Task Cell_ShouldNeverFault(string stateName, string eventName)
    {
        var id = await Arrange(stateName);

        var faulted = await PublishAndCheckFaulted(eventName, id);

        faulted.Should().BeFalse($"{eventName} arriving during {stateName} must be dropped by " +
                                  "OnUnhandledEvent, not thrown to the error queue");
    }
}
