using MassTransit;
using SharedKernel.Domain.Enums;

namespace Saga.Application;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime? FailedAt { get; set; }
    public byte[] RowVersion { get; set; }
    public Guid? ApprovalTimeoutTokenId { get; set; }
    public Guid? PaymentTimeoutTokenId { get; set; }

    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
}