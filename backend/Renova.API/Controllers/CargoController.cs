using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cargo;
using Renova.Service.Parameters.Cargo;
using Renova.Service.Services.Cargo;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/cargo")]
    [Authorize]
    public class CargoController(ICargoService cargoService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly ICargoService _cargoService = cargoService;

        [HttpGet]
        public async Task<IActionResult> GetCargos([FromQuery] int lojaId, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                IReadOnlyList<CargoDto> resultado = await _cargoService.GetAllAsync(
                    new OperacaoCargoParametros
                    {
                        LojaId = lojaId,
                        UsuarioId = usuarioId.Value
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpGet("funcionalidade")]
        public async Task<IActionResult> GetFuncionalidades([FromQuery] int lojaId, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                IReadOnlyList<FuncionalidadeDto> resultado = await _cargoService.GetFuncionalidadesAsync(
                    new OperacaoCargoParametros
                    {
                        LojaId = lojaId,
                        UsuarioId = usuarioId.Value
                    },
                    cancellationToken);

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostCargo([FromQuery] int lojaId, [FromBody] CriarCargoCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                CargoDto resultado = await _cargoService.CreateAsync(
                    command,
                    new OperacaoCargoParametros
                    {
                        LojaId = lojaId,
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCargo(int id, [FromQuery] int lojaId, [FromBody] EditarCargoCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                CargoDto resultado = await _cargoService.EditAsync(
                    command,
                    new OperacaoCargoParametros
                    {
                        CargoId = id,
                        LojaId = lojaId,
                        UsuarioId = usuarioId.Value
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCargo(int id, [FromQuery] int lojaId, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                await _cargoService.DeleteAsync(
                    new OperacaoCargoParametros
                    {
                        CargoId = id,
                        LojaId = lojaId,
                        UsuarioId = usuarioId.Value
                    },
                    cancellationToken);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
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
    }
}
