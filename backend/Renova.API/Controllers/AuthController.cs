using Microsoft.AspNetCore.Mvc;
using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Auth;
using Renova.Service.Services.Auth;

namespace Renova.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("cadastro")]
    [ProducesResponseType(typeof(UsuarioTokenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostCadastro([FromBody] CadastroCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _authService.CreateAsync(command, cancellationToken);

            return Created(string.Empty, resultado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }
}
