using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Ordering;

public sealed record CreateOrderRequest([Required] Guid RestaurantId);