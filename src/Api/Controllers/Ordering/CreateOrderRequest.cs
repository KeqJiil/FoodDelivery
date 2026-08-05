using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Ordering;

/// <summary>Starts a new draft order for a restaurant.</summary>
/// <param name="RestaurantId">Id of the restaurant the order is placed with.</param>
public sealed record CreateOrderRequest([Required] Guid RestaurantId);