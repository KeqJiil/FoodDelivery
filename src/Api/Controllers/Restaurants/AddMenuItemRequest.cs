using System.ComponentModel.DataAnnotations;
using SharedKernel.Domain.Enums;

namespace Api.Controllers.Restaurants;

/// <summary>Adds a new item to a restaurant's menu.</summary>
/// <param name="Name">Menu item name, 3-30 characters.</param>
/// <param name="Description">Menu item description, 10-200 characters.</param>
/// <param name="Currency">Currency of the price.</param>
/// <param name="Amount">Price of the menu item.</param>
public sealed record AddMenuItemRequest(
    [Required]
    [MaxLength(30)]
    [MinLength(3)]
    string Name,
    [Required]
    [MaxLength(200)]
    [MinLength(10)]
    string Description,
    [Required]
    [EnumDataType(typeof(Currency))]
    Currency Currency,
    [Required]
    [Range(0.1, double.MaxValue)]
    decimal Amount)
{
    public static AddMenuItemRequest Example { get; } =
        new("California Roll", "Crab, avocado, and cucumber, topped with tobiko.", Currency.Usd, 8.50m);
}