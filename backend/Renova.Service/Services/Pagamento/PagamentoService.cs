using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Pagamento;

namespace Renova.Service.Services.Pagamento
{
    public class PagamentoService(RenovaDbContext context) : IPagamentoService
    {
        private readonly RenovaDbContext _context = context;

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
                        Valor = valorFornecedor,
                        Data = request.Data
                    };
                }));

            await _context.Pagamentos.AddRangeAsync(pagamentos, cancellationToken);
            await AtualizarCreditosAsync(pagamentos, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return [.. pagamentos.Select(Mapear)];
        }

        private async Task AtualizarCreditosAsync(IEnumerable<PagamentoModel> pagamentos, CancellationToken cancellationToken)
        {
            List<PagamentoModel> pagamentosList = [.. pagamentos];
            List<int> clientesIds = [.. pagamentosList.Select(item => item.ClienteId).Distinct()];

            Dictionary<int, ClienteCreditoModel> creditosPorCliente = await _context.ClientesCreditos
                .Where(item => clientesIds.Contains(item.ClienteId))
                .ToDictionaryAsync(item => item.ClienteId, cancellationToken);

            foreach (PagamentoModel pagamento in pagamentosList)
            {
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

                credito.Valor += pagamento.Natureza switch
                {
                    NaturezaPagamento.Pagar => pagamento.Valor,
                    NaturezaPagamento.Receber => -pagamento.Valor,
                    _ => throw new ArgumentOutOfRangeException(nameof(PagamentoModel.Natureza), "Natureza de pagamento desconhecida.")
                };
            }
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
                Valor = pagamento.Valor,
                Data = pagamento.Data
            };
        }
    }
}
