using Deliveries.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Infrastructure.Persistence;
using OrderRequests.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence;
using Restaurants.Infrastructure.Persistence;
using Saga.Infrastructure.Persistence;

namespace Api.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public const string MessagingSchema = "messaging";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOrderingModel();
        modelBuilder.AddRestaurantsModel();
        modelBuilder.AddOrderRequestsModel();
        modelBuilder.AddPaymentsModel();
        modelBuilder.AddDeliveriesModel();
        modelBuilder.AddSagaModel();

        modelBuilder.HasDefaultSchema(MessagingSchema);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}