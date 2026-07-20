using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderRequests.Application.AcceptOrder;
using OrderRequests.Application.GetOrderRequestById;
using OrderRequests.Application.GetOrdersByRestaurantId;
using OrderRequests.Application.RejectOrder;
using OrderRequests.Domain.Enums;
using OrderRequests.Domain.Ids;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Api.Controllers.OrderRequests;

[ApiController]
[Route("v1/[controller]")]
public class OrderRequestsController : ControllerBase
{
    private readonly ISender _mediator;

    public OrderRequestsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("restaurant/{id:Guid}")]
    public async Task<IActionResult> GetOrdersByRestaurantId([FromRoute] Guid id, [FromQuery] ByRestaurantIdBody body)
    {
        var result = await _mediator.Send(new GetOrdersByRestaurantIdQuery(id, body.Cursor, body.Limit, body.Status));
        return result.Any() ? Ok(result) : NotFound();
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetOrder([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetOrderRequestByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("reject/{id:Guid}")]
    public async Task<IActionResult> RejectOrder([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new RejectOrderCommand(new OrderRequestId(id)));
        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPost("approve/{id:Guid}")]
    public async Task<IActionResult> ApproveOrder([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new AcceptOrderCommand(new OrderRequestId(id)));
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

public record ByRestaurantIdBody(byte Limit, OrderRequestStatus? Status, Guid? Cursor);