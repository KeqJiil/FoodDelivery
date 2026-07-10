using SharedKernel.Domain.Enums;

namespace SharedKernel.Domain.Errors;

public record Error(ErrorEnum Type, string Message);