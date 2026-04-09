using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Queries.Usuario;
using Renova.Service.Services.Usuario;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    [Authorize]
    public class UsuarioController(IUsuarioService usuarioService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly IUsuarioService _usuarioService = usuarioService;

        [HttpGet]
        [ProducesResponseType(typeof(PaginacaoDto<UsuarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsuarios([FromQuery] ObterUsuariosQuery query, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                PaginacaoDto<UsuarioDto> resultado = await _usuarioService.GetAllAsync(query, usuarioId.Value, cancellationToken);
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }
    }
}
