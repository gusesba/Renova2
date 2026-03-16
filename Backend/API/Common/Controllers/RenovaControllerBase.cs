using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Responses;

namespace Renova.Api.Common.Controllers;

[ApiController]
public abstract class RenovaControllerBase : ControllerBase
{
    protected ActionResult<ApiEnvelope<T>> OkEnvelope<T>(T data)
    {
        return Ok(ApiEnvelope<T>.CreateSuccess(data, HttpContext.TraceIdentifier));
    }
}
