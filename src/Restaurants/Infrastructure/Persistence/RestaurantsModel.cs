using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;

namespace Restaurants.Infrastructure.Persistence;

public static class RestaurantsModel
{
    public const string Schema = "restaurants";

    public static ModelBuilder AddRestaurantsModel(this ModelBuilder modelBuilder) =>
        modelBuilder.ApplyModuleConfiguration(typeof(RestaurantsModel).Assembly, Schema);
}
