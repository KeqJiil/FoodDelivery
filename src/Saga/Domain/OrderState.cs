using MassTransit;

namespace Saga.Domain;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime? FailedAt { get; set; }
    public byte[] RowVersion { get; set; }
}