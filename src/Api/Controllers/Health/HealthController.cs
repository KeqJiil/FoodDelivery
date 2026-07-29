using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Controllers.Health;

public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet("liveness")]
    public IActionResult Liveness()
    {
        return Ok();
    }

    [HttpGet("readiness")]
    public async Task<IActionResult> Readiness(CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);

        return report.Status == HealthStatus.Healthy
            ? Ok(report.Status.ToString())
            : StatusCode(StatusCodes.Status503ServiceUnavailable, report.Status.ToString());
    }
}
