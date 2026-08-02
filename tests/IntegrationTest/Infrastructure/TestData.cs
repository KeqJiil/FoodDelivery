using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Controllers.Ordering;
using Api.Controllers.Restaurants;
using Deliveries.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Enums;
using OrderRequests.Domain.Enums;
using OrderRequests.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence;
using Saga.Application;
using Saga.Infrastructure.Persistence;
using Restaurants.Domain.Enums;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Infrastructure;

internal record CreatedResponse(Guid Id);

internal record MoneyTestDto(Currency Currency, decimal Amount);

internal record MenuItemTestDto(Guid Id, string Name, string Description, MoneyTestDto Price);

internal record RestaurantTestDto(
    Guid Id,
    string Name,
    string Description,
    MoneyTestDto MinimalOrderPrice,
    RestaurantStatus Status,
    IReadOnlyList<OpeningWindow> OpeningWindows,
    IReadOnlyList<MenuItemTestDto> MenuItems);

internal record OrderLineTestDto(Guid Id, MoneyTestDto Price, byte Quantity, Guid MenuItemRefId);

internal record OrderTestDto(
    Guid Id,
    OrderStatus Status,
    Guid RestaurantRefId,
    MoneyTestDto? TotalPrice,
    IReadOnlyList<OrderLineTestDto> OrderLines,
    DateTime CreatedAt);

internal record OrderRequestTestDto(
    Guid OrderRequestId,
    Guid RestaurantId,
    Guid OrderId,
    OrderRequestStatus Status,
    DateTime CreatedAt);

public static class TestData
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static CreateRestaurantRequest GetRestaurantRequest()
    {
        return new CreateRestaurantRequest("restaurant", "restaurant", 1, Currency.Usd,
            new List<OpeningWindow>
                { new(DayOfWeek.Friday, new TimeOnly(1, 1), DayOfWeek.Monday, new TimeOnly(2, 2)) });
    }

    public static async Task<Guid> SeedRestaurant(HttpClient client,
        CreateRestaurantRequest? createRestaurantRequest = null)
    {
        var restaurant = createRestaurantRequest ?? GetRestaurantRequest();

        var response = await client.PostAsJsonAsync("v1/restaurants", restaurant);
        var body = await response.Content.ReadFromJsonAsync<CreatedResponse>();

        return body!.Id;
    }

    internal static async Task<RestaurantTestDto> GetRestaurant(HttpClient client, Guid restaurantId)
    {
        var response = await client.GetAsync($"v1/restaurants/{restaurantId}");

        return (await response.Content.ReadFromJsonAsync<RestaurantTestDto>(JsonOptions))!;
    }
    
    public static async Task<Guid> SeedMenuItem(HttpClient client, Guid restaurantId,
        AddMenuItemRequest? addMenuItemRequest = null)
    {
        var menuItem = addMenuItemRequest ??
                       new AddMenuItemRequest("menu item", "menu item description", Currency.Usd, 5);

        await client.PostAsJsonAsync($"v1/restaurants/{restaurantId}/menu-items", menuItem);

        var restaurant = await GetRestaurant(client, restaurantId);

        return restaurant.MenuItems.First(x => x.Name == menuItem.Name).Id;
    }

    public static async Task<Guid> SeedOrder(HttpClient client, Guid restaurantId)
    {
        var response = await client.PostAsJsonAsync("v1/ordering", new CreateOrderRequest(restaurantId));
        var body = await response.Content.ReadFromJsonAsync<CreatedResponse>();

        return body!.Id;
    }

    internal static async Task<OrderTestDto> GetOrder(HttpClient client, Guid orderId)
    {
        var response = await client.GetAsync($"v1/ordering/{orderId}");

        return (await response.Content.ReadFromJsonAsync<OrderTestDto>(JsonOptions))!;
    }

    internal static async Task<OrderRequestTestDto?> FindOrderRequestByOrderId(HttpClient client, Guid restaurantId,
        Guid orderId)
    {
        var response = await client.GetAsync($"v1/orderrequests/restaurant/{restaurantId}?Limit=100");
        var raw = await response.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var body = JsonSerializer.Deserialize<List<OrderRequestTestDto>>(raw, JsonOptions) ?? [];

        return body.FirstOrDefault(x => x.OrderId == orderId);
    }

    private static TContext CreateContext<TContext>(string connectionString, string schema,
        Func<DbContextOptions<TContext>, TContext> create) where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema))
            .Options;

        return create(options);
    }
    
    public static async Task<Guid> SeedPayment(string connectionString, decimal amount = 25m)
    {
        var payment = Payments.Domain.Aggregates.Payment.Create(
            new Payments.Domain.Ids.PaymentId(Guid.NewGuid()),
            new Payments.Domain.Ids.OrderRefId(Guid.NewGuid()),
            Money.Create(Currency.Usd, amount).Ok!);

        await using var context = CreateContext<PaymentsDbContext>(connectionString, "payments",
            o => new PaymentsDbContext(o));
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        return payment.Id.Id;
    }

    public static async Task<Guid> SeedDelivery(string connectionString)
    {
        var delivery = Deliveries.Domain.Aggregates.Delivery.Create(
            new Deliveries.Domain.Ids.DeliveryId(Guid.NewGuid()),
            new Deliveries.Domain.Ids.OrderRefId(Guid.NewGuid()));

        await using var context = CreateContext<DeliveriesDbContext>(connectionString, "deliveries",
            o => new DeliveriesDbContext(o));
        context.Deliveries.Add(delivery);
        await context.SaveChangesAsync();

        return delivery.Id.Id;
    }

    public static async Task<Guid> SeedOrderRequest(string connectionString)
    {
        var orderRequest = OrderRequests.Domain.Aggregates.OrderRequest.Create(
            new OrderRequests.Domain.Ids.OrderRequestId(Guid.NewGuid()),
            new OrderRequests.Domain.Ids.OrderRefId(Guid.NewGuid()),
            new OrderRequests.Domain.Ids.RestaurantRefId(Guid.NewGuid()));

        await using var context = CreateContext<OrderRequestsDbContext>(connectionString, "order_requests",
            o => new OrderRequestsDbContext(o));
        context.OrderRequests.Add(orderRequest);
        await context.SaveChangesAsync();

        return orderRequest.Id.Id;
    }

    public static async Task<Payments.Domain.Aggregates.Payment?> FindPaymentByOrderId(string connectionString,
        Guid orderId)
    {
        await using var context = CreateContext<PaymentsDbContext>(connectionString, "payments",
            o => new PaymentsDbContext(o));

        return await context.Payments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderRefId == new Payments.Domain.Ids.OrderRefId(orderId));
    }

    public static async Task<Deliveries.Domain.Aggregates.Delivery?> FindDeliveryByOrderId(string connectionString,
        Guid orderId)
    {
        await using var context = CreateContext<DeliveriesDbContext>(connectionString, "deliveries",
            o => new DeliveriesDbContext(o));

        return await context.Deliveries.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderRefId == new Deliveries.Domain.Ids.OrderRefId(orderId));
    }

    public static async Task<OrderState?> GetSagaState(string connectionString, Guid orderId)
    {
        await using var context = CreateContext<SagaDbContext>(connectionString, "saga", o => new SagaDbContext(o));

        return await context.OrderStates.AsNoTracking().FirstOrDefaultAsync(x => x.CorrelationId == orderId);
    }
}
