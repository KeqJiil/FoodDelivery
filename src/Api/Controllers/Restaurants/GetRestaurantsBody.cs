using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Restaurants;

/// <summary>Pagination params.</summary>
/// <param name="Page">Number of current page.</param>
/// <param name="PageSize">Size of page.</param>>
public record GetRestaurantsBody(
    [Required] [Range(1, int.MaxValue)] int Page,
    [Required] [Range(5, 50)] int PageSize)
{
    public static GetRestaurantsBody Example { get; } = new(2, 10);
}