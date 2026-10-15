using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Persistence;

public static class OrderingModel
{
    public const string Schema = "ordering";

    public static ModelBuilder AddOrderingModel(this ModelBuilder modelBuilder) =>
        modelBuilder.ApplyModuleConfiguration(typeof(OrderingModel).Assembly, Schema);
}
