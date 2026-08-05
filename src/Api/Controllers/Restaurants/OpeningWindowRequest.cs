namespace Api.Controllers.Restaurants;

/// <summary>A single recurring window during which a restaurant is open, e.g. Friday 12:00 through Monday 13:00.</summary>
/// <param name="OpenDay">Day of week the window opens.</param>
/// <param name="OpenTime">Time of day the window opens.</param>
/// <param name="CloseDay">Day of week the window closes. May differ from <paramref name="OpenDay"/> for windows that span midnight.</param>
/// <param name="CloseTime">Time of day the window closes.</param>
public sealed record OpeningWindowRequest(DayOfWeek OpenDay, TimeOnly OpenTime, DayOfWeek CloseDay, TimeOnly CloseTime);
