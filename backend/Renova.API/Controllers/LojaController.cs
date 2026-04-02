using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Loja;
using Renova.Service.Parameters.Loja;
using Renova.Service.Services.Loja;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/loja")]
    [Authorize]
    public class LojaController(ILojaService lojaService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly ILojaService _lojaService = lojaService;

        [HttpPost]
        [ProducesResponseType(typeof(LojaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostLoja([FromBody] CriarLojaCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                LojaDto resultado = await _lojaService.CreateAsync(
                    command,
                    new CriarLojaParametros
                    {
                        UsuarioId = usuarioId.Value
                    },
                    cancellationToken);

                return Created(string.Empty, resultado);
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