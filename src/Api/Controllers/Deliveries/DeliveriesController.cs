using Deliveries.Application.CompleteDelivery;
using Deliveries.Application.FailDelivery;
using Deliveries.Application.GetDeliveryById;
using Deliveries.Application.MarkPickedUp;
using Deliveries.Domain.Ids;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Deliveries;

[ApiController]
[Route("v1/[controller]")]
public class DeliveriesController : MyBasicController
{
    private readonly ISender _mediator;

    public DeliveriesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetDelivery([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeliveryByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("pickup/{id:Guid}")]
    public async Task<IActionResult> PickUp([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkPickedUpCommand(new DeliveryId(id)), cancellationToken);
        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    [HttpPost("complete/{id:Guid}")]
    public async Task<IActionResult> Complete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CompleteDeliveryCommand(new DeliveryId(id)), cancellationToken);
        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    [HttpPost("fail/{id:Guid}")]
    public async Task<IActionResult> Fail([FromRoute] Guid id, [FromBody] FailDeliveryBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new FailDeliveryCommand(new DeliveryId(id), body.Reason), cancellationToken);
        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }
}

public record FailDeliveryBody(string Reason);