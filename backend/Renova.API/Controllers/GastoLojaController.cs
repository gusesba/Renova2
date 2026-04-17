using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.GastoLoja;
using Renova.Service.Parameters.GastoLoja;
using Renova.Service.Queries.GastoLoja;
using Renova.Service.Services.GastoLoja;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/gasto-loja")]
    [Authorize]
    public class GastoLojaController(IGastoLojaService gastoLojaService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly IGastoLojaService _gastoLojaService = gastoLojaService;

        [HttpGet]
        [ProducesResponseType(typeof(PaginacaoDto<GastoLojaBuscaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGastosLoja([FromQuery] ObterGastosLojaQuery query, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                PaginacaoDto<GastoLojaBuscaDto> resultado = await _gastoLojaService.GetAllAsync(
                    query,
                    new OperacaoGastoLojaParametros
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
        [ProducesResponseType(typeof(GastoLojaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostGastoLoja([FromBody] CriarGastoLojaCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                GastoLojaDto resultado = await _gastoLojaService.CreateAsync(
                    command,
                    new OperacaoGastoLojaParametros
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
