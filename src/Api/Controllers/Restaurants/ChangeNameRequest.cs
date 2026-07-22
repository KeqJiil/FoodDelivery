using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Restaurants;

public sealed record ChangeNameRequest(
    [Required] [MinLength(3)] [MaxLength(30)] string Name);
