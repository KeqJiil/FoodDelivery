using Deliveries.Domain.Enums;

namespace Deliveries.Application.GetDeliveryById;

public record DeliveryDto(
    Guid Id,
    Guid OrderRefId,
    DeliveryStatus Status,
    string? FailureReason,
    DateTime CreatedAt);
