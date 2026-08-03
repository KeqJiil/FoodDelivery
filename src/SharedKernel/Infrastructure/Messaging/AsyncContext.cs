namespace SharedKernel.Infrastructure.Messaging;

public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _current = new();
    public static string? CorrelationId
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}