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
            decimal valorFornecedor = decimal.Round(valorTotal * (config.PercentualRepasseFornecedor / 100m), 2, MidpointRounding.AwayFromZero);
            int fornecedorId = produtos[0].FornecedorId;

            if (produtos.Any(item => item.FornecedorId != fornecedorId))
            {
                throw new InvalidOperationException("Todos os produtos da movimentacao devem possuir o mesmo fornecedor para gerar os pagamentos.");
            }

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

            PagamentoModel pagamentoFornecedor = new()
            {
                MovimentacaoId = request.MovimentacaoId,
                LojaId = request.LojaId,
                ClienteId = fornecedorId,
                Natureza = naturezaFornecedor,
                Valor = valorFornecedor,
                Data = request.Data
            };

            _ = await _context.Pagamentos.AddRangeAsync([pagamentoCliente, pagamentoFornecedor], cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return
            [
                Mapear(pagamentoCliente),
                Mapear(pagamentoFornecedor)
            ];
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
