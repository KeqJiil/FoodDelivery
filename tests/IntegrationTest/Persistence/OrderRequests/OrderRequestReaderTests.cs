using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using OrderRequests.Domain.Aggregates;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;
using OrderRequests.Infrastructure.Persistence;
using OrderRequests.Infrastructure.Persistence.Readers;

namespace FoodDelivery.IntegrationTest.Persistence.OrderRequests;

public class OrderRequestReaderTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnRequest_WhenItExists()
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
        var reader = new OrderRequestReader(readContext);

        var dto = await reader.GetByIdAsync(request.Id.Id);

        dto.Should().NotBeNull();
        dto!.OrderId.Should().Be(orderRefId.Id);
        dto.RestaurantId.Should().Be(restaurantRefId.Id);
        dto.Status.Should().Be(OrderRequestStatus.Pending);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRequestDoesNotExist()
    {
        await using var readContext = CreateOrderRequestsContext();
        var reader = new OrderRequestReader(readContext);

        var dto = await reader.GetByIdAsync(Guid.NewGuid());

        dto.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByRestaurantIdAsync_ShouldFilterByRestaurantAndStatus()
    {
        var targetRestaurant = new RestaurantRefId(Guid.NewGuid());
        var otherRestaurant = new RestaurantRefId(Guid.NewGuid());

        var pendingForTarget = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()),
            targetRestaurant);
        var approvedForTarget = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()),
            targetRestaurant);
        approvedForTarget.Approve();
        var pendingForOther = OrderRequest.Create(new OrderRequestId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()),
            otherRestaurant);

        await using (var writeContext = CreateOrderRequestsContext())
        {
            writeContext.OrderRequests.AddRange(pendingForTarget, approvedForTarget, pendingForOther);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateOrderRequestsContext();
        var reader = new OrderRequestReader(readContext);

        var result = await reader.GetAllByRestaurantIdAsync(targetRestaurant.Id, cursorCreatedAt: null,
            cursorId: null, limit: 10, statusFilter: OrderRequestStatus.Pending);

        result.Should().ContainSingle().Which.OrderRequestId.Should().Be(pendingForTarget.Id.Id);
    }

    [Fact]
    public async Task GetAllByRestaurantIdAsync_ShouldPaginateWithoutGapsOrDuplicates()
    {
        var restaurantRefId = new RestaurantRefId(Guid.NewGuid());
        var requests = Enumerable.Range(0, 3)
            .Select(_ => OrderRequest.Create(new OrderRequestId(), new OrderRefId(Guid.NewGuid()), restaurantRefId))
            .ToList();

        await using (var writeContext = CreateOrderRequestsContext())
        {
            writeContext.OrderRequests.AddRange(requests);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateOrderRequestsContext();
        var reader = new OrderRequestReader(readContext);

        var firstPage = (await reader.GetAllByRestaurantIdAsync(restaurantRefId.Id, cursorCreatedAt: null,
            cursorId: null, limit: 2, statusFilter: null)).ToList();
        firstPage.Should().HaveCount(2);

        var lastOfFirstPage = firstPage[^1];
        var secondPage = (await reader.GetAllByRestaurantIdAsync(restaurantRefId.Id,
            cursorCreatedAt: lastOfFirstPage.CreatedAt, cursorId: lastOfFirstPage.OrderRequestId, limit: 2,
            statusFilter: null)).ToList();

        secondPage.Should().ContainSingle();
        firstPage.Concat(secondPage).Select(r => r.OrderRequestId).Should()
            .BeEquivalentTo(requests.Select(r => r.Id.Id));
    }

    private OrderRequestsDbContext CreateOrderRequestsContext() =>
        CreateContext<OrderRequestsDbContext>("order_requests", o => new OrderRequestsDbContext(o));
}
