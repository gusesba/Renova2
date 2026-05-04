using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Usuario;
using Renova.Service.Parameters.Usuario;
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
        private readonly RenovaDbContext _context = context;

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
                if (query.LojaId.HasValue)
                {
                    bool usuarioDonoDaLoja = _context.Lojas
                        .Any(loja => loja.Id == query.LojaId.Value && loja.UsuarioId == usuarioId.Value);

                    if (!usuarioDonoDaLoja)
                    {
                        return Unauthorized(new { mensagem = "Loja informada nao pertence ao usuario autenticado." });
                    }
                }

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

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] EditarUsuarioCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                UsuarioDto resultado = await _usuarioService.EditAsync(
                    command,
                    new EditarUsuarioParametros
                    {
                        UsuarioAutenticadoId = usuarioId.Value,
                        UsuarioId = id
                    },
                    cancellationToken);

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
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
