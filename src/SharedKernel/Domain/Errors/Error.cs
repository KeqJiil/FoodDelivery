using SharedKernel.Domain.Enums;

namespace SharedKernel.Domain.Errors;

public record Error(ErrorEnum Type, string Message)
{
    public static Error NotFound(string message) => new(ErrorEnum.NotFound, message);
    public static Error Validation(string message) => new(ErrorEnum.Validation, message);
    public static Error Conflict(string message) => new(ErrorEnum.Conflict, message);
    public static Error NotAllowed(string message) => new(ErrorEnum.NotAllowed, message);
    public static Error Unexpected() => new(ErrorEnum.Unexpected, "Unexpected error");
}