using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Produto;
using Renova.Service.Parameters.Produto;
using Renova.Service.Queries.Produto;
using Renova.Service.Services.Produto;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/produto")]
    [Authorize]
    public class ProdutoController(IProdutoService produtoService, RenovaDbContext context) : AuthenticatedControllerBase(context)
    {
        private readonly IProdutoService _produtoService = produtoService;

        [HttpGet]
        [ProducesResponseType(typeof(PaginacaoDto<ProdutoBuscaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProdutos([FromQuery] ObterProdutosQuery query, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                PaginacaoDto<ProdutoBuscaDto> resultado = await _produtoService.GetAllAsync(
                    query,
                    new ObterProdutosParametros
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

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProdutoBuscaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduto(int id, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                ProdutoBuscaDto resultado = await _produtoService.GetByIdAsync(
                    new ObterProdutoParametros
                    {
                        UsuarioId = usuarioId.Value,
                        ProdutoId = id
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

        [HttpGet("emprestados")]
        [ProducesResponseType(typeof(IReadOnlyList<ProdutoBuscaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProdutosEmprestadosDoCliente(
            [FromQuery] int lojaId,
            [FromQuery] int clienteId,
            CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                IReadOnlyList<ProdutoBuscaDto> resultado = await _produtoService.GetEmprestadosDoClienteAsync(
                    new ObterProdutosEmprestadosClienteParametros
                    {
                        UsuarioId = usuarioId.Value,
                        LojaId = lojaId,
                        ClienteId = clienteId
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
        [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostProduto([FromBody] CriarProdutoCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                ProdutoDto resultado = await _produtoService.CreateAsync(
                    command,
                    new CriarProdutoParametros
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

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutProduto(int id, [FromBody] EditarProdutoCommand command, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                ProdutoDto resultado = await _produtoService.EditAsync(
                    command,
                    new EditarProdutoParametros
                    {
                        UsuarioId = usuarioId.Value,
                        ProdutoId = id
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

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteProduto(int id, CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                await _produtoService.DeleteAsync(
                    new ExcluirProdutoParametros
                    {
                        UsuarioId = usuarioId.Value,
                        ProdutoId = id
                    },
                    cancellationToken);

                return NoContent();
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

        [HttpPost("referencia")]
        [ProducesResponseType(typeof(ProdutoAuxiliarDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public Task<IActionResult> PostProdutoAuxiliar([FromBody] CriarProdutoAuxiliarCommand command, CancellationToken cancellationToken)
        {
            return CriarAuxiliarAsync(
                (request, parametros, token) => _produtoService.CreateProdutoAuxiliarAsync(request, parametros, token),
                command,
                cancellationToken);
        }

        [HttpGet("referencia")]
        [ProducesResponseType(typeof(PaginacaoDto<ProdutoAuxiliarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> GetProdutoAuxiliar([FromQuery] ObterProdutoAuxiliarQuery query, CancellationToken cancellationToken)
        {
            return ObterAuxiliarAsync(
                (request, parametros, token) => _produtoService.GetProdutoAuxiliarAsync(request, parametros, token),
                query,
                cancellationToken);
        }

        [HttpPost("marca")]
        [ProducesResponseType(typeof(ProdutoAuxiliarDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public Task<IActionResult> PostMarca([FromBody] CriarProdutoAuxiliarCommand command, CancellationToken cancellationToken)
        {
            return CriarAuxiliarAsync(
                (request, parametros, token) => _produtoService.CreateMarcaAsync(request, parametros, token),
                command,
                cancellationToken);
        }

        [HttpGet("marca")]
        [ProducesResponseType(typeof(PaginacaoDto<ProdutoAuxiliarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> GetMarca([FromQuery] ObterProdutoAuxiliarQuery query, CancellationToken cancellationToken)
        {
            return ObterAuxiliarAsync(
                (request, parametros, token) => _produtoService.GetMarcaAsync(request, parametros, token),
                query,
                cancellationToken);
        }

        [HttpPost("tamanho")]
        [ProducesResponseType(typeof(ProdutoAuxiliarDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public Task<IActionResult> PostTamanho([FromBody] CriarProdutoAuxiliarCommand command, CancellationToken cancellationToken)
        {
            return CriarAuxiliarAsync(
                (request, parametros, token) => _produtoService.CreateTamanhoAsync(request, parametros, token),
                command,
                cancellationToken);
        }

        [HttpGet("tamanho")]
        [ProducesResponseType(typeof(PaginacaoDto<ProdutoAuxiliarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> GetTamanho([FromQuery] ObterProdutoAuxiliarQuery query, CancellationToken cancellationToken)
        {
            return ObterAuxiliarAsync(
                (request, parametros, token) => _produtoService.GetTamanhoAsync(request, parametros, token),
                query,
                cancellationToken);
        }

        [HttpPost("cor")]
        [ProducesResponseType(typeof(ProdutoAuxiliarDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public Task<IActionResult> PostCor([FromBody] CriarProdutoAuxiliarCommand command, CancellationToken cancellationToken)
        {
            return CriarAuxiliarAsync(
                (request, parametros, token) => _produtoService.CreateCorAsync(request, parametros, token),
                command,
                cancellationToken);
        }

        [HttpGet("cor")]
        [ProducesResponseType(typeof(PaginacaoDto<ProdutoAuxiliarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> GetCor([FromQuery] ObterProdutoAuxiliarQuery query, CancellationToken cancellationToken)
        {
            return ObterAuxiliarAsync(
                (request, parametros, token) => _produtoService.GetCorAsync(request, parametros, token),
                query,
                cancellationToken);
        }

        private async Task<IActionResult> CriarAuxiliarAsync(
            Func<CriarProdutoAuxiliarCommand, CriarProdutoAuxiliarParametros, CancellationToken, Task<ProdutoAuxiliarDto>> operacao,
            CriarProdutoAuxiliarCommand command,
            CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                ProdutoAuxiliarDto resultado = await operacao(
                    command,
                    new CriarProdutoAuxiliarParametros
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

        private async Task<IActionResult> ObterAuxiliarAsync(
            Func<ObterProdutoAuxiliarQuery, ObterProdutoAuxiliarParametros, CancellationToken, Task<PaginacaoDto<ProdutoAuxiliarDto>>> operacao,
            ObterProdutoAuxiliarQuery query,
            CancellationToken cancellationToken)
        {
            int? usuarioId = await ObterUsuarioIdAsync(cancellationToken);

            if (!usuarioId.HasValue)
            {
                return Unauthorized(new { mensagem = "Usuario autenticado invalido." });
            }

            try
            {
                PaginacaoDto<ProdutoAuxiliarDto> resultado = await operacao(
                    query,
                    new ObterProdutoAuxiliarParametros
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
    }
}
