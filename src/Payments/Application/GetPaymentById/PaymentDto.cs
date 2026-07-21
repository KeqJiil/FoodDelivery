using Payments.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace Payments.Application.GetPaymentById;

public record PaymentDto(
    Guid Id,
    Guid OrderRefId,
    Money Amount,
    PaymentStatus Status,
    string? FailureReason,
    DateTime CreatedAt);
