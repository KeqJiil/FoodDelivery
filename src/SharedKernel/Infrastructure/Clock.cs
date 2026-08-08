using SharedKernel.Domain;

namespace SharedKernel.Infrastructure;

public class ClockDateTimeUtf : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}