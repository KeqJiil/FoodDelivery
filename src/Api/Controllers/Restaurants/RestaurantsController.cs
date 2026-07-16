using MediatR;
using Microsoft.AspNetCore.Mvc;
using Restaurants.Application.ActivateRestaurant;
using Restaurants.Application.AddMenuItem;
using Restaurants.Application.ChangeMenuItemDescription;
using Restaurants.Application.ChangeMenuItemName;
using Restaurants.Application.ChangeMenuItemPrice;
using Restaurants.Application.ChangeRestaurantDescription;
using Restaurants.Application.ChangeRestaurantName;
using Restaurants.Application.ChangeRestaurantSchedule;
using Restaurants.Application.CreateRestaurant;
using Restaurants.Application.DeactivateRestaurant;
using Restaurants.Application.GetRestaurantById;
using Restaurants.Application.RemoveMenuItem;
using Restaurants.Application.SetMinimalOrderPrice;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Api.Controllers.Restaurants;

[ApiController]
[Route("v1/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly ISender _mediator;

    public RestaurantsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetRestaurantByIdQuery(id));

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateRestaurantRequest request)
    {
        var name = new Name(request.Name);
        var desc = new Description(request.Description);
        var money = new Money(request.Currency, request.Amount);
        var schedule = new Schedule(request.Schedules);
        var result = await _mediator.Send(new CreateRestaurantCommand(name, desc, money, schedule));

        return !result.IsSuccess ? MapError(result.Error!) : CreatedAtAction(nameof(Get), new { id = result.Ok }, null);
    }

    [HttpPut("{id:guid}/name")]
    public async Task<IActionResult> ChangeName([FromRoute] Guid id, [FromBody] ChangeNameRequest request)
    {
        var result = await _mediator.Send(new ChangeRestaurantNameCommand(new RestaurantId(id), new Name(request.Name)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPut("{id:guid}/description")]
    public async Task<IActionResult> ChangeDescription([FromRoute] Guid id, [FromBody] ChangeDescriptionRequest request)
    {
        var result = await _mediator.Send(new ChangeRestaurantDescriptionCommand(new RestaurantId(id), new Description(request.Description)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPut("{id:guid}/schedule")]
    public async Task<IActionResult> ChangeSchedule([FromRoute] Guid id, [FromBody] ChangeScheduleRequest request)
    {
        var result = await _mediator.Send(new ChangeRestaurantScheduleCommand(new RestaurantId(id), new Schedule(request.Schedules)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPut("{id:guid}/minimal-order-price")]
    public async Task<IActionResult> SetMinimalOrderPrice([FromRoute] Guid id, [FromBody] MoneyRequest request)
    {
        var result = await _mediator.Send(new SetMinimalOrderPriceCommand(new RestaurantId(id), new Money(request.Currency, request.Amount)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new ActivateRestaurantCommand(new RestaurantId(id)));

        return !result.IsSuccess ? MapError(result.Error!) : CreatedAtAction(nameof(Get), new { id }, null);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new DeactivateRestaurantCommand(new RestaurantId(id)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPost("{id:guid}/menu-items")]
    public async Task<IActionResult> AddMenuItem([FromRoute] Guid id, [FromBody] AddMenuItemRequest request)
    {
        var name = new Name(request.Name);
        var desc = new Description(request.Description);
        var money = new Money(request.Currency, request.Amount);
        var result = await _mediator.Send(new AddMenuItemCommand(new RestaurantId(id), name, desc, money));

        return result.IsSuccess ? Ok(result.Ok) : MapError(result.Error!);
    }

    [HttpDelete("{id:guid}/menu-items/{menuItemId:guid}")]
    public async Task<IActionResult> RemoveMenuItem([FromRoute] Guid id, [FromRoute] Guid menuItemId)
    {
        var result = await _mediator.Send(new RemoveMenuItemCommand(new RestaurantId(id), new MenuItemId(menuItemId)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPut("{id:guid}/menu-items/{menuItemId:guid}/name")]
    public async Task<IActionResult> ChangeMenuItemName([FromRoute] Guid id, [FromRoute] Guid menuItemId, [FromBody] ChangeNameRequest request)
    {
        var result = await _mediator.Send(new ChangeMenuItemNameCommand(new RestaurantId(id), new MenuItemId(menuItemId), new Name(request.Name)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPut("{id:guid}/menu-items/{menuItemId:guid}/description")]
    public async Task<IActionResult> ChangeMenuItemDescription([FromRoute] Guid id, [FromRoute] Guid menuItemId, [FromBody] ChangeDescriptionRequest request)
    {
        var result = await _mediator.Send(new ChangeMenuItemDescriptionCommand(new RestaurantId(id), new MenuItemId(menuItemId), new Description(request.Description)));

        return result.IsSuccess ? NoContent() : MapError(result.Error!);
    }

    [HttpPut("{id:guid}/menu-items/{menuItemId:guid}/price")]
    public async Task<IActionResult> ChangeMenuItemPrice([FromRoute] Guid id, [FromRoute] Guid menuItemId, [FromBody] MoneyRequest request)
    {
        var result = await _mediator.Send(new ChangeMenuItemPriceCommand(new RestaurantId(id), new MenuItemId(menuItemId), new Money(request.Currency, request.Amount)));

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

public sealed record CreateRestaurantRequest(string Name, string Description, decimal Amount, Currency Currency, List<OpeningWindow> Schedules);
public sealed record ChangeNameRequest(string Name);
public sealed record ChangeDescriptionRequest(string Description);
public sealed record ChangeScheduleRequest(List<OpeningWindow> Schedules);
public sealed record MoneyRequest(Currency Currency, decimal Amount);
public sealed record AddMenuItemRequest(string Name, string Description, Currency Currency, decimal Amount);
