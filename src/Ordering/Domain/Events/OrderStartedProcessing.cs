using Ordering.Domain.Ids;
using SharedKernel.Domain;
using SharedKernel.Domain.ValueObjects;

namespace Ordering.Domain.Events;

public record OrderStartedProcessing(OrderId Id, Money Price) : DomainEvent<OrderId>(Id);