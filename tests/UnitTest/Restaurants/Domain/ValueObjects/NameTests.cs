using FluentAssertions;
using Restaurants.Domain.ValueObjects;

namespace Restaurants.UnitTest.Domain.ValueObjects;

public class NameTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("A valid restaurant name")]
    public void Create_ShouldSucceed_WhenLengthWithinBounds(string value)
    {
        var result = Name.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Ok!.Data.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("this name is definitely way too long to be valid")]
    public void Create_ShouldFail_WhenLengthOutOfBounds(string value)
    {
        var result = Name.Create(value);

        result.IsSuccess.Should().BeFalse();
    }
}
