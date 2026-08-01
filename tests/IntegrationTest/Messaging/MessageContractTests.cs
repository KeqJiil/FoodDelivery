using System.Text.Json;
using FluentAssertions;
using MassTransit;
using MassTransit.Serialization;
using SharedKernel.Domain.Enums;
using SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents;
using SharedKernel.Infrastructure.IntegrationEvents.SagaEvents;

namespace FoodDelivery.IntegrationTest.Messaging;

public class MessageContractTests
{
    private static readonly JsonSerializerOptions WireOptions = SystemTextJsonMessageSerializer.Options;

    [Fact]
    public void OrderPlacedIntegration_ShouldRoundTripThroughWireSerializerWithoutLoss()
    {
        var original = new OrderPlacedIntegration(Guid.CreateVersion7(), Guid.CreateVersion7(), 42.567891234m, Currency.Eur);

        var json = JsonSerializer.Serialize(original, WireOptions);
        var roundtripped = JsonSerializer.Deserialize<OrderPlacedIntegration>(json, WireOptions);

        roundtripped.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void PaymentFailedIntegration_ShouldRoundTripStatusAndReasonThroughWireSerializer()
    {
        var original = new PaymentFailedIntegration(Guid.CreateVersion7(), "gateway declined");

        var json = JsonSerializer.Serialize(original, WireOptions);
        var roundtripped = JsonSerializer.Deserialize<PaymentFailedIntegration>(json, WireOptions);

        roundtripped.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Decimal_ShouldSerializeAsStringOnTheWire_ToAvoidFloatingPointPrecisionLoss()
    {
        var original = new OrderPlacedIntegration(Guid.CreateVersion7(), Guid.CreateVersion7(), 42.567891234m, Currency.Eur);

        var json = JsonSerializer.Serialize(original, WireOptions);

        json.Should().Contain("\"amount\": \"42.567891234\"",
            "MassTransit's StringDecimalJsonConverter must keep full decimal precision, " +
            "which a plain JSON number (double) would not guarantee");
    }

    [Theory]
    [InlineData(typeof(OrderPlacedIntegration), "urn:message:SharedKernel.Infrastructure.IntegrationEvents.SagaEvents:OrderPlacedIntegration")]
    [InlineData(typeof(OrderApprovedIntegration), "urn:message:SharedKernel.Infrastructure.IntegrationEvents.SagaEvents:OrderApprovedIntegration")]
    [InlineData(typeof(MenuItemPriceChangedIntegration), "urn:message:SharedKernel.Infrastructure.IntegrationEvents.NonsagaEvents:MenuItemPriceChangedIntegration")]
    public void MessageUrn_ShouldStayStable_SoRenamingOrMovingAContractIsADeliberateBreakingChange(Type messageType, string expectedUrn)
    {
        MessageUrn.ForTypeString(messageType).Should().Be(expectedUrn);
    }
}
