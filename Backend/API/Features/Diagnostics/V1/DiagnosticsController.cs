using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;

namespace Renova.Api.Features.Diagnostics.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/diagnostics")]
public sealed class DiagnosticsController(
    IHostEnvironment environment) : RenovaControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiEnvelope<ApplicationStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult<ApiEnvelope<ApplicationStatusResponse>> GetStatus()
    {
        var response = new ApplicationStatusResponse(
            "Renova API",
            environment.EnvironmentName,
            "v1",
            DateTimeOffset.UtcNow);

        return OkEnvelope(response);
    }
}
