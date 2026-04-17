using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Solicitacao;
using Renova.Service.Parameters.Solicitacao;
using Renova.Service.Queries.Solicitacao;
using Renova.Service.Services.Solicitacao;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/solicitacao")]
    [Authorize]
    public class SolicitacaoController(ISolicitacaoService solicitacaoService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly ISolicitacaoService _solicitacaoService = solicitacaoService;

        [HttpGet]
        [ProducesResponseType(typeof(PaginacaoDto<SolicitacaoBuscaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSolicitacoes([FromQuery] ObterSolicitacoesQuery query, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                PaginacaoDto<SolicitacaoBuscaDto> resultado = await _solicitacaoService.GetAllAsync(
                    query,
                    new ObterSolicitacoesParametros
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
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(SolicitacaoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostSolicitacao([FromBody] CriarSolicitacaoCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                SolicitacaoDto resultado = await _solicitacaoService.CreateAsync(
                    command,
                    new CriarSolicitacaoParametros
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
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = ex.Message });
            }
        }
    }
}
