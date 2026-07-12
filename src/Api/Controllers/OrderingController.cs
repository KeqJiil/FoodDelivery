using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.AddOrderLineItem;
using Ordering.Application.CancelOrder;
using Ordering.Application.CreateOrder;
using Ordering.Application.GetOrderById;
using Ordering.Application.PlaceOrder;
using Ordering.Application.RemoveOrderLineItem;
using Ordering.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class OrderingController : ControllerBase
{
    private readonly ISender _mediator;

    public OrderingController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _mediator.Send(new CreateOrderCommand(new RestaurantRefId(request.RestaurantId)));

        if (!result.IsSuccess) return MapError(result.Error!);

        return CreatedAtAction(nameof(GetById), new { id = result.Ok!.Id }, result.Ok);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new CancelOrderCommand(new OrderId(id)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPost("{id:guid}/place")]
    public async Task<IActionResult> PlaceOrder([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new PlaceOrderCommand(new OrderId(id)));

        return result.IsSuccess ? Ok(result.Ok) : MapError(result.Error!);
    }

    [HttpPost("{id:guid}/add-items")]
    public async Task<IActionResult> AddOrderLineItem([FromRoute] Guid id, [FromBody] AddOrderLineRequest request)
    {
        var result = await _mediator.Send(new AddOrderLineItemCommand(new OrderId(id), new MenuItemRefId(request.menuId)));

        return result.IsSuccess ? Ok(result.Ok) : MapError(result.Error!);
    }

    [HttpDelete("{id:guid}/remove")]
    public async Task<IActionResult> RemoveOrderLineItem([FromRoute] Guid id, [FromBody] RemoveOrderLineRequest request)
    {
        var result = await _mediator.Send(new RemoveOrderLineItemCommand(new OrderId(id), new OrderLineId(request.orderlineId)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    private ObjectResult MapError(Error error)
    {
        return error.Type switch
        {
            ErrorEnum.NotFound => NotFound(error.Message),
            ErrorEnum.Validation => BadRequest(error.Message),
            ErrorEnum.Conflict => Conflict(error.Message),
            ErrorEnum.NotAllowed => StatusCode(StatusCodes.Status403Forbidden, error.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, error.Message)
        };
    }
}

public sealed record CreateOrderRequest(Guid RestaurantId);

public sealed record AddOrderLineRequest(Guid menuId);
public sealed record RemoveOrderLineRequest(Guid orderlineId);