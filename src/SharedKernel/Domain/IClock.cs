namespace SharedKernel.Domain;

public interface IClock
{
    DateTimeOffset Now { get; }
}