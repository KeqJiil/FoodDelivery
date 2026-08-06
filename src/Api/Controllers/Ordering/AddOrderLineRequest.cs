using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Ordering;

/// <summary>Adds a menu item to a draft order as a new order line.</summary>
/// <param name="MenuId">Id of the menu item to add.</param>
public sealed record AddOrderLineRequest(
    [Required] Guid MenuId
)
{
    public static AddOrderLineRequest Example { get; } = new(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
}