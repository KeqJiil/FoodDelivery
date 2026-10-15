using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;

namespace Payments.Infrastructure.Persistence;

public static class PaymentsModel
{
    public const string Schema = "payments";

    public static ModelBuilder AddPaymentsModel(this ModelBuilder modelBuilder) =>
        modelBuilder.ApplyModuleConfiguration(typeof(PaymentsModel).Assembly, Schema);
}
