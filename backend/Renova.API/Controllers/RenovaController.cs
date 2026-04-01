using Microsoft.AspNetCore.Mvc;
using Renova.Domain.Model;
using Renova.Service.Commands;
using Renova.Service.Queries;
using Renova.Service.Services;

namespace Renova.API.Controllers;

[ApiController]
[Route("api/renova")]
public class RenovaController(IRenovaService renovaService) : ControllerBase
{
    private readonly IRenovaService _renovaService = renovaService;

    [HttpGet]
    [ProducesResponseType(typeof(RenovaModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetRenova([FromQuery] RenovaQuery request, CancellationToken cancellationToken)
    {
        var renova = await _renovaService.GetAsync(request, cancellationToken);
        if (renova is null)
        {
            return NoContent();
        }

        return Ok(renova);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RenovaModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> PostRenova([FromBody] RenovaCommand command, CancellationToken cancellationToken)
    {
        var renova = await _renovaService.CreateAsync(command, cancellationToken);

        return Created(string.Empty, renova);
    }
}
