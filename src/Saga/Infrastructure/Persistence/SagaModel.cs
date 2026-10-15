using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;

namespace Saga.Infrastructure.Persistence;

public static class SagaModel
{
    public const string Schema = "saga";

    public static ModelBuilder AddSagaModel(this ModelBuilder modelBuilder) =>
        modelBuilder.ApplyModuleConfiguration(typeof(SagaModel).Assembly, Schema);
}
