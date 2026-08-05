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

namespace Api.Controllers.Restaurants;

[ApiController]
[Route("v1/[controller]")]
public class RestaurantsController : MyBasicController
{
    private readonly ISender _mediator;

    public RestaurantsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gets a restaurant by id, including its menu and schedule.</summary>
    /// <param name="id">Restaurant id.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRestaurantByIdQuery(id), cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Registers a new restaurant, inactive by default.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRestaurantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateRestaurantCommand(request.Name, request.Description,
            request.Currency, request.Amount, ToScheduleTuples(request.Schedules)), cancellationToken);

        return !result.IsSuccess
            ? GetProblem(result.Error!)
            : CreatedAtAction(nameof(Get), new { id = result.Ok!.Id }, new { id = result.Ok!.Id });
    }

    /// <summary>Changes a restaurant's name.</summary>
    /// <param name="id">Restaurant id.</param>
    [HttpPatch("{id:guid}/name")]
    public async Task<IActionResult> ChangeName([FromRoute] Guid id, [FromBody] ChangeNameRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChangeRestaurantNameCommand(new RestaurantId(id), request.Name),
            cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Changes a restaurant's description.</summary>
    /// <param name="id">Restaurant id.</param>
    [HttpPatch("{id:guid}/description")]
    public async Task<IActionResult> ChangeDescription([FromRoute] Guid id, [FromBody] ChangeDescriptionRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(new ChangeRestaurantDescriptionCommand(new RestaurantId(id), request.Description),
                cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Replaces a restaurant's opening schedule.</summary>
    /// <param name="id">Restaurant id.</param>
    [HttpPatch("{id:guid}/schedule")]
    public async Task<IActionResult> ChangeSchedule([FromRoute] Guid id, [FromBody] ChangeScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ChangeRestaurantScheduleCommand(new RestaurantId(id), ToScheduleTuples(request.Schedules)),
            cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Sets the minimum price an order must reach before it can be placed at this restaurant.</summary>
    /// <param name="id">Restaurant id.</param>
    [HttpPatch("{id:guid}/minimal-order-price")]
    public async Task<IActionResult> SetMinimalOrderPrice([FromRoute] Guid id, [FromBody] MoneyRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(
                new SetMinimalOrderPriceCommand(new RestaurantId(id), request.Currency, request.Amount),
                cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Activates a restaurant, making it visible to customers.</summary>
    /// <param name="id">Restaurant id.</param>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ActivateRestaurantCommand(new RestaurantId(id)), cancellationToken);

        return !result.IsSuccess ? GetProblem(result.Error!) : NoContent();
    }

    /// <summary>Deactivates a restaurant, hiding it from customers.</summary>
    /// <param name="id">Restaurant id.</param>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateRestaurantCommand(new RestaurantId(id)), cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Adds a new item to a restaurant's menu.</summary>
    /// <param name="id">Restaurant id.</param>
    [HttpPost("{id:guid}/menu-items")]
    public async Task<IActionResult> AddMenuItem([FromRoute] Guid id, [FromBody] AddMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddMenuItemCommand(new RestaurantId(id), request.Name,
            request.Description, request.Currency, request.Amount), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Ok!.Id }, new { id = result.Ok!.Id })
            : GetProblem(result.Error!);
    }

    /// <summary>Removes an item from a restaurant's menu.</summary>
    /// <param name="id">Restaurant id.</param>
    /// <param name="menuItemId">Menu item id to remove.</param>
    [HttpDelete("{id:guid}/menu-items/{menuItemId:guid}")]
    public async Task<IActionResult> RemoveMenuItem([FromRoute] Guid id, [FromRoute] Guid menuItemId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveMenuItemCommand(new RestaurantId(id), new MenuItemId(menuItemId)),
            cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Changes a menu item's name.</summary>
    /// <param name="id">Restaurant id.</param>
    /// <param name="menuItemId">Menu item id.</param>
    [HttpPut("{id:guid}/menu-items/{menuItemId:guid}/name")]
    public async Task<IActionResult> ChangeMenuItemName([FromRoute] Guid id, [FromRoute] Guid menuItemId,
        [FromBody] ChangeNameRequest request, CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(new ChangeMenuItemNameCommand(new RestaurantId(id), new MenuItemId(menuItemId),
                request.Name), cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Changes a menu item's description.</summary>
    /// <param name="id">Restaurant id.</param>
    /// <param name="menuItemId">Menu item id.</param>
    [HttpPut("{id:guid}/menu-items/{menuItemId:guid}/description")]
    public async Task<IActionResult> ChangeMenuItemDescription([FromRoute] Guid id, [FromRoute] Guid menuItemId,
        [FromBody] ChangeDescriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChangeMenuItemDescriptionCommand(new RestaurantId(id),
            new MenuItemId(menuItemId), request.Description), cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    /// <summary>Changes a menu item's price.</summary>
    /// <param name="id">Restaurant id.</param>
    /// <param name="menuItemId">Menu item id.</param>
    [HttpPut("{id:guid}/menu-items/{menuItemId:guid}/price")]
    public async Task<IActionResult> ChangeMenuItemPrice([FromRoute] Guid id, [FromRoute] Guid menuItemId,
        [FromBody] MoneyRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChangeMenuItemPriceCommand(new RestaurantId(id),
            new MenuItemId(menuItemId), request.Currency, request.Amount), cancellationToken);

        return result.IsSuccess ? NoContent() : GetProblem(result.Error!);
    }

    private static List<(DayOfWeek OpenDay, TimeOnly OpenTime, DayOfWeek CloseDay, TimeOnly CloseTime)>
        ToScheduleTuples(List<OpeningWindowRequest> schedules) =>
        schedules.Select(w => (w.OpenDay, w.OpenTime, w.CloseDay, w.CloseTime)).ToList();
}
