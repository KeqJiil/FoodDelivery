using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Domain.ValueObjects;

public record Description
{
    private const int MaxDescriptionLength = 200;
    private const int MinDescriptionLength = 10;

    public string Data { get; private set; }

    private Description(string data)
    {
        if (data.Length is > MaxDescriptionLength or < MinDescriptionLength)
            throw new ArgumentOutOfRangeException(nameof(data));
        Data = data;
    }

    public static Result<Description, Error> Create(string data)
    {
        return data.Length is > MaxDescriptionLength or < MinDescriptionLength
            ? Result<Description, Error>.Fail(
                Error.Validation($"Description must be between {MinDescriptionLength} and {MaxDescriptionLength} characters"))
            : Result<Description, Error>.Success(new Description(data));
    }
}