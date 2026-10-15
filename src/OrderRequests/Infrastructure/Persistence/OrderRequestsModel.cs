using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;

namespace OrderRequests.Infrastructure.Persistence;

public static class OrderRequestsModel
{
    public const string Schema = "order_requests";

    public static ModelBuilder AddOrderRequestsModel(this ModelBuilder modelBuilder) =>
        modelBuilder.ApplyModuleConfiguration(typeof(OrderRequestsModel).Assembly, Schema);
}
