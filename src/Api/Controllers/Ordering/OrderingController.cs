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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateOrderCommand(new RestaurantRefId(request.RestaurantId)),
            cancellationToken);

        if (!result.IsSuccess) return GetProblem(result.Error!);

        return CreatedAtAction(nameof(GetById), new { id = result.Ok!.Id }, result.Ok);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelOrderCommand(new OrderId(id)), cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    [HttpPost("{id:guid}/place")]
    public async Task<IActionResult> PlaceOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PlaceOrderCommand(new OrderId(id)), cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    [HttpPost("{id:guid}/add-items")]
    public async Task<IActionResult> AddOrderLineItem([FromRoute] Guid id, [FromBody] AddOrderLineRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(new AddOrderLineItemCommand(new OrderId(id), new MenuItemRefId(request.MenuId)),
                cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Ok }, null)
            : GetProblem(result.Error!);
    }

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