using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Ordering;

/// <summary>Adds a menu item to a draft order as a new order line.</summary>
/// <param name="MenuId">Id of the menu item to add.</param>
public sealed record AddOrderLineRequest(
    [Required] Guid MenuId
);