using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderRequests.Application.AcceptOrder;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Application.GetOrdersByRestaurantId;
using OrderRequests.Application.RejectOrder;
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

    /// <summary>Lists a restaurant's order requests, newest first, using cursor-based pagination.</summary>
    /// <param name="id">Restaurant id.</param>
    /// <param name="body">Pagination cursor, page size, and optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("restaurant/{id:Guid}")]
    public async Task<ActionResult<IEnumerable<OrderRequestDto>>> GetOrdersByRestaurantId([FromRoute] Guid id,
        [FromQuery] ByRestaurantIdBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetOrdersByRestaurantIdQuery(id, body.CursorCreatedAt, body.CursorId, body.Limit, body.Status),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets an order request by id.</summary>
    /// <param name="id">Order request id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<OrderRequestDto>> GetOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderRequestByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : result;
    }

    /// <summary>Rejects an order request. Triggers the saga's compensating flow for the originating order.</summary>
    /// <param name="id">Order request id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("{id:Guid}/reject")]
    public async Task<IActionResult> RejectOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RejectOrderCommand(new OrderRequestId(id)), cancellationToken);
        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Approves an order request, letting the order proceed to payment.</summary>
    /// <param name="id">Order request id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("{id:Guid}/approve")]
    public async Task<IActionResult> ApproveOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AcceptOrderCommand(new OrderRequestId(id)), cancellationToken);
        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }
}