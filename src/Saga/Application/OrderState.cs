using MassTransit;
using SharedKernel.Domain.Enums;

namespace Saga.Application;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public DateTime? FailedAt { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    public Guid? ApprovalTimeoutTokenId { get; set; }
    public Guid? PaymentTimeoutTokenId { get; set; }

    public Guid? PaymentId { get; set; }
    public decimal? Amount { get; set; }
    public Currency? Currency { get; set; }
}