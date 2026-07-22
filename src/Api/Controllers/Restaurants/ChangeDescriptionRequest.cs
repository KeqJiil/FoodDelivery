using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Restaurants;

public sealed record ChangeDescriptionRequest(
    [Required] [MinLength(10)] [MaxLength(200)] string Description);
