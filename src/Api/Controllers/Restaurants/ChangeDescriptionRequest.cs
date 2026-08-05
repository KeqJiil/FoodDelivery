using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Restaurants;

/// <summary>Changes a restaurant's description.</summary>
/// <param name="Description">New description, 10-200 characters.</param>
public sealed record ChangeDescriptionRequest(
    [Required] [MinLength(10)] [MaxLength(200)] string Description);
