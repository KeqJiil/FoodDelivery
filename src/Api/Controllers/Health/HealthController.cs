using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Health;

public class HealthController : ControllerBase
{
    [HttpGet("liveness")]
    public IActionResult Liveness()
    {
        return Ok();
    }

    [HttpGet("readiness")]
    public IActionResult Readiness()
    {
        throw new NotImplementedException();
    }
}