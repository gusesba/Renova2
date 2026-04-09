using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.ConfigLoja;
using Renova.Service.Parameters.ConfigLoja;
using Renova.Service.Services.ConfigLoja;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/config-loja")]
    [Authorize]
    public class ConfigLojaController(IConfigLojaService configLojaService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly IConfigLojaService _configLojaService = configLojaService;

        [HttpGet]
        [ProducesResponseType(typeof(ConfigLojaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfigLoja([FromQuery] int lojaId, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                ConfigLojaDto resultado = await _configLojaService.GetAsync(
                    lojaId,
                    new ObterConfigLojaParametros { UsuarioId = usuarioId.Value },
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

        [HttpPut]
        [ProducesResponseType(typeof(ConfigLojaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutConfigLoja([FromBody] SalvarConfigLojaCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                ConfigLojaDto resultado = await _configLojaService.SaveAsync(
                    command,
                    new SalvarConfigLojaParametros { UsuarioId = usuarioId.Value },
                    cancellationToken);

                return Ok(resultado);
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
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
