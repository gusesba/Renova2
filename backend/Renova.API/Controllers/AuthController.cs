using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Auth;
using Renova.Service.Services.Auth;

namespace Renova.API.Controllers
{
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
                UsuarioTokenDto resultado = await _authService.CreateAsync(command, cancellationToken);

                return Created(string.Empty, resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensagem = ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(UsuarioTokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostLogin([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            try
            {
                UsuarioTokenDto resultado = await _authService.LoginAsync(command, cancellationToken);

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("liberacao")]
        [ProducesResponseType(typeof(LiberacaoUsuarioStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLiberacao(CancellationToken cancellationToken)
        {
            string? usuarioIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return Unauthorized(new { mensagem = "Usuario nao identificado." });
            }

            LiberacaoUsuarioStatusDto resultado = await _authService.ObterLiberacaoAsync(usuarioId, cancellationToken);

            return Ok(resultado);
        }
    }
}
