using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Restaurants;

/// <summary>Replaces a restaurant's opening schedule.</summary>
/// <param name="Schedules">Full list of opening windows; replaces the existing schedule entirely.</param>
public sealed record ChangeScheduleRequest([Required] List<OpeningWindowRequest> Schedules);
