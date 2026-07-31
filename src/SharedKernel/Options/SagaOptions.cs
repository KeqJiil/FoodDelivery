using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Options;

public class SagaOptions
{
    public const string SectionName = "Saga";

    [Required] public int TimeoutPayment { get; init; } = int.MaxValue;

    [Required] public int TimeoutApprovement { get; init; } = int.MaxValue;
}