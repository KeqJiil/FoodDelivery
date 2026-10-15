using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;

namespace Deliveries.Infrastructure.Persistence;

public static class DeliveriesModel
{
    public const string Schema = "deliveries";

    public static ModelBuilder AddDeliveriesModel(this ModelBuilder modelBuilder) =>
        modelBuilder.ApplyModuleConfiguration(typeof(DeliveriesModel).Assembly, Schema);
}
