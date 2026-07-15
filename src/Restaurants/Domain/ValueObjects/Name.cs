namespace Restaurants.Domain.ValueObjects;

public record Name
{
    private const int MaxNameLength = 30;
    private const int MinNameLength = 3;

    public string Data { get; private set; }

    public Name(string data)
    {
        if (data.Length is > MaxNameLength or < MinNameLength) throw new ArgumentOutOfRangeException(nameof(data));
        Data = data;
    }
}