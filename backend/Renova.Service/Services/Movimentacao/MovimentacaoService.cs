using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Movimentacao;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Movimentacao;
using Renova.Service.Queries.Movimentacao;
using System.Linq.Expressions;

namespace Renova.Service.Services.Movimentacao
{
    public class MovimentacaoService(RenovaDbContext context) : IMovimentacaoService
    {
        private readonly RenovaDbContext _context = context;
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<MovimentacaoModel, int>>)(movimentacao => movimentacao.Id),
            ["data"] = (Expression<Func<MovimentacaoModel, DateTime>>)(movimentacao => movimentacao.Data),
            ["cliente"] = (Expression<Func<MovimentacaoModel, string>>)(movimentacao => movimentacao.Cliente != null ? movimentacao.Cliente.Nome : string.Empty),
            ["tipo"] = (Expression<Func<MovimentacaoModel, TipoMovimentacao>>)(movimentacao => movimentacao.Tipo)
        };
        private static readonly IReadOnlyDictionary<TipoMovimentacao, SituacaoProduto> SituacoesEsperadasPorTipo = new Dictionary<TipoMovimentacao, SituacaoProduto>
        {
            [TipoMovimentacao.Venda] = SituacaoProduto.Estoque,
            [TipoMovimentacao.Emprestimo] = SituacaoProduto.Estoque,
            [TipoMovimentacao.Doacao] = SituacaoProduto.Estoque,
            [TipoMovimentacao.DevolucaoDono] = SituacaoProduto.Estoque,
            [TipoMovimentacao.DevolucaoVenda] = SituacaoProduto.Vendido,
            [TipoMovimentacao.DevolucaoEmprestimo] = SituacaoProduto.Emprestado
        };
        private static readonly IReadOnlyDictionary<TipoMovimentacao, SituacaoProduto> SituacoesFinaisPorTipo = new Dictionary<TipoMovimentacao, SituacaoProduto>
        {
            [TipoMovimentacao.Venda] = SituacaoProduto.Vendido,
            [TipoMovimentacao.Emprestimo] = SituacaoProduto.Emprestado,
            [TipoMovimentacao.Doacao] = SituacaoProduto.Doado,
            [TipoMovimentacao.DevolucaoDono] = SituacaoProduto.Devolvido,
            [TipoMovimentacao.DevolucaoVenda] = SituacaoProduto.Estoque,
            [TipoMovimentacao.DevolucaoEmprestimo] = SituacaoProduto.Estoque
        };

        public async Task<PaginacaoDto<MovimentacaoBuscaDto>> GetAllAsync(ObterMovimentacoesQuery request, ObterMovimentacoesParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            _ = await ObterLojaDoUsuarioAsync(request.LojaId.Value, parametros.UsuarioId, cancellationToken);

            IQueryable<MovimentacaoModel> query = _context.Movimentacoes
                .Where(movimentacao => movimentacao.LojaId == request.LojaId.Value);

            if (request.DataInicial.HasValue)
            {
                query = query.Where(movimentacao => movimentacao.Data >= request.DataInicial.Value);
            }

            if (request.DataFinal.HasValue)
            {
                query = query.Where(movimentacao => movimentacao.Data <= request.DataFinal.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Cliente))
            {
                string clienteFiltro = request.Cliente.Trim().ToLowerInvariant();
                query = query.Where(movimentacao => movimentacao.Cliente != null && movimentacao.Cliente.Nome.ToLower().Contains(clienteFiltro));
            }

            if (request.Tipo.HasValue)
            {
                if (!Enum.IsDefined(request.Tipo.Value))
                {
                    throw new ArgumentException("Tipo de movimentacao informado e invalido.", nameof(request));
                }

                query = query.Where(movimentacao => movimentacao.Tipo == request.Tipo.Value);
            }

            IQueryable<MovimentacaoModel> queryOrdenada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveis, "data")
                .ThenBy(movimentacao => movimentacao.Id);

            IQueryable<MovimentacaoBuscaDto> queryProjetada = queryOrdenada.Select(movimentacao => new MovimentacaoBuscaDto
            {
                Id = movimentacao.Id,
                Tipo = movimentacao.Tipo,
                Data = movimentacao.Data,
                ClienteId = movimentacao.ClienteId,
                Cliente = movimentacao.Cliente != null ? movimentacao.Cliente.Nome : string.Empty,
                LojaId = movimentacao.LojaId,
                QuantidadeProdutos = movimentacao.Produtos.Count,
                Produtos = movimentacao.Produtos
                    .OrderBy(item => item.ProdutoId)
                    .Select(item => new ProdutoBuscaDto
                    {
                        Id = item.ProdutoId,
                        Preco = item.Produto != null ? item.Produto.Preco : 0,
                        ProdutoId = item.Produto != null ? item.Produto.ProdutoId : 0,
                        Produto = item.Produto != null && item.Produto.Produto != null ? item.Produto.Produto.Valor : string.Empty,
                        MarcaId = item.Produto != null ? item.Produto.MarcaId : 0,
                        Marca = item.Produto != null && item.Produto.Marca != null ? item.Produto.Marca.Valor : string.Empty,
                        TamanhoId = item.Produto != null ? item.Produto.TamanhoId : 0,
                        Tamanho = item.Produto != null && item.Produto.Tamanho != null ? item.Produto.Tamanho.Valor : string.Empty,
                        CorId = item.Produto != null ? item.Produto.CorId : 0,
                        Cor = item.Produto != null && item.Produto.Cor != null ? item.Produto.Cor.Valor : string.Empty,
                        FornecedorId = item.Produto != null ? item.Produto.FornecedorId : 0,
                        Fornecedor = item.Produto != null && item.Produto.Fornecedor != null ? item.Produto.Fornecedor.Nome : string.Empty,
                        Descricao = item.Produto != null ? item.Produto.Descricao : string.Empty,
                        Entrada = item.Produto != null ? item.Produto.Entrada : default,
                        LojaId = item.Produto != null ? item.Produto.LojaId : 0,
                        Situacao = item.Produto != null ? item.Produto.Situacao : default,
                        Consignado = item.Produto != null && item.Produto.Consignado
                    })
                    .ToList()
            });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        public async Task<MovimentacaoDto> CreateAsync(CriarMovimentacaoCommand request, CriarMovimentacaoParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(request.Tipo))
            {
                throw new ArgumentException("Tipo de movimentacao informado e invalido.", nameof(request));
            }

            if (request.Data == default)
            {
                throw new ArgumentException("Data da movimentacao e obrigatoria.", nameof(request));
            }

            if (request.ProdutoIds.Count == 0)
            {
                throw new ArgumentException("Ao menos um produto deve ser informado.", nameof(request));
            }

            LojaModel loja = await ObterLojaDoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            ClienteModel cliente = await _context.Clientes
                .SingleOrDefaultAsync(item => item.Id == request.ClienteId, cancellationToken)
                ?? throw new ArgumentException("Cliente informado nao foi encontrado.", nameof(request));

            if (cliente.LojaId != loja.Id)
            {
                throw new ArgumentException("Cliente informado nao pertence a loja selecionada.", nameof(request));
            }

            List<int> produtoIds = request.ProdutoIds
                .Distinct()
                .ToList();

            IDbContextTransaction? transaction = null;

            if (_context.Database.IsNpgsql())
            {
                transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            }

            List<ProdutoEstoqueModel> produtos = await ObterProdutosParaMovimentacaoAsync(produtoIds, cancellationToken);

            if (produtos.Count != produtoIds.Count)
            {
                throw new ArgumentException("Um ou mais produtos informados nao foram encontrados.", nameof(request));
            }

            if (produtos.Any(item => item.LojaId != loja.Id))
            {
                throw new ArgumentException("Os produtos informados devem pertencer a loja selecionada.", nameof(request));
            }

            SituacaoProduto situacaoEsperada = SituacoesEsperadasPorTipo[request.Tipo];
            List<ProdutoEstoqueModel> produtosComSituacaoInvalida = produtos
                .Where(item => item.Situacao != situacaoEsperada)
                .OrderBy(item => item.Id)
                .ToList();

            if (produtosComSituacaoInvalida.Count > 0)
            {
                string produtosInvalidos = string.Join(", ", produtosComSituacaoInvalida.Select(item => item.Id));
                throw new ArgumentException(
                    $"Os produtos [{produtosInvalidos}] nao estao com a situacao esperada {situacaoEsperada} para a movimentacao {request.Tipo}.",
                    nameof(request));
            }

            SituacaoProduto situacaoFinal = SituacoesFinaisPorTipo[request.Tipo];

            MovimentacaoModel entity = new()
            {
                Tipo = request.Tipo,
                Data = request.Data,
                ClienteId = request.ClienteId,
                LojaId = request.LojaId,
                Produtos = produtoIds
                    .Select(produtoId => new MovimentacaoProdutoModel
                    {
                        ProdutoId = produtoId
                    })
                    .ToList()
            };

            foreach (ProdutoEstoqueModel produto in produtos)
            {
                produto.Situacao = situacaoFinal;
            }

            _ = await _context.Movimentacoes.AddAsync(entity, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                await transaction.DisposeAsync();
            }

            return new MovimentacaoDto
            {
                Id = entity.Id,
                Tipo = entity.Tipo,
                Data = entity.Data,
                ClienteId = entity.ClienteId,
                LojaId = entity.LojaId,
                ProdutoIds = produtoIds
            };
        }

        private async Task<List<ProdutoEstoqueModel>> ObterProdutosParaMovimentacaoAsync(List<int> produtoIds, CancellationToken cancellationToken)
        {
            if (_context.Database.IsNpgsql())
            {
                int[] ids = produtoIds.ToArray();
                return await _context.ProdutosEstoque
                    .FromSqlRaw("""
                        SELECT *
                        FROM "ProdutoEstoque"
                        WHERE "Id" = ANY ({0})
                        FOR UPDATE
                        """, ids)
                    .ToListAsync(cancellationToken);
            }

            return await _context.ProdutosEstoque
                .Where(item => produtoIds.Contains(item.Id))
                .ToListAsync(cancellationToken);
        }

        private async Task<LojaModel> ObterLojaDoUsuarioAsync(int lojaId, int usuarioId, CancellationToken cancellationToken)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == usuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == lojaId, cancellationToken)
                ?? throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");

            return loja.UsuarioId != usuarioId
                ? throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.")
                : loja;
        }
    }
}
