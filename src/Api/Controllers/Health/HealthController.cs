using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Controllers.Health;

[ApiController]
[DisableRateLimiting]
public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    /// <summary>Liveness probe: returns 200 as soon as the process is running, regardless of dependency health.</summary>
    [HttpGet("liveness")]
    public IActionResult Liveness()
    {
        return Ok();
    }

    /// <summary>Readiness probe: checks every module's database is reachable, returning 503 if any is down.</summary>
    [HttpGet("readiness")]
    public async Task<IActionResult> Readiness(CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);

        return report.Status == HealthStatus.Healthy
            ? Ok(report.Status.ToString())
            : StatusCode(StatusCodes.Status503ServiceUnavailable, report.Status.ToString());
    }
}
