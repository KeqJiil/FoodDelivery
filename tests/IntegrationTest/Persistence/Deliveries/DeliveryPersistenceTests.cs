using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Deliveries.Domain.Aggregates;
using Deliveries.Domain.Enums;
using Deliveries.Domain.Ids;
using Deliveries.Infrastructure.Persistence;

namespace FoodDelivery.IntegrationTest.Persistence.Deliveries;

public class DeliveryPersistenceTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SaveAndReload_ShouldPersistDelivery()
    {
        var orderRefId = new OrderRefId(Guid.NewGuid());
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), orderRefId);

        await using (var writeContext = CreateDeliveriesContext())
        {
            writeContext.Deliveries.Add(delivery);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDeliveriesContext();
        var reloaded = await readContext.Deliveries.FirstOrDefaultAsync(d => d.Id == delivery.Id);

        reloaded.Should().NotBeNull();
        reloaded!.Status.Should().Be(DeliveryStatus.Pending);
        reloaded.OrderRefId.Should().Be(orderRefId);
        reloaded.FailureReason.Should().BeNull();
    }

    [Fact]
    public async Task StatusTransitions_ShouldPersist_AcrossSeparateRequests()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));

        await using (var writeContext = CreateDeliveriesContext())
        {
            writeContext.Deliveries.Add(delivery);
            await writeContext.SaveChangesAsync();
        }

        await using (var pickUpContext = CreateDeliveriesContext())
        {
            var tracked = await pickUpContext.Deliveries.FirstAsync(d => d.Id == delivery.Id);
            tracked.MarkPickedUp();
            await pickUpContext.SaveChangesAsync();
        }

        await using (var completeContext = CreateDeliveriesContext())
        {
            var tracked = await completeContext.Deliveries.FirstAsync(d => d.Id == delivery.Id);
            tracked.Complete();
            await completeContext.SaveChangesAsync();
        }

        await using var readContext = CreateDeliveriesContext();
        var reloaded = await readContext.Deliveries.FirstAsync(d => d.Id == delivery.Id);

        reloaded.Status.Should().Be(DeliveryStatus.Delivered);
    }

    [Fact]
    public async Task SecondSave_ShouldThrow_WhenBothContextsLoadedTheSameStaleDelivery()
    {
        var delivery = Delivery.Create(new DeliveryId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()));

        await using (var writeContext = CreateDeliveriesContext())
        {
            writeContext.Deliveries.Add(delivery);
            await writeContext.SaveChangesAsync();
        }

        await using var firstContext = CreateDeliveriesContext();
        await using var secondContext = CreateDeliveriesContext();

        var firstCopy = await firstContext.Deliveries.FirstAsync(d => d.Id == delivery.Id);
        var secondCopy = await secondContext.Deliveries.FirstAsync(d => d.Id == delivery.Id);

        firstCopy.MarkPickedUp();
        await firstContext.SaveChangesAsync();

        secondCopy.Fail("Courier unavailable");

        DbUpdateConcurrencyException? caughtException = null;
        try
        {
            await secondContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            caughtException = ex;
        }

        caughtException.Should().NotBeNull();
    }

    private DeliveriesDbContext CreateDeliveriesContext() =>
        CreateContext<DeliveriesDbContext>("deliveries", o => new DeliveriesDbContext(o));
}
