using System.ComponentModel.DataAnnotations;
using Restaurants.Domain.ValueObjects;

namespace Api.Controllers.Restaurants;

public sealed record ChangeScheduleRequest([Required] List<OpeningWindow> Schedules);
