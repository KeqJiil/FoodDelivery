using System.ComponentModel.DataAnnotations;
using SharedKernel.Domain.Enums;

namespace Api.Controllers.Restaurants;

/// <summary>Registers a new restaurant.</summary>
/// <param name="Name">Restaurant name, 3-30 characters.</param>
/// <param name="Description">Restaurant description, 10-200 characters.</param>
/// <param name="Amount">Minimum order price amount.</param>
/// <param name="Currency">Currency of the minimum order price.</param>
/// <param name="Schedules">Weekly opening windows.</param>
public sealed record CreateRestaurantRequest(
    [Required] [MinLength(3)] [MaxLength(30)] string Name,
    [Required] [MinLength(10)] [MaxLength(200)] string Description,
    [Range(0.01, double.MaxValue)] decimal Amount,
    [EnumDataType(typeof(Currency))] Currency Currency,
    [Required] List<OpeningWindowRequest> Schedules)
{
    public static CreateRestaurantRequest Example { get; } = new(
        "Sakura Sushi", "Downtown Japanese restaurant, dine-in and delivery.", 15.00m, Currency.Usd,
        [new OpeningWindowRequest(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0))]);
}