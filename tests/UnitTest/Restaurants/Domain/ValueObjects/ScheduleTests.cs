using FluentAssertions;
using Restaurants.Domain.ValueObjects;

namespace Restaurants.UnitTest.Domain.ValueObjects;

public class ScheduleTests
{
    [Fact]
    public void IsOpenNow_ShouldBeFalse_WhenNoOpeningWindows()
    {
        var schedule = new Schedule();

        schedule.IsOpenNow(DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsOpenNow_ShouldBeTrue_WhenMomentFallsWithinSameDayWindow()
    {
        var schedule = new Schedule([new OpeningWindow(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0))]);
        var monday = new DateTimeOffset(2026, 7, 20, 12, 0, 0, TimeSpan.Zero);

        schedule.IsOpenNow(monday).Should().BeTrue();
    }

    [Fact]
    public void IsOpenNow_ShouldBeFalse_WhenMomentOutsideWindow()
    {
        var schedule = new Schedule([new OpeningWindow(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0))]);
        var mondayNight = new DateTimeOffset(2026, 7, 20, 23, 0, 0, TimeSpan.Zero);

        schedule.IsOpenNow(mondayNight).Should().BeFalse();
    }

    [Fact]
    public void IsOpenNow_ShouldBeTrue_WhenWindowWrapsAcrossWeekBoundary()
    {
        var schedule = new Schedule([new OpeningWindow(DayOfWeek.Saturday, new TimeOnly(22, 0), DayOfWeek.Sunday, new TimeOnly(2, 0))]);
        var sundayEarlyMorning = new DateTimeOffset(2026, 7, 19, 1, 0, 0, TimeSpan.Zero);

        schedule.IsOpenNow(sundayEarlyMorning).Should().BeTrue();
    }

    [Fact]
    public void IsOpenNow_ShouldBeFalse_WhenOutsideWrappedWindow()
    {
        var schedule = new Schedule([new OpeningWindow(DayOfWeek.Saturday, new TimeOnly(22, 0), DayOfWeek.Sunday, new TimeOnly(2, 0))]);
        var sundayAfternoon = new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);

        schedule.IsOpenNow(sundayAfternoon).Should().BeFalse();
    }

    [Fact]
    public void AddOpeningWindow_ShouldReturnNewScheduleWithWindowAdded()
    {
        var schedule = new Schedule();
        var window = new OpeningWindow(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0));

        var updated = schedule.AddOpeningWindow(window);

        updated.OpeningWindows.Should().ContainSingle().Which.Should().Be(window);
        schedule.OpeningWindows.Should().BeEmpty();
    }

    [Fact]
    public void RemoveOpeningWindow_ShouldReturnNewScheduleWithoutWindow()
    {
        var window = new OpeningWindow(DayOfWeek.Monday, new TimeOnly(9, 0), DayOfWeek.Monday, new TimeOnly(22, 0));
        var schedule = new Schedule([window]);

        var updated = schedule.RemoveOpeningWindow(window);

        updated.OpeningWindows.Should().BeEmpty();
        schedule.OpeningWindows.Should().ContainSingle();
    }
}
