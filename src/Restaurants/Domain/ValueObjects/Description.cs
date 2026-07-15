namespace Restaurants.Domain.ValueObjects;

public record Description
{
    private const int MaxDescriptionLength = 200;
    private const int MinDescriptionLength = 10;

    public string Data { get; private set; }

    public Description(string data)
    {
        if (data.Length is > MaxDescriptionLength or < MinDescriptionLength)
            throw new ArgumentOutOfRangeException(nameof(data));
        Data = data;
    }
}