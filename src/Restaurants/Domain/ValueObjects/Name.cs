using SharedKernel.Domain;
using SharedKernel.Domain.Errors;

namespace Restaurants.Domain.ValueObjects;

public record Name
{
    private const int MaxNameLength = 30;
    private const int MinNameLength = 3;

    public string Data { get; private set; }

    private Name(string data)
    {
        Data = data;
    }

    public static Result<Name, Error> Create(string data)
    {
        return data.Length is > MaxNameLength or < MinNameLength
            ? Result<Name, Error>.Fail(Error.Validation($"Name must be between {MinNameLength} and {MaxNameLength} characters"))
            : Result<Name, Error>.Success(new Name(data));
    }
}