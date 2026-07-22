using System.ComponentModel.DataAnnotations;
using OrderRequests.Domain.Enums;

namespace Api.Controllers.OrderRequests;

public record ByRestaurantIdBody(
    [Range(1, byte.MaxValue)] byte Limit,
    [EnumDataType(typeof(OrderRequestStatus))]
    OrderRequestStatus? Status,
    Guid? Cursor);