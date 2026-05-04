using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Funcionario;
using Renova.Service.Parameters.Funcionario;
using Renova.Service.Services.Acesso;
using Renova.Service.Services.Funcionario;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/funcionario")]
    [Authorize]
    public class FuncionarioController(
        IFuncionarioService funcionarioService,
        ILojaAuthorizationService authorizationService,
        RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly IFuncionarioService _funcionarioService = funcionarioService;
        private readonly ILojaAuthorizationService _authorizationService = authorizationService;

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<FuncionarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFuncionarios([FromQuery] int lojaId, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                IReadOnlyList<FuncionarioDto> resultado = await _funcionarioService.GetAllAsync(
                    new ObterFuncionariosParametros
                    {
                        UsuarioAutenticadoId = usuarioId.Value,
                        LojaId = lojaId
                    },
                    cancellationToken);

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(FuncionarioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostFuncionario(
            [FromQuery] int lojaId,
            [FromBody] CriarFuncionarioCommand command,
            CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                FuncionarioDto resultado = await _funcionarioService.CreateAsync(
                    command,
                    new CriarFuncionarioParametros
                    {
                        UsuarioAutenticadoId = usuarioId.Value,
                        LojaId = lojaId
                    },
                    cancellationToken);

                return Created(string.Empty, resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpGet("acesso")]
        [ProducesResponseType(typeof(AcessoLojaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAcessoLoja([FromQuery] int lojaId, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                AcessoLojaDto resultado = await _authorizationService.GetAccessAsync(
                    lojaId,
                    usuarioId.Value,
                    cancellationToken);

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(FuncionarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutFuncionario(
            [FromQuery] int lojaId,
            [FromQuery] int usuarioId,
            [FromBody] AtualizarFuncionarioCargoCommand command,
            CancellationToken cancellationToken)
        {
            int? usuarioAutenticadoId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioAutenticadoId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                FuncionarioDto resultado = await _funcionarioService.UpdateCargoAsync(
                    command,
                    new ExcluirFuncionarioParametros
                    {
                        LojaId = lojaId,
                        UsuarioAutenticadoId = usuarioAutenticadoId.Value,
                        UsuarioId = usuarioId
                    },
                    cancellationToken);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFuncionario(
            [FromQuery] int lojaId,
            [FromQuery] int usuarioId,
            CancellationToken cancellationToken)
        {
            int? usuarioAutenticadoId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioAutenticadoId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                await _funcionarioService.DeleteAsync(
                    new ExcluirFuncionarioParametros
                    {
                        UsuarioAutenticadoId = usuarioAutenticadoId.Value,
                        LojaId = lojaId,
                        UsuarioId = usuarioId
                    },
                    cancellationToken);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }
    }
}
