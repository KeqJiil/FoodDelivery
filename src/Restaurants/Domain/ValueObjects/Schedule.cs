namespace Restaurants.Domain.ValueObjects;

public record Schedule
{
    private readonly List<OpeningWindow> _openingWindows;

    public IReadOnlyList<OpeningWindow> OpeningWindows => _openingWindows.AsReadOnly();

    public Schedule(IEnumerable<OpeningWindow>? openingWindows = null)
    {
        _openingWindows = openingWindows?.ToList() ?? [];
    }

    private Schedule()
    {
        _openingWindows = [];
    }

    public bool IsOpenNow(DateTimeOffset moment)
    {
        return _openingWindows.Any(x => Contains(x, moment));
    }

    public Schedule AddOpeningWindow(OpeningWindow openingWindow)
    {
        return new Schedule([.._openingWindows, openingWindow]);
    }

    public Schedule RemoveOpeningWindow(OpeningWindow openingWindow)
    {
        return new Schedule(_openingWindows.Where(w => w != openingWindow).ToList());
    }

    private bool Contains(OpeningWindow w, DateTimeOffset moment)
    {
        var now = ToWeekMinutes(moment.DayOfWeek, TimeOnly.FromDateTime(moment.DateTime));
        var start = ToWeekMinutes(w.OpenDay, w.OpenTime);
        var end = ToWeekMinutes(w.CloseDay, w.CloseTime);
        return start <= end ? now >= start && now < end : now >= start || now < end;
    }

    private static int ToWeekMinutes(DayOfWeek day, TimeOnly time)
    {
        return (int)day * 1440 + time.Hour * 60 + time.Minute;
    }
}

public record OpeningWindow(DayOfWeek OpenDay, TimeOnly OpenTime, DayOfWeek CloseDay, TimeOnly CloseTime);