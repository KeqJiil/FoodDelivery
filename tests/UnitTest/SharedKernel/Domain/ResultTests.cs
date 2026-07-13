using FluentAssertions;
using SharedKernel.Domain;

namespace SharedKernel.UnitTest.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ShouldSetIsSuccessTrue_AndSetOkValue()
    {
        var result = Result<int, int>.Success(1);
        result.IsSuccess.Should().BeTrue();
        result.Ok.Should().Be(1);
        result.Error.Should().Be(default);
    }

    [Fact]
    public void Fail_ShouldSetIsSuccessFalse_AndSetError()
    {
        var result = Result<int, int>.Fail(123);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(123);
        result.Ok.Should().Be(0);
    }
}