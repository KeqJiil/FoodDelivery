using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;
using OrderRequests.Infrastructure.Persistence;

namespace FoodDelivery.IntegrationTest.Persistence.OrderRequests;

public class OrderRequestPersistenceTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SaveAndReload_ShouldPersistOrderRequest()
    {
        var orderRefId = new OrderRefId(Guid.NewGuid());
        var restaurantRefId = new RestaurantRefId(Guid.NewGuid());
        var request = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()), orderRefId, restaurantRefId);

        await using (var writeContext = CreateOrderRequestsContext())
        {
            writeContext.OrderRequests.Add(request);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateOrderRequestsContext();
        var reloaded = await readContext.OrderRequests.FirstOrDefaultAsync(r => r.Id == request.Id);

        reloaded.Should().NotBeNull();
        reloaded!.Status.Should().Be(OrderRequestStatus.Pending);
        reloaded.OrderRefId.Should().Be(orderRefId);
        reloaded.RestaurantRefId.Should().Be(restaurantRefId);
    }

    [Fact]
    public async Task SecondSave_ShouldThrow_WhenBothContextsLoadedTheSameStaleRequest()
    {
        var request = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()),
            new RestaurantRefId(Guid.NewGuid()));

        await using (var writeContext = CreateOrderRequestsContext())
        {
            writeContext.OrderRequests.Add(request);
            await writeContext.SaveChangesAsync();
        }

        await using var firstContext = CreateOrderRequestsContext();
        await using var secondContext = CreateOrderRequestsContext();

        var firstCopy = await firstContext.OrderRequests.FirstAsync(r => r.Id == request.Id);
        var secondCopy = await secondContext.OrderRequests.FirstAsync(r => r.Id == request.Id);

        firstCopy.Approve();
        await firstContext.SaveChangesAsync();

        secondCopy.Reject();

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

    private OrderRequestsDbContext CreateOrderRequestsContext() =>
        CreateContext<OrderRequestsDbContext>("order_requests", o => new OrderRequestsDbContext(o));
}
