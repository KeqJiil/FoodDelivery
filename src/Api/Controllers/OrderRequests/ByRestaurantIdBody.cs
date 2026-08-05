using System.ComponentModel.DataAnnotations;
using OrderRequests.Domain.Enums;

namespace Api.Controllers.OrderRequests;

/// <summary>Cursor-pagination query parameters for listing a restaurant's order requests.</summary>
/// <param name="Limit">Max number of results per page.</param>
/// <param name="Status">Optional status filter.</param>
/// <param name="CursorCreatedAt">Creation timestamp of the last item from the previous page; omit for the first page.</param>
/// <param name="CursorId">Id of the last item from the previous page, used as a tie-break when timestamps collide; omit for the first page.</param>
public record ByRestaurantIdBody(
    [Range(1, byte.MaxValue)] byte Limit,
    [EnumDataType(typeof(OrderRequestStatus))]
    OrderRequestStatus? Status,
    DateTime? CursorCreatedAt,
    Guid? CursorId);