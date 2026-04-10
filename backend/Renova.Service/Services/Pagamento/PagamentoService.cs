using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Pagamento;
using Renova.Service.Queries.Pagamento;
using System.Linq.Expressions;

namespace Renova.Service.Services.Pagamento
{
    public class PagamentoService(RenovaDbContext context) : IPagamentoService
    {
        private readonly RenovaDbContext _context = context;
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<PagamentoModel, int>>)(pagamento => pagamento.Id),
            ["data"] = (Expression<Func<PagamentoModel, DateTime>>)(pagamento => pagamento.Data),
            ["cliente"] = (Expression<Func<PagamentoModel, string>>)(pagamento => pagamento.Cliente != null ? pagamento.Cliente.Nome : string.Empty),
            ["valor"] = (Expression<Func<PagamentoModel, decimal>>)(pagamento => pagamento.Valor),
            ["natureza"] = (Expression<Func<PagamentoModel, NaturezaPagamento>>)(pagamento => pagamento.Natureza),
            ["status"] = (Expression<Func<PagamentoModel, StatusPagamento>>)(pagamento => pagamento.Status)
        };
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveisCredito = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<PagamentoCreditoModel, int>>)(pagamento => pagamento.Id),
            ["data"] = (Expression<Func<PagamentoCreditoModel, DateTime>>)(pagamento => pagamento.Data),
            ["cliente"] = (Expression<Func<PagamentoCreditoModel, string>>)(pagamento => pagamento.Cliente != null ? pagamento.Cliente.Nome : string.Empty),
            ["tipo"] = (Expression<Func<PagamentoCreditoModel, TipoPagamentoCredito>>)(pagamento => pagamento.Tipo),
            ["valorCredito"] = (Expression<Func<PagamentoCreditoModel, decimal>>)(pagamento => pagamento.ValorCredito),
            ["valorDinheiro"] = (Expression<Func<PagamentoCreditoModel, decimal>>)(pagamento => pagamento.ValorDinheiro)
        };

        public async Task<PaginacaoDto<PagamentoBuscaDto>> GetAllAsync(
            ObterPagamentosQuery request,
            ObterPagamentosParametros parametros,
            CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            _ = await ObterLojaDoUsuarioAsync(request.LojaId.Value, parametros.UsuarioId, cancellationToken);

            IQueryable<PagamentoModel> query = _context.Pagamentos
                .Where(pagamento => pagamento.LojaId == request.LojaId.Value);

            if (request.DataInicial.HasValue)
            {
                DateTime dataInicialUtc = NormalizarDateTimeParaUtc(request.DataInicial.Value);
                query = query.Where(pagamento => pagamento.Data >= dataInicialUtc);
            }

            if (request.DataFinal.HasValue)
            {
                DateTime dataFinalUtc = NormalizarDateTimeParaUtc(request.DataFinal.Value);
                query = query.Where(pagamento => pagamento.Data <= dataFinalUtc);
            }

            if (!string.IsNullOrWhiteSpace(request.Cliente))
            {
                string clienteFiltro = request.Cliente.Trim().ToLowerInvariant();
                query = query.Where(pagamento => pagamento.Cliente != null && pagamento.Cliente.Nome.ToLower().Contains(clienteFiltro));
            }

            if (request.MovimentacaoId.HasValue)
            {
                query = query.Where(pagamento => pagamento.MovimentacaoId == request.MovimentacaoId.Value);
            }

            if (request.Natureza.HasValue)
            {
                if (!Enum.IsDefined(request.Natureza.Value))
                {
                    throw new ArgumentException("Natureza de pagamento informada e invalida.", nameof(request));
                }

                query = query.Where(pagamento => pagamento.Natureza == request.Natureza.Value);
            }

            if (request.Status.HasValue)
            {
                if (!Enum.IsDefined(request.Status.Value))
                {
                    throw new ArgumentException("Status de pagamento informado e invalido.", nameof(request));
                }

                query = query.Where(pagamento => pagamento.Status == request.Status.Value);
            }

            IQueryable<PagamentoModel> queryOrdenada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveis, "data")
                .ThenBy(pagamento => pagamento.Id);

            IQueryable<PagamentoBuscaDto> queryProjetada = queryOrdenada.Select(pagamento => new PagamentoBuscaDto
            {
                Id = pagamento.Id,
                MovimentacaoId = pagamento.MovimentacaoId,
                LojaId = pagamento.LojaId,
                ClienteId = pagamento.ClienteId,
                Cliente = pagamento.Cliente != null ? pagamento.Cliente.Nome : string.Empty,
                Natureza = pagamento.Natureza,
                Status = pagamento.Status,
                Valor = pagamento.Valor,
                Data = pagamento.Data,
                Movimentacao = new MovimentacaoResumoDto
                {
                    Id = pagamento.Movimentacao != null ? pagamento.Movimentacao.Id : pagamento.MovimentacaoId,
                    Tipo = pagamento.Movimentacao != null ? pagamento.Movimentacao.Tipo : default,
                    Data = pagamento.Movimentacao != null ? pagamento.Movimentacao.Data : default,
                    ClienteId = pagamento.Movimentacao != null ? pagamento.Movimentacao.ClienteId : 0,
                    Cliente = pagamento.Movimentacao != null && pagamento.Movimentacao.Cliente != null ? pagamento.Movimentacao.Cliente.Nome : string.Empty,
                    LojaId = pagamento.Movimentacao != null ? pagamento.Movimentacao.LojaId : pagamento.LojaId,
                    QuantidadeProdutos = pagamento.Movimentacao != null ? pagamento.Movimentacao.Produtos.Count : 0,
                    ProdutoIds = pagamento.Movimentacao != null
                        ? pagamento.Movimentacao.Produtos
                            .OrderBy(item => item.ProdutoId)
                            .Select(item => item.ProdutoId)
                            .ToList()
                        : new List<int>()
                }
            });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        public async Task<PaginacaoDto<PagamentoCreditoBuscaDto>> GetCreditosAsync(
            ObterPagamentosCreditoQuery request,
            ObterPagamentosParametros parametros,
            CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            _ = await ObterLojaDoUsuarioAsync(request.LojaId.Value, parametros.UsuarioId, cancellationToken);

            IQueryable<PagamentoCreditoModel> query = _context.PagamentosCredito
                .Where(pagamento => pagamento.LojaId == request.LojaId.Value);

            if (request.DataInicial.HasValue)
            {
                DateTime dataInicialUtc = NormalizarDateTimeParaUtc(request.DataInicial.Value);
                query = query.Where(pagamento => pagamento.Data >= dataInicialUtc);
            }

            if (request.DataFinal.HasValue)
            {
                DateTime dataFinalUtc = NormalizarDateTimeParaUtc(request.DataFinal.Value);
                query = query.Where(pagamento => pagamento.Data <= dataFinalUtc);
            }

            if (!string.IsNullOrWhiteSpace(request.Cliente))
            {
                string clienteFiltro = request.Cliente.Trim().ToLowerInvariant();
                query = query.Where(pagamento => pagamento.Cliente != null && pagamento.Cliente.Nome.ToLower().Contains(clienteFiltro));
            }

            if (request.Tipo.HasValue)
            {
                if (!Enum.IsDefined(request.Tipo.Value))
                {
                    throw new ArgumentException("Tipo de pagamento externo informado e invalido.", nameof(request));
                }

                query = query.Where(pagamento => pagamento.Tipo == request.Tipo.Value);
            }

            IQueryable<PagamentoCreditoModel> queryOrdenada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveisCredito, "data")
                .ThenBy(pagamento => pagamento.Id);

            IQueryable<PagamentoCreditoBuscaDto> queryProjetada = queryOrdenada.Select(pagamento => new PagamentoCreditoBuscaDto
            {
                Id = pagamento.Id,
                LojaId = pagamento.LojaId,
                ClienteId = pagamento.ClienteId,
                Cliente = pagamento.Cliente != null ? pagamento.Cliente.Nome : string.Empty,
                Tipo = pagamento.Tipo,
                ValorCredito = pagamento.ValorCredito,
                ValorDinheiro = pagamento.ValorDinheiro,
                Data = pagamento.Data
            });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        public async Task<IReadOnlyList<ClientePendenciaDto>> GetPendenciasAsync(
            int lojaId,
            int usuarioId,
            CancellationToken cancellationToken = default)
        {
            _ = await ObterLojaDoUsuarioAsync(lojaId, usuarioId, cancellationToken);

            return await _context.Clientes
                .Where(cliente => cliente.LojaId == lojaId
                    && cliente.Credito != null
                    && cliente.Credito.Valor != 0)
                .OrderBy(cliente => cliente.Nome)
                .ThenBy(cliente => cliente.Id)
                .Select(cliente => new ClientePendenciaDto
                {
                    ClienteId = cliente.Id,
                    Nome = cliente.Nome,
                    Contato = cliente.Contato,
                    Credito = cliente.Credito != null ? cliente.Credito.Valor : 0m
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PagamentoDto>> CreateAsync(CriarPagamentoCommand request, CancellationToken cancellationToken = default)
        {
            if (request.Produtos.Count == 0)
            {
                throw new ArgumentException("Ao menos um produto deve ser informado para gerar pagamentos.", nameof(request));
            }

            ConfigLojaModel config = await _context.ConfiguracoesLoja
                .SingleOrDefaultAsync(item => item.LojaId == request.LojaId, cancellationToken)
                ?? throw new InvalidOperationException("Loja nao possui configuracao de repasse ao fornecedor.");

            List<ProdutoEstoqueModel> produtos = await _context.ProdutosEstoque
                .Where(item => request.Produtos.Select(produto => produto.ProdutoId).Contains(item.Id))
                .OrderBy(item => item.Id)
                .ToListAsync(cancellationToken);

            if (produtos.Count != request.Produtos.Select(produto => produto.ProdutoId).Distinct().Count())
            {
                throw new ArgumentException("Um ou mais produtos informados nao foram encontrados.", nameof(request));
            }

            if (produtos.Any(item => item.LojaId != request.LojaId))
            {
                throw new ArgumentException("Os produtos informados devem pertencer a loja selecionada.", nameof(request));
            }

            Dictionary<int, decimal> descontosPorProdutoId = request.Produtos
                .GroupBy(item => item.ProdutoId)
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        if (group.Count() > 1)
                        {
                            throw new ArgumentException($"O produto {group.Key} foi informado mais de uma vez para gerar pagamentos.", nameof(request));
                        }

                        decimal desconto = group.Single().Desconto;
                        if (desconto is < 0 or > 100)
                        {
                            throw new ArgumentException("Desconto por produto deve estar entre 0 e 100.", nameof(request));
                        }

                        return desconto;
                    });

            decimal valorTotal = produtos.Sum(produto => CalcularValorComDesconto(
                produto.Preco,
                descontosPorProdutoId[produto.Id]));

            (NaturezaPagamento naturezaCliente, NaturezaPagamento naturezaFornecedor) = request.TipoMovimentacao switch
            {
                TipoMovimentacao.Venda => (NaturezaPagamento.Receber, NaturezaPagamento.Pagar),
                TipoMovimentacao.DevolucaoVenda => (NaturezaPagamento.Pagar, NaturezaPagamento.Receber),
                _ => throw new ArgumentException("Tipo de movimentacao nao gera pagamentos.", nameof(request))
            };

            PagamentoModel pagamentoCliente = new()
            {
                MovimentacaoId = request.MovimentacaoId,
                LojaId = request.LojaId,
                ClienteId = request.ClienteId,
                Natureza = naturezaCliente,
                Status = StatusPagamento.Pendente,
                Valor = decimal.Round(valorTotal, 2, MidpointRounding.AwayFromZero),
                Data = request.Data
            };

            List<PagamentoModel> pagamentos = [pagamentoCliente];
            pagamentos.AddRange(produtos
                .GroupBy(item => item.FornecedorId)
                .Select(grupo =>
                {
                    decimal valorFornecedor = decimal.Round(
                        grupo.Sum(item => CalcularValorComDesconto(item.Preco, descontosPorProdutoId[item.Id]))
                            * (config.PercentualRepasseVendedorCredito / 100m),
                        2,
                        MidpointRounding.AwayFromZero);

                    return new PagamentoModel
                    {
                        MovimentacaoId = request.MovimentacaoId,
                        LojaId = request.LojaId,
                        ClienteId = grupo.Key,
                        Natureza = naturezaFornecedor,
                        Status = StatusPagamento.Pendente,
                        Valor = valorFornecedor,
                        Data = request.Data
                    };
                }));

            await _context.Pagamentos.AddRangeAsync(pagamentos, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return [.. pagamentos.Select(Mapear)];
        }

        private static decimal CalcularValorComDesconto(decimal preco, decimal desconto)
        {
            return decimal.Round(
                preco * ((100m - desconto) / 100m),
                2,
                MidpointRounding.AwayFromZero);
        }

        public async Task<PagamentoCreditoDto> CreateCreditoAsync(
            CriarPagamentoCreditoCommand request,
            CriarPagamentoCreditoParametros parametros,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(request.Tipo))
            {
                throw new ArgumentException("Tipo de pagamento de credito informado e invalido.", nameof(request));
            }

            if (request.Data == default)
            {
                throw new ArgumentException("Data do pagamento de credito e obrigatoria.", nameof(request));
            }

            if (request.ValorCredito <= 0)
            {
                throw new ArgumentException("Valor de credito deve ser maior que zero.", nameof(request));
            }

            DateTime dataPagamentoUtc = NormalizarDateTimeParaUtc(request.Data);

            LojaModel loja = await ObterLojaDoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            ClienteModel cliente = await _context.Clientes
                .SingleOrDefaultAsync(item => item.Id == request.ClienteId, cancellationToken)
                ?? throw new ArgumentException("Cliente informado nao foi encontrado.", nameof(request));

            if (cliente.LojaId != loja.Id)
            {
                throw new ArgumentException("Cliente informado nao pertence a loja selecionada.", nameof(request));
            }

            decimal valorCredito = decimal.Round(request.ValorCredito, 2, MidpointRounding.AwayFromZero);
            ClienteCreditoModel credito = await ObterOuCriarCreditoAsync(request.LojaId, request.ClienteId, cancellationToken);

            decimal valorDinheiro = request.Tipo switch
            {
                TipoPagamentoCredito.AdicionarCredito => valorCredito,
                TipoPagamentoCredito.ResgatarCredito => await CalcularValorResgateAsync(
                    request.LojaId,
                    valorCredito,
                    credito,
                    cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(request.Tipo), "Tipo de pagamento de credito desconhecido.")
            };

            credito.Valor += request.Tipo switch
            {
                TipoPagamentoCredito.AdicionarCredito => valorCredito,
                TipoPagamentoCredito.ResgatarCredito => -valorCredito,
                _ => 0m
            };

            PagamentoCreditoModel pagamentoCredito = new()
            {
                LojaId = request.LojaId,
                ClienteId = request.ClienteId,
                Tipo = request.Tipo,
                ValorCredito = valorCredito,
                ValorDinheiro = valorDinheiro,
                Data = dataPagamentoUtc
            };

            _ = await _context.PagamentosCredito.AddAsync(pagamentoCredito, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return Mapear(pagamentoCredito);
        }

        public async Task<AtualizarPendenciasDto> UpdatePendenciasAsync(
            AtualizarPendenciasCommand request,
            AtualizarPendenciasParametros parametros,
            CancellationToken cancellationToken = default)
        {
            if (request.Data == default)
            {
                throw new ArgumentException("Data limite para atualizacao das pendencias e obrigatoria.", nameof(request));
            }

            _ = await ObterLojaDoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            DateTime dataLimite = NormalizarDataLimiteParaFimDoDiaUtc(request.Data);
            List<PagamentoModel> pagamentosPendentes = await _context.Pagamentos
                .Include(pagamento => pagamento.Cliente)
                .Where(pagamento =>
                    pagamento.LojaId == request.LojaId
                    && pagamento.Status == StatusPagamento.Pendente
                    && pagamento.Data <= dataLimite)
                .OrderBy(pagamento => pagamento.Data)
                .ThenBy(pagamento => pagamento.Id)
                .ToListAsync(cancellationToken);

            if (pagamentosPendentes.Count == 0)
            {
                return new AtualizarPendenciasDto();
            }

            List<int> clienteIds = pagamentosPendentes
                .Select(pagamento => pagamento.ClienteId)
                .Distinct()
                .ToList();

            Dictionary<int, ClienteCreditoModel> creditosPorCliente = await _context.ClientesCreditos
                .Where(credito => credito.LojaId == request.LojaId && clienteIds.Contains(credito.ClienteId))
                .ToDictionaryAsync(credito => credito.ClienteId, cancellationToken);

            Dictionary<int, AtualizacaoPendenciaClienteDto> atualizacoesPorCliente = [];

            foreach (PagamentoModel pagamento in pagamentosPendentes)
            {
                decimal variacaoCredito = pagamento.Natureza switch
                {
                    NaturezaPagamento.Receber => -pagamento.Valor,
                    NaturezaPagamento.Pagar => pagamento.Valor,
                    _ => 0m
                };

                if (!creditosPorCliente.TryGetValue(pagamento.ClienteId, out ClienteCreditoModel? credito))
                {
                    credito = new ClienteCreditoModel
                    {
                        LojaId = pagamento.LojaId,
                        ClienteId = pagamento.ClienteId,
                        Valor = 0m
                    };

                    creditosPorCliente[pagamento.ClienteId] = credito;
                    _ = await _context.ClientesCreditos.AddAsync(credito, cancellationToken);
                }

                credito.Valor += variacaoCredito;
                pagamento.Status = StatusPagamento.Pago;

                if (!atualizacoesPorCliente.TryGetValue(pagamento.ClienteId, out AtualizacaoPendenciaClienteDto? atualizacao))
                {
                    atualizacao = new AtualizacaoPendenciaClienteDto
                    {
                        ClienteId = pagamento.ClienteId,
                        Nome = pagamento.Cliente?.Nome ?? $"Cliente {pagamento.ClienteId}",
                        QuantidadeOrdensAtualizadas = 0,
                        ValorAtualizado = 0m
                    };

                    atualizacoesPorCliente[pagamento.ClienteId] = atualizacao;
                }

                atualizacao.QuantidadeOrdensAtualizadas++;
                atualizacao.ValorAtualizado += variacaoCredito;
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            List<AtualizacaoPendenciaClienteDto> clientesAtualizados = atualizacoesPorCliente.Values
                .OrderBy(item => item.Nome)
                .ThenBy(item => item.ClienteId)
                .ToList();

            return new AtualizarPendenciasDto
            {
                QuantidadeOrdensAtualizadas = pagamentosPendentes.Count,
                ValorTotalCredito = clientesAtualizados.Sum(item => item.ValorAtualizado),
                ClientesAtualizados = clientesAtualizados
            };
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
                .SingleOrDefaultAsync(item => item.Id == lojaId, cancellationToken)
                ?? throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");

            return loja.UsuarioId != usuarioId
                ? throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.")
                : loja;
        }

        private async Task<ClienteCreditoModel> ObterOuCriarCreditoAsync(int lojaId, int clienteId, CancellationToken cancellationToken)
        {
            ClienteCreditoModel? credito = await _context.ClientesCreditos
                .SingleOrDefaultAsync(item => item.ClienteId == clienteId, cancellationToken);

            if (credito is not null)
            {
                return credito;
            }

            credito = new ClienteCreditoModel
            {
                LojaId = lojaId,
                ClienteId = clienteId,
                Valor = 0m
            };

            _ = await _context.ClientesCreditos.AddAsync(credito, cancellationToken);
            return credito;
        }

        private async Task<decimal> CalcularValorResgateAsync(
            int lojaId,
            decimal valorCredito,
            ClienteCreditoModel credito,
            CancellationToken cancellationToken)
        {
            if (credito.Valor <= 0)
            {
                throw new InvalidOperationException("Cliente deve possuir credito positivo para realizar resgate.");
            }

            if (credito.Valor < valorCredito)
            {
                throw new InvalidOperationException("Cliente nao possui credito suficiente para resgate.");
            }

            ConfigLojaModel config = await _context.ConfiguracoesLoja
                .SingleOrDefaultAsync(item => item.LojaId == lojaId, cancellationToken)
                ?? throw new InvalidOperationException("Loja nao possui configuracao de repasse ao fornecedor.");

            if (config.PercentualRepasseVendedorCredito <= 0)
            {
                throw new InvalidOperationException("Percentual de repasse ao fornecedor em credito deve ser maior que zero.");
            }

            return decimal.Round(
                valorCredito * config.PercentualRepasseFornecedor / config.PercentualRepasseVendedorCredito,
                2,
                MidpointRounding.AwayFromZero);
        }

        private static DateTime NormalizarDataLimiteParaFimDoDiaUtc(DateTime value)
        {
            DateTime utcValue = value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => value
            };

            return utcValue.TimeOfDay == TimeSpan.Zero
                ? utcValue.Date.AddDays(1).AddTicks(-1)
                : utcValue;
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

        private static PagamentoDto Mapear(PagamentoModel pagamento)
        {
            return new PagamentoDto
            {
                Id = pagamento.Id,
                MovimentacaoId = pagamento.MovimentacaoId,
                LojaId = pagamento.LojaId,
                ClienteId = pagamento.ClienteId,
                Natureza = pagamento.Natureza,
                Status = pagamento.Status,
                Valor = pagamento.Valor,
                Data = pagamento.Data
            };
        }

        private static PagamentoCreditoDto Mapear(PagamentoCreditoModel pagamento)
        {
            return new PagamentoCreditoDto
            {
                Id = pagamento.Id,
                LojaId = pagamento.LojaId,
                ClienteId = pagamento.ClienteId,
                Tipo = pagamento.Tipo,
                ValorCredito = pagamento.ValorCredito,
                ValorDinheiro = pagamento.ValorDinheiro,
                Data = pagamento.Data
            };
        }
    }
}
