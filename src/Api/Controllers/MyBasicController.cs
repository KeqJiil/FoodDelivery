using Microsoft.AspNetCore.Mvc;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;

namespace Api.Controllers;

public abstract class MyBasicController : ControllerBase
{
    public IActionResult GetProblem(Error error)
    {
        var title = error.Type switch
        {
            ErrorEnum.NotFound => "Not found",
            ErrorEnum.Validation => "Bad Request",
            ErrorEnum.Conflict => "Conflict",
            ErrorEnum.NotAllowed => "Not Allowed",
            _ => "Internal Error"
        };

        var status = error.Type switch
        {
            ErrorEnum.NotFound => 404,
            ErrorEnum.Validation => 400,
            ErrorEnum.Conflict => 409,
            ErrorEnum.NotAllowed => 403,
            _ => 500
        };

        var instance = HttpContext.Request.Path;

        return Problem(error.Message, instance, status, title);
    }
}