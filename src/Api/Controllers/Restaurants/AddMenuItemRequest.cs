using System.ComponentModel.DataAnnotations;
using SharedKernel.Domain.Enums;

namespace Api.Controllers.Restaurants;

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
    decimal Amount);