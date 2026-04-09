using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Movimentacao;
using Renova.Service.Parameters.Movimentacao;
using Renova.Service.Queries.Movimentacao;
using Renova.Service.Services.Movimentacao;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/movimentacao")]
    [Authorize]
    public class MovimentacaoController(IMovimentacaoService movimentacaoService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly IMovimentacaoService _movimentacaoService = movimentacaoService;

        [HttpGet]
        [ProducesResponseType(typeof(PaginacaoDto<MovimentacaoBuscaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMovimentacoes([FromQuery] ObterMovimentacoesQuery query, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                PaginacaoDto<MovimentacaoBuscaDto> resultado = await _movimentacaoService.GetAllAsync(
                    query,
                    new ObterMovimentacoesParametros
                    {
                        UsuarioId = usuarioId.Value
                    },
                    cancellationToken);

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

        [HttpPost]
        [ProducesResponseType(typeof(MovimentacaoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostMovimentacao([FromBody] CriarMovimentacaoCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                MovimentacaoDto resultado = await _movimentacaoService.CreateAsync(
                    command,
                    new CriarMovimentacaoParametros
                    {
                        UsuarioId = usuarioId.Value
                    },
                    cancellationToken);

                return Created(string.Empty, resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
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
