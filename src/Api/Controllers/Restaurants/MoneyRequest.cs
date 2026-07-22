using System.ComponentModel.DataAnnotations;
using SharedKernel.Domain.Enums;

namespace Api.Controllers.Restaurants;

public sealed record MoneyRequest(
    [Required] [EnumDataType(typeof(Currency))] Currency Currency,
    [Range(0, double.MaxValue)] decimal Amount);
