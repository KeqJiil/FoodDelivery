using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Enums;
using Deliveries.Domain.Ids;
using Deliveries.Infrastructure.Persistence;
using Deliveries.Infrastructure.Persistence.Readers;

namespace FoodDelivery.IntegrationTest.Persistence.Deliveries;

public class DeliveryReaderTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnDelivery_WhenItExists()
    {
        var orderRefId = new OrderRefId(Guid.NewGuid());
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), orderRefId);

        await using (var writeContext = CreateDeliveriesContext())
        {
            writeContext.Deliveries.Add(delivery);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDeliveriesContext();
        var reader = new DeliveryReader(readContext);

        var dto = await reader.GetByIdAsync(delivery.Id.Id);

        dto.Should().NotBeNull();
        dto!.OrderRefId.Should().Be(orderRefId.Id);
        dto.Status.Should().Be(DeliveryStatus.Pending);
        dto.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDeliveryDoesNotExist()
    {
        await using var readContext = CreateDeliveriesContext();
        var reader = new DeliveryReader(readContext);

        var dto = await reader.GetByIdAsync(Guid.NewGuid());

        dto.Should().BeNull();
    }

    private DeliveriesDbContext CreateDeliveriesContext() =>
        CreateContext<DeliveriesDbContext>("deliveries", o => new DeliveriesDbContext(o));
}
