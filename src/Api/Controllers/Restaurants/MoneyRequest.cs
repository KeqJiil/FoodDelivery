using System.ComponentModel.DataAnnotations;
using SharedKernel.Domain.Enums;

namespace Api.Controllers.Restaurants;

/// <summary>A monetary amount in a given currency.</summary>
/// <param name="Currency">Currency of the amount.</param>
/// <param name="Amount">Non-negative amount.</param>
public sealed record MoneyRequest(
    [Required] [EnumDataType(typeof(Currency))] Currency Currency,
    [Range(0, double.MaxValue)] decimal Amount);
