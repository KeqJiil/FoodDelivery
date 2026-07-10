using SharedKernel.Domain.ValueObjects;

namespace Ordering.Application.GetOrderById;

public record OrderLineDto(Guid Id, Money Price, byte Quantity, Guid MenuItemRefId);