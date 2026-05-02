using Microsoft.EntityFrameworkCore;

using Renova.Domain.Access;
using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Produto;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Produto;
using Renova.Service.Queries.Produto;
using Renova.Service.Services.Acesso;
using System.Linq.Expressions;

namespace Renova.Service.Services.Produto
{
    public class ProdutoService(RenovaDbContext context, ILojaAuthorizationService? authorizationService = null) : IProdutoService
    {
        private readonly RenovaDbContext _context = context;
        private readonly ILojaAuthorizationService _authorizationService = authorizationService ?? NullLojaAuthorizationService.Instance;
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<ProdutoEstoqueModel, int>>)(produto => produto.Id),
            ["etiqueta"] = (Expression<Func<ProdutoEstoqueModel, int>>)(produto => produto.Etiqueta),
            ["descricao"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Descricao),
            ["preco"] = (Expression<Func<ProdutoEstoqueModel, decimal>>)(produto => produto.Preco),
            ["entrada"] = (Expression<Func<ProdutoEstoqueModel, DateTime>>)(produto => produto.Entrada),
            ["produto"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Produto != null ? produto.Produto.Valor : string.Empty),
            ["marca"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Marca != null ? produto.Marca.Valor : string.Empty),
            ["tamanho"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Tamanho != null ? produto.Tamanho.Valor : string.Empty),
            ["cor"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Cor != null ? produto.Cor.Valor : string.Empty),
            ["fornecedor"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Fornecedor != null ? produto.Fornecedor.Nome : string.Empty)
        };
        public async Task<ProdutoDto> CreateAsync(CriarProdutoCommand request, CriarProdutoParametros parametros, CancellationToken cancellationToken = default)
        {
            if (request.Preco <= 0)
            {
                throw new ArgumentException("Preco deve ser maior que zero.", nameof(request));
            }

            if (request.Quantidade <= 0)
            {
                throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(request));
            }

            string descricaoNormalizada = request.Descricao.Trim();

            await _authorizationService.EnsurePermissionAsync(request.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.ProdutosAdicionar, cancellationToken);
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            ClienteModel fornecedor = await _context.Clientes
                .SingleOrDefaultAsync(cliente => cliente.Id == request.FornecedorId, cancellationToken)
                ?? throw new ArgumentException("Fornecedor informado nao foi encontrado.", nameof(request));

            if (fornecedor.LojaId != loja.Id)
            {
                throw new ArgumentException("Fornecedor informado nao pertence a loja selecionada.", nameof(request));
            }

            ProdutoReferenciaModel produtoReferencia = await _context.ProdutosReferencia
                .SingleOrDefaultAsync(produto => produto.Id == request.ProdutoId, cancellationToken)
                ?? throw new ArgumentException("Produto informado nao foi encontrado.", nameof(request));

            MarcaModel marca = await _context.Marcas
                .SingleOrDefaultAsync(item => item.Id == request.MarcaId, cancellationToken)
                ?? throw new ArgumentException("Marca informada nao foi encontrada.", nameof(request));

            TamanhoModel tamanho = await _context.Tamanhos
                .SingleOrDefaultAsync(item => item.Id == request.TamanhoId, cancellationToken)
                ?? throw new ArgumentException("Tamanho informado nao foi encontrado.", nameof(request));

            CorModel cor = await _context.Cores
                .SingleOrDefaultAsync(item => item.Id == request.CorId, cancellationToken)
                ?? throw new ArgumentException("Cor informada nao foi encontrada.", nameof(request));

            if (produtoReferencia.LojaId != loja.Id || marca.LojaId != loja.Id || tamanho.LojaId != loja.Id || cor.LojaId != loja.Id)
            {
                throw new ArgumentException("Os registros auxiliares informados devem pertencer a loja selecionada.", nameof(request));
            }

            if (!Enum.IsDefined(request.Situacao))
            {
                throw new ArgumentException("Situacao informada e invalida.", nameof(request));
            }

            DateTime entrada = request.Entrada == default ? DateTime.UtcNow : request.Entrada;
            int etiquetaInicial = await ObterEtiquetaInicialAsync(request.LojaId, request.Etiqueta, request.Quantidade, null, cancellationToken);

            List<ProdutoEstoqueModel> produtos = Enumerable.Range(0, request.Quantidade)
                .Select(index => new ProdutoEstoqueModel
                {
                    Etiqueta = etiquetaInicial + index,
                    Preco = request.Preco,
                    ProdutoId = request.ProdutoId,
                    MarcaId = request.MarcaId,
                    TamanhoId = request.TamanhoId,
                    CorId = request.CorId,
                    FornecedorId = request.FornecedorId,
                    Descricao = descricaoNormalizada,
                    Entrada = entrada,
                    LojaId = request.LojaId,
                    Situacao = request.Situacao,
                    Consignado = request.Consignado
                })
                .ToList();

            await _context.ProdutosEstoque.AddRangeAsync(produtos, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            ProdutoEstoqueModel produtoCriado = produtos[0];
            ProdutoDto resultado = MapearProdutoDto(produtoCriado);
            resultado.SolicitacoesCompativeis = await ObterSolicitacoesCompativeisCoreAsync(produtoCriado, cancellationToken);
            return resultado;
        }

        public async Task<ProdutoDto> EditAsync(EditarProdutoCommand request, EditarProdutoParametros parametros, CancellationToken cancellationToken = default)
        {
            if (request.Preco <= 0)
            {
                throw new ArgumentException("Preco deve ser maior que zero.", nameof(request));
            }

            string descricaoNormalizada = request.Descricao.Trim();

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            ProdutoEstoqueModel produto = await _context.ProdutosEstoque
                .SingleOrDefaultAsync(produtoAtual => produtoAtual.Id == parametros.ProdutoId, cancellationToken)
                ?? throw new KeyNotFoundException("Produto informado nao foi encontrado.");

            await _authorizationService.EnsurePermissionAsync(produto.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.ProdutosEditar, cancellationToken);
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(produto.LojaId, parametros.UsuarioId, cancellationToken);

            ClienteModel fornecedor = await _context.Clientes
                .SingleOrDefaultAsync(cliente => cliente.Id == request.FornecedorId, cancellationToken)
                ?? throw new ArgumentException("Fornecedor informado nao foi encontrado.", nameof(request));

            if (fornecedor.LojaId != loja.Id)
            {
                throw new ArgumentException("Fornecedor informado nao pertence a loja selecionada.", nameof(request));
            }

            ProdutoReferenciaModel produtoReferencia = await _context.ProdutosReferencia
                .SingleOrDefaultAsync(item => item.Id == request.ProdutoId, cancellationToken)
                ?? throw new ArgumentException("Produto informado nao foi encontrado.", nameof(request));

            MarcaModel marca = await _context.Marcas
                .SingleOrDefaultAsync(item => item.Id == request.MarcaId, cancellationToken)
                ?? throw new ArgumentException("Marca informada nao foi encontrada.", nameof(request));

            TamanhoModel tamanho = await _context.Tamanhos
                .SingleOrDefaultAsync(item => item.Id == request.TamanhoId, cancellationToken)
                ?? throw new ArgumentException("Tamanho informado nao foi encontrado.", nameof(request));

            CorModel cor = await _context.Cores
                .SingleOrDefaultAsync(item => item.Id == request.CorId, cancellationToken)
                ?? throw new ArgumentException("Cor informada nao foi encontrada.", nameof(request));

            if (produtoReferencia.LojaId != loja.Id || marca.LojaId != loja.Id || tamanho.LojaId != loja.Id || cor.LojaId != loja.Id)
            {
                throw new ArgumentException("Os registros auxiliares informados devem pertencer a loja selecionada.", nameof(request));
            }

            if (!Enum.IsDefined(request.Situacao))
            {
                throw new ArgumentException("Situacao informada e invalida.", nameof(request));
            }

            if (!string.IsNullOrWhiteSpace(request.Etiqueta))
            {
                produto.Etiqueta = await ObterEtiquetaInicialAsync(produto.LojaId, request.Etiqueta, 1, produto.Id, cancellationToken);
            }

            produto.Preco = request.Preco;
            produto.ProdutoId = request.ProdutoId;
            produto.MarcaId = request.MarcaId;
            produto.TamanhoId = request.TamanhoId;
            produto.CorId = request.CorId;
            produto.FornecedorId = request.FornecedorId;
            produto.Descricao = descricaoNormalizada;
            produto.Entrada = request.Entrada == default ? produto.Entrada : request.Entrada;
            produto.Situacao = request.Situacao;
            produto.Consignado = request.Consignado;

            _ = await _context.SaveChangesAsync(cancellationToken);

            ProdutoDto resultado = MapearProdutoDto(produto);
            resultado.SolicitacoesCompativeis = await ObterSolicitacoesCompativeisCoreAsync(produto, cancellationToken);
            return resultado;
        }

        public async Task DeleteAsync(ExcluirProdutoParametros parametros, CancellationToken cancellationToken = default)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            ProdutoEstoqueModel produto = await _context.ProdutosEstoque
                .SingleOrDefaultAsync(produtoAtual => produtoAtual.Id == parametros.ProdutoId, cancellationToken)
                ?? throw new KeyNotFoundException("Produto informado nao foi encontrado.");

            await _authorizationService.EnsurePermissionAsync(produto.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.ProdutosExcluir, cancellationToken);

            if (await ProdutoPossuiRelacionamentosAtivosAsync(produto.Id, cancellationToken))
            {
                throw new InvalidOperationException("Produto possui movimentacoes vinculadas e nao pode ser excluido.");
            }

            _ = _context.ProdutosEstoque.Remove(produto);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<ProdutoAuxiliarDto> CreateProdutoAuxiliarAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return CriarAuxiliarAsync(
                request,
                parametros,
                _context.ProdutosReferencia,
                valor => new ProdutoReferenciaModel
                {
                    Valor = valor,
                    LojaId = request.LojaId
                },
                "Produto",
                cancellationToken);
        }

        public Task DeleteProdutoAuxiliarAsync(ExcluirProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return ExcluirAuxiliarAsync(
                parametros,
                _context.ProdutosReferencia,
                produtoAuxiliarId => _context.ProdutosEstoque.AnyAsync(item => item.ProdutoId == produtoAuxiliarId, cancellationToken),
                "Produto",
                cancellationToken);
        }

        public Task<ProdutoAuxiliarDto> CreateMarcaAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return CriarAuxiliarAsync(
                request,
                parametros,
                _context.Marcas,
                valor => new MarcaModel
                {
                    Valor = valor,
                    LojaId = request.LojaId
                },
                "Marca",
                cancellationToken);
        }

        public Task DeleteMarcaAsync(ExcluirProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return ExcluirAuxiliarAsync(
                parametros,
                _context.Marcas,
                produtoAuxiliarId => _context.ProdutosEstoque.AnyAsync(item => item.MarcaId == produtoAuxiliarId, cancellationToken),
                "Marca",
                cancellationToken);
        }

        public Task<ProdutoAuxiliarDto> CreateTamanhoAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return CriarAuxiliarAsync(
                request,
                parametros,
                _context.Tamanhos,
                valor => new TamanhoModel
                {
                    Valor = valor,
                    LojaId = request.LojaId
                },
                "Tamanho",
                cancellationToken);
        }

        public Task DeleteTamanhoAsync(ExcluirProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return ExcluirAuxiliarAsync(
                parametros,
                _context.Tamanhos,
                produtoAuxiliarId => _context.ProdutosEstoque.AnyAsync(item => item.TamanhoId == produtoAuxiliarId, cancellationToken),
                "Tamanho",
                cancellationToken);
        }

        public Task<ProdutoAuxiliarDto> CreateCorAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return CriarAuxiliarAsync(
                request,
                parametros,
                _context.Cores,
                valor => new CorModel
                {
                    Valor = valor,
                    LojaId = request.LojaId
                },
                "Cor",
                cancellationToken);
        }

        public Task DeleteCorAsync(ExcluirProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return ExcluirAuxiliarAsync(
                parametros,
                _context.Cores,
                produtoAuxiliarId => _context.ProdutosEstoque.AnyAsync(item => item.CorId == produtoAuxiliarId, cancellationToken),
                "Cor",
                cancellationToken);
        }

        public Task<PaginacaoDto<ProdutoAuxiliarDto>> GetProdutoAuxiliarAsync(ObterProdutoAuxiliarQuery request, ObterProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return ObterAuxiliaresAsync(request, parametros, _context.ProdutosReferencia, cancellationToken);
        }

        public Task<PaginacaoDto<ProdutoAuxiliarDto>> GetMarcaAsync(ObterProdutoAuxiliarQuery request, ObterProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return ObterAuxiliaresAsync(request, parametros, _context.Marcas, cancellationToken);
        }

        public Task<PaginacaoDto<ProdutoAuxiliarDto>> GetTamanhoAsync(ObterProdutoAuxiliarQuery request, ObterProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return ObterAuxiliaresAsync(request, parametros, _context.Tamanhos, cancellationToken);
        }

        public Task<PaginacaoDto<ProdutoAuxiliarDto>> GetCorAsync(ObterProdutoAuxiliarQuery request, ObterProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return ObterAuxiliaresAsync(request, parametros, _context.Cores, cancellationToken);
        }

        public async Task<PaginacaoDto<ProdutoBuscaDto>> GetAllAsync(ObterProdutosQuery request, ObterProdutosParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            await _authorizationService.EnsurePermissionAsync(request.LojaId.Value, parametros.UsuarioId, FuncionalidadeCatalogo.ProdutosVisualizar, cancellationToken);

            IQueryable<ProdutoEstoqueModel> query = _context.ProdutosEstoque
                .Where(produto => produto.LojaId == request.LojaId.Value);

            if (!string.IsNullOrWhiteSpace(request.Descricao))
            {
                string descricaoFiltro = request.Descricao.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Descricao.ToLower().Contains(descricaoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Etiqueta))
            {
                if (!int.TryParse(request.Etiqueta.Trim(), out int etiquetaFiltro))
                {
                    throw new ArgumentException("Etiqueta deve conter apenas numeros.", nameof(request));
                }

                query = query.Where(produto => produto.Etiqueta == etiquetaFiltro);
            }

            if (!string.IsNullOrWhiteSpace(request.Produto))
            {
                string produtoFiltro = request.Produto.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Produto != null && produto.Produto.Valor.ToLower().Contains(produtoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Marca))
            {
                string marcaFiltro = request.Marca.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Marca != null && produto.Marca.Valor.ToLower().Contains(marcaFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Tamanho))
            {
                string tamanhoFiltro = request.Tamanho.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Tamanho != null && produto.Tamanho.Valor.ToLower().Contains(tamanhoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Cor))
            {
                string corFiltro = request.Cor.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Cor != null && produto.Cor.Valor.ToLower().Contains(corFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Fornecedor))
            {
                string fornecedorFiltro = request.Fornecedor.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Fornecedor != null && produto.Fornecedor.Nome.ToLower().Contains(fornecedorFiltro));
            }

            if (request.PrecoInicial.HasValue)
            {
                query = query.Where(produto => produto.Preco >= request.PrecoInicial.Value);
            }

            if (request.PrecoFinal.HasValue)
            {
                query = query.Where(produto => produto.Preco <= request.PrecoFinal.Value);
            }

            if (request.DataInicial.HasValue)
            {
                DateTime dataInicialUtc = NormalizarDateTimeParaUtc(request.DataInicial.Value);
                query = query.Where(produto => produto.Entrada >= dataInicialUtc);
            }

            if (request.DataFinal.HasValue)
            {
                DateTime dataFinalUtc = NormalizarDateTimeParaUtc(request.DataFinal.Value);
                query = query.Where(produto => produto.Entrada <= dataFinalUtc);
            }

            IQueryable<ProdutoEstoqueModel> queryOrdenada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveis, "etiqueta")
                .ThenBy(produto => produto.Id);

            IQueryable<ProdutoBuscaDto> queryProjetada = queryOrdenada.Select(produto => new ProdutoBuscaDto
            {
                Id = produto.Id,
                Etiqueta = produto.Etiqueta,
                Preco = produto.Preco,
                ProdutoId = produto.ProdutoId,
                Produto = produto.Produto != null ? produto.Produto.Valor : string.Empty,
                MarcaId = produto.MarcaId,
                Marca = produto.Marca != null ? produto.Marca.Valor : string.Empty,
                TamanhoId = produto.TamanhoId,
                Tamanho = produto.Tamanho != null ? produto.Tamanho.Valor : string.Empty,
                CorId = produto.CorId,
                Cor = produto.Cor != null ? produto.Cor.Valor : string.Empty,
                FornecedorId = produto.FornecedorId,
                Fornecedor = produto.Fornecedor != null ? produto.Fornecedor.Nome : string.Empty,
                Descricao = produto.Descricao,
                Entrada = produto.Entrada,
                LojaId = produto.LojaId,
                Situacao = produto.Situacao,
                Consignado = produto.Consignado,
                TipoSugerido = produto.Fornecedor != null && produto.Fornecedor.Doacao
                    ? TipoMovimentacao.Doacao
                    : TipoMovimentacao.DevolucaoDono
            });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        public async Task<ProdutoBuscaDto> GetByIdAsync(ObterProdutoParametros parametros, CancellationToken cancellationToken = default)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            ProdutoEstoqueModel produto = await _context.ProdutosEstoque
                .Include(item => item.Produto)
                .Include(item => item.Marca)
                .Include(item => item.Tamanho)
                .Include(item => item.Cor)
                .Include(item => item.Fornecedor)
                .SingleOrDefaultAsync(item => item.Id == parametros.ProdutoId, cancellationToken)
                ?? throw new KeyNotFoundException("Produto informado nao foi encontrado.");

            await _authorizationService.EnsurePermissionAsync(produto.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.ProdutosVisualizarItem, cancellationToken);

            return new ProdutoBuscaDto
            {
                Id = produto.Id,
                Etiqueta = produto.Etiqueta,
                Preco = produto.Preco,
                ProdutoId = produto.ProdutoId,
                Produto = produto.Produto != null ? produto.Produto.Valor : string.Empty,
                MarcaId = produto.MarcaId,
                Marca = produto.Marca != null ? produto.Marca.Valor : string.Empty,
                TamanhoId = produto.TamanhoId,
                Tamanho = produto.Tamanho != null ? produto.Tamanho.Valor : string.Empty,
                CorId = produto.CorId,
                Cor = produto.Cor != null ? produto.Cor.Valor : string.Empty,
                FornecedorId = produto.FornecedorId,
                Fornecedor = produto.Fornecedor != null ? produto.Fornecedor.Nome : string.Empty,
                Descricao = produto.Descricao,
                Entrada = produto.Entrada,
                LojaId = produto.LojaId,
                Situacao = produto.Situacao,
                Consignado = produto.Consignado,
                TipoSugerido = produto.Fornecedor != null && produto.Fornecedor.Doacao
                    ? TipoMovimentacao.Doacao
                    : TipoMovimentacao.DevolucaoDono
            };
        }

        public async Task<IReadOnlyList<ProdutoBuscaDto>> GetEmprestadosDoClienteAsync(
            ObterProdutosEmprestadosClienteParametros parametros,
            CancellationToken cancellationToken = default)
        {
            await _authorizationService.EnsurePermissionAsync(parametros.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.ProdutosEmprestadosVisualizar, cancellationToken);
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(parametros.LojaId, parametros.UsuarioId, cancellationToken);

            ClienteModel cliente = await _context.Clientes
                .SingleOrDefaultAsync(item => item.Id == parametros.ClienteId, cancellationToken)
                ?? throw new ArgumentException("Cliente informado nao foi encontrado.", nameof(parametros));

            if (cliente.LojaId != loja.Id)
            {
                throw new ArgumentException("Cliente informado nao pertence a loja selecionada.", nameof(parametros));
            }

            return await _context.ProdutosEstoque
                .Where(produto => produto.LojaId == parametros.LojaId && produto.Situacao == SituacaoProduto.Emprestado)
                .Where(produto => produto.Movimentacoes
                    .Where(movimentacaoProduto => movimentacaoProduto.Movimentacao != null)
                    .OrderByDescending(movimentacaoProduto => movimentacaoProduto.Movimentacao!.Data)
                    .ThenByDescending(movimentacaoProduto => movimentacaoProduto.MovimentacaoId)
                    .Take(1)
                    .Any(movimentacaoProduto =>
                        movimentacaoProduto.Movimentacao!.Tipo == TipoMovimentacao.Emprestimo
                        && movimentacaoProduto.Movimentacao.ClienteId == parametros.ClienteId))
                .OrderBy(produto => produto.Id)
                .Select(produto => new ProdutoBuscaDto
                {
                    Id = produto.Id,
                    Etiqueta = produto.Etiqueta,
                    Preco = produto.Preco,
                    ProdutoId = produto.ProdutoId,
                    Produto = produto.Produto != null ? produto.Produto.Valor : string.Empty,
                    MarcaId = produto.MarcaId,
                    Marca = produto.Marca != null ? produto.Marca.Valor : string.Empty,
                    TamanhoId = produto.TamanhoId,
                    Tamanho = produto.Tamanho != null ? produto.Tamanho.Valor : string.Empty,
                    CorId = produto.CorId,
                    Cor = produto.Cor != null ? produto.Cor.Valor : string.Empty,
                    FornecedorId = produto.FornecedorId,
                    Fornecedor = produto.Fornecedor != null ? produto.Fornecedor.Nome : string.Empty,
                    Descricao = produto.Descricao,
                    Entrada = produto.Entrada,
                    LojaId = produto.LojaId,
                    Situacao = produto.Situacao,
                    Consignado = produto.Consignado
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SolicitacaoCompativelDto>> GetSolicitacoesCompativeisAsync(int produtoId, CancellationToken cancellationToken = default)
        {
            ProdutoEstoqueModel produto = await _context.ProdutosEstoque
                .SingleOrDefaultAsync(item => item.Id == produtoId, cancellationToken)
                ?? throw new KeyNotFoundException("Produto informado nao foi encontrado.");

            return await ObterSolicitacoesCompativeisCoreAsync(produto, cancellationToken);
        }

        private async Task<ProdutoAuxiliarDto> CriarAuxiliarAsync<TModel>(
            CriarProdutoAuxiliarCommand request,
            CriarProdutoAuxiliarParametros parametros,
            DbSet<TModel> dbSet,
            Func<string, TModel> factory,
            string nomeEntidade,
            CancellationToken cancellationToken)
            where TModel : class
        {
            string valorNormalizado = request.Valor.Trim();

            await _authorizationService.EnsurePermissionAsync(request.LojaId, parametros.UsuarioId, ResolverPermissaoAuxiliarCriacao(nomeEntidade), cancellationToken);

            bool valorJaExiste = await dbSet.AnyAsync(
                entity => EF.Property<int>(entity, "LojaId") == request.LojaId
                    && EF.Property<string>(entity, "Valor") == valorNormalizado,
                cancellationToken);

            if (valorJaExiste)
            {
                throw new InvalidOperationException($"{nomeEntidade} ja possui este valor cadastrado para a loja informada.");
            }

            TModel entity = factory(valorNormalizado);

            _ = await dbSet.AddAsync(entity, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new ProdutoAuxiliarDto
            {
                Id = ObterId(entity),
                Valor = ObterValor(entity),
                LojaId = ObterLojaId(entity)
            };
        }

        private async Task<PaginacaoDto<ProdutoAuxiliarDto>> ObterAuxiliaresAsync<TModel>(
            ObterProdutoAuxiliarQuery request,
            ObterProdutoAuxiliarParametros parametros,
            DbSet<TModel> dbSet,
            CancellationToken cancellationToken)
            where TModel : class
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            await _authorizationService.EnsurePermissionAsync(request.LojaId.Value, parametros.UsuarioId, FuncionalidadeCatalogo.ProdutosAuxiliaresVisualizar, cancellationToken);

            IQueryable<TModel> query = dbSet.Where(entity => EF.Property<int>(entity, "LojaId") == request.LojaId.Value);

            if (!string.IsNullOrWhiteSpace(request.Valor))
            {
                string valorFiltro = request.Valor.Trim().ToLowerInvariant();
                query = query.Where(entity => EF.Property<string>(entity, "Valor").ToLower().Contains(valorFiltro));
            }

            IQueryable<TModel> queryOrdenada = AplicarOrdenacaoAuxiliar(query, request.OrdenarPor, request.Direcao);

            IQueryable<ProdutoAuxiliarDto> queryProjetada = queryOrdenada.Select(entity => new ProdutoAuxiliarDto
            {
                Id = EF.Property<int>(entity, "Id"),
                Valor = EF.Property<string>(entity, "Valor"),
                LojaId = EF.Property<int>(entity, "LojaId")
            });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        private async Task ExcluirAuxiliarAsync<TModel>(
            ExcluirProdutoAuxiliarParametros parametros,
            DbSet<TModel> dbSet,
            Func<int, Task<bool>> possuiProdutosVinculados,
            string nomeEntidade,
            CancellationToken cancellationToken)
            where TModel : class
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            TModel entity = await dbSet
                .SingleOrDefaultAsync(item => EF.Property<int>(item, "Id") == parametros.ProdutoAuxiliarId, cancellationToken)
                ?? throw new KeyNotFoundException($"{nomeEntidade} informada nao foi encontrada.");

            int lojaId = ObterLojaId(entity);

            await _authorizationService.EnsurePermissionAsync(lojaId, parametros.UsuarioId, ResolverPermissaoAuxiliarExclusao(nomeEntidade), cancellationToken);

            if (await possuiProdutosVinculados(parametros.ProdutoAuxiliarId))
            {
                throw new InvalidOperationException($"{nomeEntidade} possui produtos vinculados e nao pode ser excluido.");
            }

            _ = dbSet.Remove(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        private static IOrderedQueryable<TModel> AplicarOrdenacaoAuxiliar<TModel>(
            IQueryable<TModel> source,
            string? ordenarPor,
            string? direcao)
            where TModel : class
        {
            string campoNormalizado = string.IsNullOrWhiteSpace(ordenarPor)
                ? "valor"
                : ordenarPor.Trim().ToLowerInvariant();

            if (campoNormalizado is not ("id" or "valor"))
            {
                throw new ArgumentException("Campo de ordenacao invalido.", nameof(ordenarPor));
            }

            string direcaoNormalizada = string.IsNullOrWhiteSpace(direcao)
                ? "asc"
                : direcao.Trim().ToLowerInvariant();

            if (direcaoNormalizada is not ("asc" or "desc"))
            {
                throw new ArgumentException("Direcao de ordenacao invalida.", nameof(direcao));
            }

            if (campoNormalizado == "id")
            {
                IOrderedQueryable<TModel> queryOrdenadaPorId = direcaoNormalizada == "desc"
                    ? source.OrderByDescending(entity => EF.Property<int>(entity, "Id"))
                    : source.OrderBy(entity => EF.Property<int>(entity, "Id"));

                return direcaoNormalizada == "desc"
                    ? queryOrdenadaPorId.ThenByDescending(entity => EF.Property<string>(entity, "Valor"))
                    : queryOrdenadaPorId.ThenBy(entity => EF.Property<string>(entity, "Valor"));
            }

            IOrderedQueryable<TModel> queryOrdenadaPorValor = direcaoNormalizada == "desc"
                ? source.OrderByDescending(entity => EF.Property<string>(entity, "Valor"))
                : source.OrderBy(entity => EF.Property<string>(entity, "Valor"));

            return queryOrdenadaPorValor.ThenBy(entity => EF.Property<int>(entity, "Id"));
        }

        private Task<bool> ProdutoPossuiRelacionamentosAtivosAsync(int produtoId, CancellationToken cancellationToken)
        {
            return _context.MovimentacoesProdutos
                .AnyAsync(movimentacaoProduto => movimentacaoProduto.ProdutoId == produtoId, cancellationToken);
        }

        private static string ResolverPermissaoAuxiliarCriacao(string nomeEntidade)
        {
            return nomeEntidade switch
            {
                "Produto" => FuncionalidadeCatalogo.ProdutosAuxiliaresAdicionarReferencia,
                "Marca" => FuncionalidadeCatalogo.ProdutosAuxiliaresAdicionarMarca,
                "Tamanho" => FuncionalidadeCatalogo.ProdutosAuxiliaresAdicionarTamanho,
                "Cor" => FuncionalidadeCatalogo.ProdutosAuxiliaresAdicionarCor,
                _ => throw new ArgumentOutOfRangeException(nameof(nomeEntidade), "Tipo auxiliar nao suportado.")
            };
        }

        private static string ResolverPermissaoAuxiliarExclusao(string nomeEntidade)
        {
            return nomeEntidade switch
            {
                "Produto" => FuncionalidadeCatalogo.ProdutosAuxiliaresAdicionarReferencia,
                "Marca" => FuncionalidadeCatalogo.ProdutosAuxiliaresAdicionarMarca,
                "Tamanho" => FuncionalidadeCatalogo.ProdutosAuxiliaresAdicionarTamanho,
                "Cor" => FuncionalidadeCatalogo.ProdutosAuxiliaresAdicionarCor,
                _ => throw new ArgumentOutOfRangeException(nameof(nomeEntidade), "Tipo auxiliar nao suportado.")
            };
        }

        private static ProdutoDto MapearProdutoDto(ProdutoEstoqueModel produto)
        {
            return new ProdutoDto
            {
                Id = produto.Id,
                Etiqueta = produto.Etiqueta,
                Preco = produto.Preco,
                ProdutoId = produto.ProdutoId,
                MarcaId = produto.MarcaId,
                TamanhoId = produto.TamanhoId,
                CorId = produto.CorId,
                FornecedorId = produto.FornecedorId,
                Descricao = produto.Descricao,
                Entrada = produto.Entrada,
                LojaId = produto.LojaId,
                Situacao = produto.Situacao,
                Consignado = produto.Consignado
            };
        }

        private async Task<int> ObterEtiquetaInicialAsync(
            int lojaId,
            string? etiquetaInformada,
            int quantidade,
            int? produtoIdIgnorado,
            CancellationToken cancellationToken)
        {
            int etiquetaInicial;

            if (string.IsNullOrWhiteSpace(etiquetaInformada))
            {
                int? maiorEtiqueta = await _context.ProdutosEstoque
                    .Where(produto => produto.LojaId == lojaId)
                    .MaxAsync(produto => (int?)produto.Etiqueta, cancellationToken);

                etiquetaInicial = maiorEtiqueta.HasValue ? maiorEtiqueta.Value + 1 : 0;
            }
            else if (!int.TryParse(etiquetaInformada.Trim(), out etiquetaInicial))
            {
                throw new ArgumentException("Etiqueta deve conter apenas numeros.", nameof(etiquetaInformada));
            }

            int etiquetaFinal = checked(etiquetaInicial + quantidade - 1);

            bool etiquetaJaExiste = await _context.ProdutosEstoque
                .AnyAsync(produto =>
                    produto.LojaId == lojaId
                    && (!produtoIdIgnorado.HasValue || produto.Id != produtoIdIgnorado.Value)
                    && produto.Etiqueta >= etiquetaInicial
                    && produto.Etiqueta <= etiquetaFinal,
                    cancellationToken);

            if (etiquetaJaExiste)
            {
                throw new InvalidOperationException("Etiqueta ja cadastrada para a loja informada.");
            }

            return etiquetaInicial;
        }

        private Task<List<SolicitacaoCompativelDto>> ObterSolicitacoesCompativeisCoreAsync(ProdutoEstoqueModel produto, CancellationToken cancellationToken)
        {
            return _context.Solicitacoes
                .Where(solicitacao =>
                    solicitacao.LojaId == produto.LojaId
                    && (!solicitacao.ProdutoId.HasValue || solicitacao.ProdutoId == produto.ProdutoId)
                    && (!solicitacao.MarcaId.HasValue || solicitacao.MarcaId == produto.MarcaId)
                    && (!solicitacao.TamanhoId.HasValue || solicitacao.TamanhoId == produto.TamanhoId)
                    && (!solicitacao.CorId.HasValue || solicitacao.CorId == produto.CorId)
                    && (!solicitacao.PrecoMinimo.HasValue || produto.Preco >= solicitacao.PrecoMinimo.Value)
                    && (!solicitacao.PrecoMaximo.HasValue || produto.Preco <= solicitacao.PrecoMaximo.Value))
                .OrderBy(solicitacao => solicitacao.Descricao)
                .ThenBy(solicitacao => solicitacao.Id)
                .Select(solicitacao => new SolicitacaoCompativelDto
                {
                    Id = solicitacao.Id,
                    Cliente = solicitacao.Cliente != null ? solicitacao.Cliente.Nome : string.Empty,
                    Produto = solicitacao.Produto != null ? solicitacao.Produto.Valor : string.Empty,
                    Marca = solicitacao.Marca != null ? solicitacao.Marca.Valor : string.Empty,
                    Tamanho = solicitacao.Tamanho != null ? solicitacao.Tamanho.Valor : string.Empty,
                    Cor = solicitacao.Cor != null ? solicitacao.Cor.Valor : string.Empty,
                    Descricao = solicitacao.Descricao ?? string.Empty,
                    PrecoMinimo = solicitacao.PrecoMinimo,
                    PrecoMaximo = solicitacao.PrecoMaximo
                })
                .ToListAsync(cancellationToken);
        }

        private static DateTime NormalizarDateTimeParaUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => value
            };
        }

        private static int ObterId<TModel>(TModel entity)
            where TModel : class
        {
            return entity switch
            {
                ProdutoReferenciaModel item => item.Id,
                MarcaModel item => item.Id,
                TamanhoModel item => item.Id,
                CorModel item => item.Id,
                _ => throw new InvalidOperationException("Tipo auxiliar nao suportado.")
            };
        }

        private static string ObterValor<TModel>(TModel entity)
            where TModel : class
        {
            return entity switch
            {
                ProdutoReferenciaModel item => item.Valor,
                MarcaModel item => item.Valor,
                TamanhoModel item => item.Valor,
                CorModel item => item.Valor,
                _ => throw new InvalidOperationException("Tipo auxiliar nao suportado.")
            };
        }

        private static int ObterLojaId<TModel>(TModel entity)
            where TModel : class
        {
            return entity switch
            {
                ProdutoReferenciaModel item => item.LojaId,
                MarcaModel item => item.LojaId,
                TamanhoModel item => item.LojaId,
                CorModel item => item.LojaId,
                _ => throw new InvalidOperationException("Tipo auxiliar nao suportado.")
            };
        }
    }
}
