using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Restaurants;

/// <summary>Changes a name.</summary>
/// <param name="Name">New name, 3-30 characters.</param>
public sealed record ChangeNameRequest(
    [Required] [MinLength(3)] [MaxLength(30)] string Name);
