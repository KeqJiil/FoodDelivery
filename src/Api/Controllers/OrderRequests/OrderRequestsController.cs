using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderRequests.Application.AcceptOrder;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Application.GetOrdersByRestaurantId;
using OrderRequests.Application.RejectOrder;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;

namespace Api.Controllers.OrderRequests;

[ApiController]
[Route("v1/[controller]")]
public class OrderRequestsController : MyBasicController
{
    private readonly ISender _mediator;

    public OrderRequestsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("restaurant/{id:Guid}")]
    public async Task<IActionResult> GetOrdersByRestaurantId([FromRoute] Guid id, [FromQuery] ByRestaurantIdBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrdersByRestaurantIdQuery(id, body.Cursor, body.Limit, body.Status), cancellationToken);
        return result.Any() ? Ok(result) : Ok();
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderRequestByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:Guid}/reject")]
    public async Task<IActionResult> RejectOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RejectOrderCommand(new OrderRequestId(id)), cancellationToken);
        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    [HttpPost("{id:Guid}/approve")]
    public async Task<IActionResult> ApproveOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AcceptOrderCommand(new OrderRequestId(id)), cancellationToken);
        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }
}

public record ByRestaurantIdBody(byte Limit, OrderRequestStatus? Status, Guid? Cursor);