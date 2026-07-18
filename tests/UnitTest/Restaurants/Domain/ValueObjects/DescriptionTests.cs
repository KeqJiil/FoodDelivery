using FluentAssertions;
using Restaurants.Domain.ValueObjects;

namespace Restaurants.UnitTest.Domain.ValueObjects;

public class DescriptionTests
{
    [Fact]
    public void Create_ShouldSucceed_WhenLengthWithinBounds()
    {
        const string value = "Best pizza in town";

        var result = Description.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Ok!.Data.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("too short")]
    public void Create_ShouldFail_WhenTooShort(string value)
    {
        var result = Description.Create(value);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldFail_WhenTooLong()
    {
        var value = new string('a', 201);

        var result = Description.Create(value);

        result.IsSuccess.Should().BeFalse();
    }
}
