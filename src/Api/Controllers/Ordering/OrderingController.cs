using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.AddOrderLineItem;
using Ordering.Application.CancelOrder;
using Ordering.Application.CreateOrder;
using Ordering.Application.GetOrderById;
using Ordering.Application.PlaceOrder;
using Ordering.Application.RemoveOrderLineItem;
using Ordering.Domain.Ids;

namespace Api.Controllers.Ordering;

[ApiController]
[Route("v1/[controller]")]
public class OrderingController : MyBasicController
{
    private readonly ISender _mediator;

    public OrderingController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gets an order by id.</summary>
    /// <param name="id">Order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);

        return result is not null ? result : NotFound();
    }

    /// <summary>Creates a new draft order for a restaurant. The order starts empty; add lines with the add-items endpoint before placing it.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateOrderCommand(new RestaurantRefId(request.RestaurantId)),
            cancellationToken);

        if (!result.IsSuccess) return GetProblem(result.Error!);

        return CreatedAtAction(nameof(GetById), new { id = result.Ok!.Id }, new { id = result.Ok!.Id });
    }

    /// <summary>Cancels an order. Only allowed while the order hasn't been placed yet.</summary>
    /// <param name="id">Order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelOrderCommand(new OrderId(id)), cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Places a draft order, moving it from the restaurant's queue toward approval. Fails if the order has no lines or is below the restaurant's minimum order price.</summary>
    /// <param name="id">Order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("{id:guid}/place")]
    public async Task<IActionResult> PlaceOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PlaceOrderCommand(new OrderId(id)), cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Adds a menu item to a draft order as a new order line.</summary>
    /// <param name="id">Order id.</param>
    /// <param name="request">Menu item to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("{id:guid}/add-items")]
    public async Task<IActionResult> AddOrderLineItem([FromRoute] Guid id, [FromBody] AddOrderLineRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(new AddOrderLineItemCommand(new OrderId(id), new MenuItemRefId(request.MenuId)),
                cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Ok!.Id }, new { id = result.Ok!.Id })
            : GetProblem(result.Error!);
    }

    /// <summary>Removes a line from a draft order.</summary>
    /// <param name="id">Order id.</param>
    /// <param name="orderLineId">Order line id to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:guid}/remove/{orderLineId:guid}")]
    public async Task<IActionResult> RemoveOrderLineItem([FromRoute] Guid id, [FromRoute] Guid orderLineId,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(new RemoveOrderLineItemCommand(new OrderId(id), new OrderLineId(orderLineId)),
                cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }
}