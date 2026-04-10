using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Parameters.Pagamento;

namespace Renova.Service.Services.Pagamento
{
    public class PagamentoService(RenovaDbContext context) : IPagamentoService
    {
        private readonly RenovaDbContext _context = context;

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
            if (request.ProdutoIds.Count == 0)
            {
                throw new ArgumentException("Ao menos um produto deve ser informado para gerar pagamentos.", nameof(request));
            }

            ConfigLojaModel config = await _context.ConfiguracoesLoja
                .SingleOrDefaultAsync(item => item.LojaId == request.LojaId, cancellationToken)
                ?? throw new InvalidOperationException("Loja nao possui configuracao de repasse ao fornecedor.");

            List<ProdutoEstoqueModel> produtos = await _context.ProdutosEstoque
                .Where(item => request.ProdutoIds.Contains(item.Id))
                .OrderBy(item => item.Id)
                .ToListAsync(cancellationToken);

            if (produtos.Count != request.ProdutoIds.Distinct().Count())
            {
                throw new ArgumentException("Um ou mais produtos informados nao foram encontrados.", nameof(request));
            }

            if (produtos.Any(item => item.LojaId != request.LojaId))
            {
                throw new ArgumentException("Os produtos informados devem pertencer a loja selecionada.", nameof(request));
            }

            decimal valorTotal = produtos.Sum(item => item.Preco);

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
                Valor = valorTotal,
                Data = request.Data
            };

            List<PagamentoModel> pagamentos = [pagamentoCliente];
            pagamentos.AddRange(produtos
                .GroupBy(item => item.FornecedorId)
                .Select(grupo =>
                {
                    decimal valorFornecedor = decimal.Round(
                        grupo.Sum(item => item.Preco) * (config.PercentualRepasseVendedorCredito / 100m),
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
                Data = request.Data
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
            List<PagamentoCreditoModel> pagamentosCredito = [];

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

                pagamentosCredito.Add(new PagamentoCreditoModel
                {
                    LojaId = pagamento.LojaId,
                    ClienteId = pagamento.ClienteId,
                    Tipo = variacaoCredito >= 0
                        ? TipoPagamentoCredito.AdicionarCredito
                        : TipoPagamentoCredito.ResgatarCredito,
                    ValorCredito = decimal.Abs(variacaoCredito),
                    ValorDinheiro = decimal.Abs(variacaoCredito),
                    Data = dataLimite
                });

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

            await _context.PagamentosCredito.AddRangeAsync(pagamentosCredito, cancellationToken);
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
