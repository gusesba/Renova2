using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Parameters.Pagamento;
using Renova.Service.Services.Pagamento;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/pagamento")]
    [Authorize]
    public class PagamentoController(IPagamentoService pagamentoService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly IPagamentoService _pagamentoService = pagamentoService;

        [HttpPost("credito")]
        [ProducesResponseType(typeof(PagamentoCreditoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostPagamentoCredito([FromBody] CriarPagamentoCreditoCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                PagamentoCreditoDto resultado = await _pagamentoService.CreateCreditoAsync(
                    command,
                    new CriarPagamentoCreditoParametros
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
                return Conflict(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }
    }
}
