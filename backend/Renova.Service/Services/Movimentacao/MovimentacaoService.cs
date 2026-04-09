using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Movimentacao;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Movimentacao;
using Renova.Service.Queries.Movimentacao;
using Renova.Service.Services.Pagamento;
using System.Linq.Expressions;

namespace Renova.Service.Services.Movimentacao
{
    public class MovimentacaoService(RenovaDbContext context, IPagamentoService? pagamentoService = null) : IMovimentacaoService
    {
        private readonly RenovaDbContext _context = context;
        private readonly IPagamentoService _pagamentoService = pagamentoService ?? NoOpPagamentoService.Instance;
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
                DateTime dataInicialUtc = NormalizarDateTimeParaUtc(request.DataInicial.Value);
                query = query.Where(movimentacao => movimentacao.Data >= dataInicialUtc);
            }

            if (request.DataFinal.HasValue)
            {
                DateTime dataFinalUtc = NormalizarDateTimeParaUtc(request.DataFinal.Value);
                query = query.Where(movimentacao => movimentacao.Data <= dataFinalUtc);
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

            try
            {
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

                List<ProdutoEstoqueModel> produtosComSituacaoInvalida = await ObterProdutosComSituacaoInvalidaAsync(
                    request,
                    produtos,
                    cancellationToken);

                if (produtosComSituacaoInvalida.Count > 0)
                {
                    string produtosInvalidos = string.Join(", ", produtosComSituacaoInvalida.Select(item => item.Id));
                    string situacaoEsperada = ObterDescricaoSituacaoEsperada(request.Tipo, request.ClienteId);
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

                if (request.Tipo is TipoMovimentacao.Venda or TipoMovimentacao.DevolucaoVenda)
                {
                    _ = await _pagamentoService.CreateAsync(new CriarPagamentoCommand
                    {
                        MovimentacaoId = entity.Id,
                        TipoMovimentacao = request.Tipo,
                        LojaId = request.LojaId,
                        ClienteId = request.ClienteId,
                        ProdutoIds = produtoIds,
                        Data = request.Data
                    }, cancellationToken);
                }

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
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
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                throw;
            }
            finally
            {
                if (transaction is not null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        public async Task<MovimentacaoDestinacaoSugestaoDto> GetDestinacaoAsync(int lojaId, ObterMovimentacoesParametros parametros, CancellationToken cancellationToken = default)
        {
            _ = await ObterLojaDoUsuarioAsync(lojaId, parametros.UsuarioId, cancellationToken);

            ConfigLojaModel config = await _context.ConfiguracoesLoja
                .SingleOrDefaultAsync(item => item.LojaId == lojaId, cancellationToken)
                ?? throw new InvalidOperationException("Configuracao da loja nao encontrada.");

            DateTime dataLimitePermanencia = DateTime.UtcNow.AddMonths(-config.TempoPermanenciaProdutoMeses);

            List<MovimentacaoDestinacaoProdutoDto> produtos = await _context.ProdutosEstoque
                .Where(produto =>
                    produto.LojaId == lojaId
                    && produto.Situacao == SituacaoProduto.Estoque
                    && produto.Entrada <= dataLimitePermanencia)
                .OrderBy(produto => produto.Fornecedor != null ? produto.Fornecedor.Nome : string.Empty)
                .ThenBy(produto => produto.Id)
                .Select(produto => new MovimentacaoDestinacaoProdutoDto
                {
                    Id = produto.Id,
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
                })
                .ToListAsync(cancellationToken);

            return new MovimentacaoDestinacaoSugestaoDto
            {
                LojaId = lojaId,
                TempoPermanenciaProdutoMeses = config.TempoPermanenciaProdutoMeses,
                DataLimitePermanencia = dataLimitePermanencia,
                Produtos = produtos
            };
        }

        public async Task<IReadOnlyList<MovimentacaoDto>> CreateDestinacaoAsync(CriarMovimentacaoDestinacaoCommand request, CriarMovimentacaoParametros parametros, CancellationToken cancellationToken = default)
        {
            if (request.Data == default)
            {
                throw new ArgumentException("Data da movimentacao e obrigatoria.", nameof(request));
            }

            if (request.Itens.Count == 0)
            {
                throw new ArgumentException("Ao menos um produto deve ser informado.", nameof(request));
            }

            _ = await ObterLojaDoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            List<int> produtoIdsDuplicados = request.Itens
                .GroupBy(item => item.ProdutoId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(id => id)
                .ToList();

            if (produtoIdsDuplicados.Count > 0)
            {
                throw new ArgumentException($"Os produtos [{string.Join(", ", produtoIdsDuplicados)}] foram informados mais de uma vez.", nameof(request));
            }

            if (request.Itens.Any(item => item.Tipo is not TipoMovimentacao.Doacao and not TipoMovimentacao.DevolucaoDono))
            {
                throw new ArgumentException("Os itens devem ser marcados apenas como doacao ou devolucao ao dono.", nameof(request));
            }

            List<int> produtoIds = request.Itens
                .Select(item => item.ProdutoId)
                .OrderBy(id => id)
                .ToList();

            Dictionary<int, TipoMovimentacao> tipoPorProdutoId = request.Itens
                .ToDictionary(item => item.ProdutoId, item => item.Tipo);

            IDbContextTransaction? transaction = null;

            try
            {
                if (_context.Database.IsNpgsql())
                {
                    transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
                }

                List<ProdutoEstoqueModel> produtos = await ObterProdutosParaMovimentacaoAsync(produtoIds, cancellationToken);

                if (produtos.Count != produtoIds.Count)
                {
                    throw new ArgumentException("Um ou mais produtos informados nao foram encontrados.", nameof(request));
                }

                if (produtos.Any(item => item.LojaId != request.LojaId))
                {
                    throw new ArgumentException("Os produtos informados devem pertencer a loja selecionada.", nameof(request));
                }

                List<ProdutoEstoqueModel> produtosComSituacaoInvalida = produtos
                    .Where(item => item.Situacao != SituacaoProduto.Estoque)
                    .OrderBy(item => item.Id)
                    .ToList();

                if (produtosComSituacaoInvalida.Count > 0)
                {
                    throw new ArgumentException(
                        $"Os produtos [{string.Join(", ", produtosComSituacaoInvalida.Select(item => item.Id))}] nao estao com a situacao esperada Estoque.",
                        nameof(request));
                }

                List<MovimentacaoModel> movimentacoes = [];

                foreach (IGrouping<(int FornecedorId, TipoMovimentacao Tipo), ProdutoEstoqueModel> grupo in produtos
                    .GroupBy(produto => (FornecedorId: produto.FornecedorId, Tipo: tipoPorProdutoId[produto.Id]))
                    .OrderBy(group => group.Key.FornecedorId)
                    .ThenBy(group => group.Key.Tipo))
                {
                    MovimentacaoModel movimentacao = new()
                    {
                        Tipo = grupo.Key.Tipo,
                        Data = request.Data,
                        ClienteId = grupo.Key.FornecedorId,
                        LojaId = request.LojaId,
                        Produtos = grupo
                            .OrderBy(produto => produto.Id)
                            .Select(produto => new MovimentacaoProdutoModel
                            {
                                ProdutoId = produto.Id
                            })
                            .ToList()
                    };

                    foreach (ProdutoEstoqueModel produto in grupo)
                    {
                        produto.Situacao = SituacoesFinaisPorTipo[grupo.Key.Tipo];
                    }

                    movimentacoes.Add(movimentacao);
                }

                await _context.Movimentacoes.AddRangeAsync(movimentacoes, cancellationToken);
                _ = await _context.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return movimentacoes
                    .OrderBy(movimentacao => movimentacao.ClienteId)
                    .ThenBy(movimentacao => movimentacao.Tipo)
                    .Select(movimentacao => new MovimentacaoDto
                    {
                        Id = movimentacao.Id,
                        Tipo = movimentacao.Tipo,
                        Data = movimentacao.Data,
                        ClienteId = movimentacao.ClienteId,
                        LojaId = movimentacao.LojaId,
                        ProdutoIds = movimentacao.Produtos
                            .Select(item => item.ProdutoId)
                            .OrderBy(id => id)
                            .ToList()
                    })
                    .ToList();
            }
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                throw;
            }
            finally
            {
                if (transaction is not null)
                {
                    await transaction.DisposeAsync();
                }
            }
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

        private async Task<List<ProdutoEstoqueModel>> ObterProdutosComSituacaoInvalidaAsync(
            CriarMovimentacaoCommand request,
            List<ProdutoEstoqueModel> produtos,
            CancellationToken cancellationToken)
        {
            SituacaoProduto situacaoEsperada = SituacoesEsperadasPorTipo[request.Tipo];
            List<ProdutoEstoqueModel> produtosComSituacaoInvalidaPorSituacao = produtos
                .Where(item => item.Situacao != situacaoEsperada)
                .ToList();

            if (request.Tipo is not TipoMovimentacao.Venda
                and not TipoMovimentacao.DevolucaoVenda
                and not TipoMovimentacao.DevolucaoEmprestimo)
            {
                return produtosComSituacaoInvalidaPorSituacao
                    .OrderBy(item => item.Id)
                    .ToList();
            }

            List<ProdutoEstoqueModel> produtosQueExigemValidacaoPorUltimaMovimentacao = produtos
                .Where(item => request.Tipo switch
                {
                    TipoMovimentacao.Venda => item.Situacao == SituacaoProduto.Emprestado,
                    TipoMovimentacao.DevolucaoVenda => item.Situacao == SituacaoProduto.Vendido,
                    TipoMovimentacao.DevolucaoEmprestimo => item.Situacao == SituacaoProduto.Emprestado,
                    _ => false
                })
                .ToList();

            if (produtosQueExigemValidacaoPorUltimaMovimentacao.Count == 0)
            {
                return produtosComSituacaoInvalidaPorSituacao
                    .OrderBy(item => item.Id)
                    .ToList();
            }

            Dictionary<int, UltimaMovimentacaoProdutoInfo> ultimasMovimentacoesPorProduto = await ObterUltimaMovimentacaoPorProdutoAsync(
                produtosQueExigemValidacaoPorUltimaMovimentacao.Select(item => item.Id).ToList(),
                cancellationToken);

            HashSet<int> produtosComValidacaoEspecial = produtosQueExigemValidacaoPorUltimaMovimentacao
                .Select(item => item.Id)
                .ToHashSet();

            HashSet<int> produtosInvalidosPorSituacao = produtosComSituacaoInvalidaPorSituacao
                .Where(item => !produtosComValidacaoEspecial.Contains(item.Id))
                .Select(item => item.Id)
                .ToHashSet();

            HashSet<int> produtosInvalidosPorUltimaMovimentacao = produtosQueExigemValidacaoPorUltimaMovimentacao
                .Where(item =>
                    !ultimasMovimentacoesPorProduto.TryGetValue(item.Id, out UltimaMovimentacaoProdutoInfo? ultimaMovimentacao)
                    || !UltimaMovimentacaoEhCompativel(request.Tipo, request.ClienteId, ultimaMovimentacao))
                .Select(item => item.Id)
                .ToHashSet();

            return produtos
                .Where(item => produtosInvalidosPorSituacao.Contains(item.Id)
                    || produtosInvalidosPorUltimaMovimentacao.Contains(item.Id))
                .OrderBy(item => item.Id)
                .ToList();
        }

        private async Task<Dictionary<int, UltimaMovimentacaoProdutoInfo>> ObterUltimaMovimentacaoPorProdutoAsync(
            List<int> produtoIds,
            CancellationToken cancellationToken)
        {
            return await _context.MovimentacoesProdutos
                .Where(item => produtoIds.Contains(item.ProdutoId) && item.Movimentacao != null)
                .Select(item => new
                {
                    item.ProdutoId,
                    MovimentacaoId = item.MovimentacaoId,
                    item.Movimentacao!.ClienteId,
                    item.Movimentacao.Tipo,
                    item.Movimentacao.Data
                })
                .GroupBy(item => item.ProdutoId)
                .Select(group => group
                    .OrderByDescending(item => item.Data)
                    .ThenByDescending(item => item.MovimentacaoId)
                    .Select(item => new UltimaMovimentacaoProdutoInfo(
                        group.Key,
                        item.Tipo,
                        item.ClienteId))
                    .First())
                .ToDictionaryAsync(item => item.ProdutoId, item => item, cancellationToken);
        }

        private static bool UltimaMovimentacaoEhCompativel(
            TipoMovimentacao tipoSolicitado,
            int clienteId,
            UltimaMovimentacaoProdutoInfo ultimaMovimentacao)
        {
            return tipoSolicitado switch
            {
                TipoMovimentacao.Venda =>
                    ultimaMovimentacao.Tipo == TipoMovimentacao.Emprestimo
                    && ultimaMovimentacao.ClienteId == clienteId,
                TipoMovimentacao.DevolucaoVenda =>
                    ultimaMovimentacao.Tipo == TipoMovimentacao.Venda
                    && ultimaMovimentacao.ClienteId == clienteId,
                TipoMovimentacao.DevolucaoEmprestimo =>
                    ultimaMovimentacao.Tipo == TipoMovimentacao.Emprestimo
                    && ultimaMovimentacao.ClienteId == clienteId,
                _ => false
            };
        }

        private static string ObterDescricaoSituacaoEsperada(TipoMovimentacao tipo, int clienteId)
        {
            return tipo switch
            {
                TipoMovimentacao.Venda =>
                    "disponiveis para venda deste cliente",
                TipoMovimentacao.DevolucaoVenda =>
                    "da ultima venda deste cliente",
                TipoMovimentacao.DevolucaoEmprestimo =>
                    "do ultimo emprestimo deste cliente",
                _ => SituacoesEsperadasPorTipo[tipo].ToString()
            };
        }

        private sealed record UltimaMovimentacaoProdutoInfo(
            int ProdutoId,
            TipoMovimentacao Tipo,
            int ClienteId);

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

        private sealed class NoOpPagamentoService : IPagamentoService
        {
            public static readonly NoOpPagamentoService Instance = new();

            public Task<IReadOnlyList<PagamentoDto>> CreateAsync(CriarPagamentoCommand request, CancellationToken cancellationToken = default)
            {
                _ = request;
                _ = cancellationToken;
                return Task.FromResult<IReadOnlyList<PagamentoDto>>([]);
            }
        }
    }
}
