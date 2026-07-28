using Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Api.UnitTest.Controllers;

public class MyBasicControllerTests
{
    private sealed class TestController : MyBasicController;
    
    private sealed class PassthroughProblemDetailsFactory : ProblemDetailsFactory
    {
        public override ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null,
            string? title = null, string? type = null, string? detail = null, string? instance = null) =>
            new()
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };

        public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext,
            Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelStateDictionary, int? statusCode = null,
            string? title = null, string? type = null, string? detail = null, string? instance = null) =>
            new(modelStateDictionary) { Status = statusCode, Title = title, Detail = detail, Instance = instance };
    }

    private static TestController CreateController(string path = "/api/orders/42")
    {
        var services = new ServiceCollection();
        services.AddSingleton<ProblemDetailsFactory, PassthroughProblemDetailsFactory>();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        httpContext.Request.Path = path;

        return new TestController { ControllerContext = new ControllerContext { HttpContext = httpContext } };
    }

    private static ProblemDetails Problem(IActionResult result)
    {
        result.Should().BeOfType<ObjectResult>();
        return ((ObjectResult)result).Value.Should().BeOfType<ProblemDetails>().Subject;
    }

    [Theory]
    [InlineData(ErrorEnum.NotFound, 404, "Not found")]
    [InlineData(ErrorEnum.Validation, 400, "Bad Request")]
    [InlineData(ErrorEnum.Conflict, 409, "Conflict")]
    [InlineData(ErrorEnum.NotAllowed, 403, "Not Allowed")]
    [InlineData(ErrorEnum.Unexpected, 500, "Internal Error")]
    public void GetProblem_ShouldMapErrorType_ToStatusAndTitle(ErrorEnum type, int expectedStatus,
        string expectedTitle)
    {
        var controller = CreateController();
        var error = new Error(type, "something went wrong");

        var problem = Problem(controller.GetProblem(error));

        problem.Status.Should().Be(expectedStatus);
        problem.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void GetProblem_ShouldFallBackToInternalError_ForUnknownErrorType()
    {
        var controller = CreateController();
        var error = new Error((ErrorEnum)999, "unknown");

        var problem = Problem(controller.GetProblem(error));

        problem.Status.Should().Be(500);
        problem.Title.Should().Be("Internal Error");
    }

    [Fact]
    public void GetProblem_ShouldUseErrorMessage_AsDetail()
    {
        var controller = CreateController();

        var problem = Problem(controller.GetProblem(Error.NotFound("Order 42 not found")));

        problem.Detail.Should().Be("Order 42 not found");
    }

    [Fact]
    public void GetProblem_ShouldUseRequestPath_AsInstance()
    {
        var controller = CreateController("/api/restaurants/7/menu");

        var problem = Problem(controller.GetProblem(Error.Conflict("busy")));

        problem.Instance.Should().Be("/api/restaurants/7/menu");
    }

    [Fact]
    public void GetProblem_ShouldProduceEmptyInstance_WhenRequestPathIsEmpty()
    {
        var controller = CreateController(string.Empty);

        var problem = Problem(controller.GetProblem(Error.Unexpected()));

        problem.Instance.Should().BeEmpty();
    }

    [Fact]
    public void GetProblem_ShouldSetObjectResultStatusCode_ToMappedStatus()
    {
        var controller = CreateController();

        var result = (ObjectResult)controller.GetProblem(Error.NotAllowed("nope"));

        result.StatusCode.Should().Be(403);
    }
}
