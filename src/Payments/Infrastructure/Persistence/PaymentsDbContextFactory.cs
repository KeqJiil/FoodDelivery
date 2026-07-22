using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var connectionStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (connectionStr is null)
            throw new InvalidOperationException(
                "No connection string found.");

        var optionBuilder = new DbContextOptionsBuilder<PaymentsDbContext>();

        optionBuilder.UseNpgsql(connectionStr);

        return new PaymentsDbContext(optionBuilder.Options);
    }
}
