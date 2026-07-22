using System.ComponentModel.DataAnnotations;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.Enums;

namespace Api.Controllers.Restaurants;

public sealed record CreateRestaurantRequest(
    [Required] [MaxLength(200)] string Name,
    [Required] [MaxLength(1000)] string Description,
    [Range(0.01, double.MaxValue)] decimal Amount,
    [EnumDataType(typeof(Currency))] Currency Currency,
    [Required] List<OpeningWindow> Schedules);