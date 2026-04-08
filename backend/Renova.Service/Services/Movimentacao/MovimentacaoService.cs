using System.Data;

using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Movimentacao;
using Renova.Service.Parameters.Movimentacao;

namespace Renova.Service.Services.Movimentacao
{
    public class MovimentacaoService(RenovaDbContext context) : IMovimentacaoService
    {
        private readonly RenovaDbContext _context = context;
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

            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

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
            await transaction.CommitAsync(cancellationToken);

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
