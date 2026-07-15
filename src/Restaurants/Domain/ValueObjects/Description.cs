namespace Restaurants.Domain.ValueObjects;

public record Description
{
    private const int MaxDescriptionLength = 200;
    private const int MinDescriptionLength = 10;

    public string Data { get; private set; }

    public Description(string desc)
    {
        if (desc.Length is > MaxDescriptionLength or < MinDescriptionLength)
            throw new ArgumentOutOfRangeException(nameof(desc));
        Data = desc;
    }
}