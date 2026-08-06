using System.ComponentModel.DataAnnotations;
using SharedKernel.Domain.Enums;

namespace Api.Controllers.Restaurants;

/// <summary>Registers a new restaurant.</summary>
/// <param name="Name">Restaurant name, up to 200 characters.</param>
/// <param name="Description">Restaurant description, up to 1000 characters.</param>
/// <param name="Amount">Minimum order price amount.</param>
/// <param name="Currency">Currency of the minimum order price.</param>
/// <param name="Schedules">Weekly opening windows.</param>
public sealed record CreateRestaurantRequest(
    [Required] [MaxLength(200)] string Name,
    [Required] [MaxLength(1000)] string Description,
    [Range(0.01, double.MaxValue)] decimal Amount,
    [EnumDataType(typeof(Currency))] Currency Currency,
    [Required] List<OpeningWindowRequest> Schedules)
{
    public static CreateRestaurantRequest Example { get; } = new(
        "Sakura Sushi", "Downtown Japanese restaurant, dine-in and delivery.", 15.00m, Currency.Usd,
        [new OpeningWindowRequest(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0))]);
}