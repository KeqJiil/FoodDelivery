using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace SharedKernel.Infrastructure.Persistence;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyModuleConfiguration(this ModelBuilder modelBuilder, Assembly assembly,
        string schema)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => !e.IsOwned() && e.ClrType.Assembly == assembly))
            entityType.SetSchema(schema);

        return modelBuilder;
    }
}
