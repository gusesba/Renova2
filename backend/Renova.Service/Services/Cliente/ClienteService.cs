using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

using Renova.Domain.Access;
using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cliente;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Queries.Cliente;
using Renova.Service.Services.Acesso;
using System.Linq.Expressions;

namespace Renova.Service.Services.Cliente
{
    public class ClienteService(RenovaDbContext context, ILojaAuthorizationService? authorizationService = null) : IClienteService
    {
        private readonly RenovaDbContext _context = context;
        private readonly ILojaAuthorizationService _authorizationService = authorizationService ?? NullLojaAuthorizationService.Instance;
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<ClienteModel, int>>)(cliente => cliente.Id),
            ["nome"] = (Expression<Func<ClienteModel, string>>)(cliente => cliente.Nome),
            ["contato"] = (Expression<Func<ClienteModel, string>>)(cliente => cliente.Contato)
        };

        private static string? NormalizarObs(string? obs)
        {
            if (string.IsNullOrWhiteSpace(obs))
            {
                return null;
            }

            return obs.Trim();
        }

        public async Task<byte[]> ExportClosingAsync(
            ExportarFechamentoClientesQuery request,
            ObterClientesParametros parametros,
            CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            if (!request.DataInicial.HasValue || !request.DataFinal.HasValue)
            {
                throw new ArgumentException("Data inicial e data final sao obrigatorias.", nameof(request));
            }

            DateTime dataInicialUtc = NormalizarDateTimeParaUtc(request.DataInicial.Value);
            DateTime dataFinalUtc = NormalizarDateTimeParaUtc(request.DataFinal.Value);

            if (dataFinalUtc < dataInicialUtc)
            {
                throw new ArgumentException("Data final deve ser maior ou igual a data inicial.", nameof(request));
            }

            await _authorizationService.EnsurePermissionAsync(request.LojaId.Value, parametros.UsuarioId, FuncionalidadeCatalogo.ClientesExportarFechamento, cancellationToken);

            ConfigLojaModel config = await _context.ConfiguracoesLoja
                .SingleOrDefaultAsync(item => item.LojaId == request.LojaId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Loja nao possui configuracao de repasse ao fornecedor.");

            List<ClienteModel> clientesElegiveis = await _context.Clientes
                .Where(cliente =>
                    cliente.LojaId == request.LojaId.Value
                    && !cliente.Doacao
                    && _context.ProdutosEstoque.Any(produto =>
                        produto.LojaId == request.LojaId.Value
                        && produto.FornecedorId == cliente.Id
                        && produto.Consignado
                        && ((produto.Entrada >= dataInicialUtc && produto.Entrada <= dataFinalUtc)
                            || (produto.Situacao == SituacaoProduto.Vendido
                                && produto.Movimentacoes.Any(movimentacaoProduto =>
                                    movimentacaoProduto.Movimentacao != null
                                    && movimentacaoProduto.Movimentacao.Tipo == TipoMovimentacao.Venda
                                    && movimentacaoProduto.Movimentacao.Data >= dataInicialUtc
                                    && movimentacaoProduto.Movimentacao.Data <= dataFinalUtc)))))
                .OrderBy(cliente => cliente.Nome)
                .ThenBy(cliente => cliente.Id)
                .ToListAsync(cancellationToken);

            Dictionary<int, decimal> saldosAnterioresPorCliente = await CalcularSaldosAntesDoPeriodoAsync(
                request.LojaId.Value,
                clientesElegiveis.Select(cliente => cliente.Id).ToList(),
                dataInicialUtc,
                cancellationToken);

            using XLWorkbook workbook = new();
            HashSet<string> nomesAbas = [];

            foreach (ClienteModel cliente in clientesElegiveis)
            {
                List<FechamentoClienteEntradaItem> entradas = await _context.ProdutosEstoque
                    .Where(produto =>
                        produto.LojaId == request.LojaId.Value
                        && produto.FornecedorId == cliente.Id
                        && produto.Entrada >= dataInicialUtc
                        && produto.Entrada <= dataFinalUtc)
                    .OrderBy(produto => produto.Entrada)
                    .ThenBy(produto => produto.Id)
                    .Select(produto => new FechamentoClienteEntradaItem
                    {
                        Id = produto.Id,
                        Valor = produto.Preco,
                        Produto = produto.Produto != null ? produto.Produto.Valor : string.Empty,
                        Marca = produto.Marca != null ? produto.Marca.Valor : string.Empty,
                        Tamanho = produto.Tamanho != null ? produto.Tamanho.Valor : string.Empty,
                        Cor = produto.Cor != null ? produto.Cor.Valor : string.Empty,
                        Observacao = produto.Descricao,
                        DataEntrada = produto.Entrada
                    })
                    .ToListAsync(cancellationToken);

                List<FechamentoClienteVendaItem> vendas = await _context.MovimentacoesProdutos
                    .Where(item =>
                        item.Produto != null
                        && item.Movimentacao != null
                        && item.Produto.LojaId == request.LojaId.Value
                        && item.Produto.FornecedorId == cliente.Id
                        && item.Produto.Consignado
                        && item.Produto.Situacao == SituacaoProduto.Vendido
                        && item.Movimentacao.Tipo == TipoMovimentacao.Venda
                        && item.Movimentacao.Data >= dataInicialUtc
                        && item.Movimentacao.Data <= dataFinalUtc)
                    .OrderBy(item => item.Movimentacao!.Data)
                    .ThenBy(item => item.ProdutoId)
                    .Select(item => new FechamentoClienteVendaItem
                    {
                        Id = item.ProdutoId,
                        IdVenda = item.MovimentacaoId,
                        Valor = item.Produto != null ? item.Produto.Preco : 0m,
                        ValorVenda = item.Produto != null
                            ? decimal.Round(
                                item.Produto.Preco * ((100m - item.Desconto) / 100m),
                                2,
                                MidpointRounding.AwayFromZero)
                            : 0m,
                        Produto = item.Produto != null && item.Produto.Produto != null ? item.Produto.Produto.Valor : string.Empty,
                        Marca = item.Produto != null && item.Produto.Marca != null ? item.Produto.Marca.Valor : string.Empty,
                        Tamanho = item.Produto != null && item.Produto.Tamanho != null ? item.Produto.Tamanho.Valor : string.Empty,
                        Cor = item.Produto != null && item.Produto.Cor != null ? item.Produto.Cor.Valor : string.Empty,
                        Descricao = item.Produto != null ? item.Produto.Descricao : string.Empty,
                        DataEntrada = item.Produto != null ? item.Produto.Entrada : default,
                        DataSaida = item.Movimentacao != null ? item.Movimentacao.Data : default
                    })
                    .ToListAsync(cancellationToken);

                decimal valorTotalVenda = vendas.Sum(item => item.ValorVenda);
                decimal repasseBrutoDinheiro = decimal.Round(
                    valorTotalVenda * (config.PercentualRepasseFornecedor / 100m),
                    2,
                    MidpointRounding.AwayFromZero);
                decimal repasseBrutoCredito = decimal.Round(
                    valorTotalVenda * (config.PercentualRepasseVendedorCredito / 100m),
                    2,
                    MidpointRounding.AwayFromZero);

                decimal debitoAnterior = 0m;
                if (saldosAnterioresPorCliente.TryGetValue(cliente.Id, out decimal saldoAnterior) && saldoAnterior < 0)
                {
                    debitoAnterior = decimal.Round(-saldoAnterior, 2, MidpointRounding.AwayFromZero);
                }

                decimal repasseLiquidoDinheiro = decimal.Round(
                    Math.Max(0m, repasseBrutoDinheiro - debitoAnterior),
                    2,
                    MidpointRounding.AwayFromZero);
                decimal repasseLiquidoCredito = decimal.Round(
                    Math.Max(0m, repasseBrutoCredito - debitoAnterior),
                    2,
                    MidpointRounding.AwayFromZero);

                IXLWorksheet worksheet = workbook.Worksheets.Add(GerarNomeAbaUnico(cliente.Nome, nomesAbas));
                PreencherPlanilhaFechamentoCliente(
                    worksheet,
                    cliente.Nome,
                    dataInicialUtc,
                    dataFinalUtc,
                    debitoAnterior,
                    repasseBrutoDinheiro,
                    repasseLiquidoDinheiro,
                    repasseBrutoCredito,
                    repasseLiquidoCredito,
                    entradas,
                    vendas);
            }

            if (workbook.Worksheets.Count == 0)
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Resumo");
                worksheet.Cell(1, 1).Value = "Nenhum cliente elegivel para o fechamento no periodo informado.";
                worksheet.Cell(2, 1).Value = "Pre-requisitos aplicados: cliente nao marcado como doacao e com movimentacao consignada no periodo.";
                worksheet.Columns().AdjustToContents();
            }

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<ClienteDto> CreateAsync(CriarClienteCommand request, CriarClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();
            string contatoNormalizado = request.Contato.KeepOnlyDigits();

            if (contatoNormalizado.Length is not (10 or 11))
            {
                throw new ArgumentException("Contato deve conter 10 ou 11 numeros.", nameof(request));
            }

            await _authorizationService.EnsurePermissionAsync(request.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.ClientesAdicionar, cancellationToken);
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            if (request.UserId.HasValue)
            {
                bool contaExiste = await _context.Usuarios
                    .AnyAsync(usuario => usuario.Id == request.UserId.Value, cancellationToken);

                if (!contaExiste)
                {
                    throw new InvalidOperationException("Conta informada para vinculo nao foi encontrada.");
                }
            }

            bool clienteJaExiste = await _context.Clientes
                .AnyAsync(cliente => cliente.LojaId == request.LojaId && cliente.Nome == nomeNormalizado, cancellationToken);

            if (clienteJaExiste)
            {
                throw new InvalidOperationException("Loja ja possui um cliente com este nome.");
            }

            ClienteModel cliente = new()
            {
                Nome = nomeNormalizado,
                Contato = contatoNormalizado,
                Obs = NormalizarObs(request.Obs),
                Doacao = request.Doacao,
                LojaId = request.LojaId,
                UserId = request.UserId
            };

            _ = await _context.Clientes.AddAsync(cliente, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Contato = cliente.Contato,
                Obs = cliente.Obs,
                Doacao = cliente.Doacao,
                LojaId = cliente.LojaId,
                UserId = cliente.UserId,
                UserNome = null,
                UserEmail = null
            };
        }

        public async Task<ClienteDto> EditAsync(EditarClienteCommand request, EditarClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();
            string contatoNormalizado = request.Contato.KeepOnlyDigits();

            if (contatoNormalizado.Length is not (10 or 11))
            {
                throw new ArgumentException("Contato deve conter 10 ou 11 numeros.", nameof(request));
            }

            ClienteModel? cliente = await _context.Clientes
                .SingleOrDefaultAsync(clienteAtual => clienteAtual.Id == parametros.ClienteId, cancellationToken);

            if (cliente is null)
            {
                throw new KeyNotFoundException("Cliente informado nao foi encontrado.");
            }

            await _authorizationService.EnsurePermissionAsync(cliente.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.ClientesEditar, cancellationToken);
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(cliente.LojaId, parametros.UsuarioId, cancellationToken);

            if (request.UserId.HasValue)
            {
                bool contaExiste = await _context.Usuarios
                    .AnyAsync(usuario => usuario.Id == request.UserId.Value, cancellationToken);

                if (!contaExiste)
                {
                    throw new InvalidOperationException("Conta informada para vinculo nao foi encontrada.");
                }
            }

            bool clienteJaExiste = await _context.Clientes
                .AnyAsync(clienteAtual =>
                    clienteAtual.LojaId == cliente.LojaId &&
                    clienteAtual.Id != cliente.Id &&
                    clienteAtual.Nome == nomeNormalizado,
                    cancellationToken);

            if (clienteJaExiste)
            {
                throw new InvalidOperationException("Loja ja possui um cliente com este nome.");
            }

            cliente.Nome = nomeNormalizado;
            cliente.Contato = contatoNormalizado;
            cliente.Obs = NormalizarObs(request.Obs);
            cliente.Doacao = request.Doacao;
            cliente.UserId = request.UserId;

            _ = await _context.SaveChangesAsync(cancellationToken);

            return new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Contato = cliente.Contato,
                Obs = cliente.Obs,
                Doacao = cliente.Doacao,
                LojaId = cliente.LojaId,
                UserId = cliente.UserId,
                UserNome = null,
                UserEmail = null
            };
        }

        public async Task DeleteAsync(ExcluirClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            ClienteModel? cliente = await _context.Clientes
                .SingleOrDefaultAsync(clienteAtual => clienteAtual.Id == parametros.ClienteId, cancellationToken);

            if (cliente is null)
            {
                throw new KeyNotFoundException("Cliente informado nao foi encontrado.");
            }

            await _authorizationService.EnsurePermissionAsync(cliente.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.ClientesExcluir, cancellationToken);

            if (await ClientePossuiProdutosVinculadosAsync(cliente.Id, cancellationToken))
            {
                throw new InvalidOperationException("Cliente possui produtos vinculados e nao pode ser excluido.");
            }

            if (await ClientePossuiMovimentacoesVinculadasAsync(cliente.Id, cancellationToken))
            {
                throw new InvalidOperationException("Cliente possui movimentacoes vinculadas e nao pode ser excluido.");
            }

            if (await ClientePossuiPagamentosVinculadosAsync(cliente.Id, cancellationToken))
            {
                throw new InvalidOperationException("Cliente possui pagamentos vinculados e nao pode ser excluido.");
            }

            if (await ClientePossuiPagamentosCreditoVinculadosAsync(cliente.Id, cancellationToken))
            {
                throw new InvalidOperationException("Cliente possui pagamentos de credito vinculados e nao pode ser excluido.");
            }

            _ = _context.Clientes.Remove(cliente);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<PaginacaoDto<ClienteDto>> GetAllAsync(ObterClientesQuery request, ObterClientesParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            await _authorizationService.EnsurePermissionAsync(request.LojaId.Value, parametros.UsuarioId, FuncionalidadeCatalogo.ClientesVisualizar, cancellationToken);

            IQueryable<ClienteModel> query = _context.Clientes
                .Where(cliente => cliente.LojaId == request.LojaId.Value);

            if (!string.IsNullOrWhiteSpace(request.Nome))
            {
                string nomeFiltro = request.Nome.Trim().ToLowerInvariant();
                query = query.Where(cliente => cliente.Nome.ToLower().Contains(nomeFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Contato))
            {
                string contatoFiltro = request.Contato.Trim().ToLowerInvariant();
                query = query.Where(cliente => cliente.Contato.ToLower().Contains(contatoFiltro));
            }

            IQueryable<ClienteModel> queryOrdenada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveis, "nome")
                .ThenBy(cliente => cliente.Id);

            IQueryable<ClienteDto> queryProjetada = queryOrdenada.Select(cliente => new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Contato = cliente.Contato,
                Obs = cliente.Obs,
                Doacao = cliente.Doacao,
                LojaId = cliente.LojaId,
                UserId = cliente.UserId,
                UserNome = cliente.User != null ? cliente.User.Nome : null,
                UserEmail = cliente.User != null ? cliente.User.Email : null
            });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        public async Task<ClienteDetalheDto> GetDetailAsync(
            ObterClienteDetalheQuery request,
            ObterClienteDetalheParametros parametros,
            CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            await _authorizationService.EnsurePermissionAsync(request.LojaId.Value, parametros.UsuarioId, FuncionalidadeCatalogo.ClientesVisualizarDetalhe, cancellationToken);
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId.Value, parametros.UsuarioId, cancellationToken);

            ClienteModel cliente = await _context.Clientes
                .Include(item => item.User)
                .SingleOrDefaultAsync(item => item.Id == parametros.ClienteId, cancellationToken)
                ?? throw new KeyNotFoundException("Cliente informado nao foi encontrado.");

            if (cliente.LojaId != loja.Id)
            {
                throw new UnauthorizedAccessException("Cliente informado nao pertence a loja selecionada.");
            }

            if (request.Situacao.HasValue && !Enum.IsDefined(request.Situacao.Value))
            {
                throw new ArgumentException("Situacao informada e invalida.", nameof(request));
            }

            DateTime? dataInicialUtc = request.DataInicial.HasValue
                ? NormalizarDateTimeParaUtc(request.DataInicial.Value)
                : null;
            DateTime? dataFinalUtc = request.DataFinal.HasValue
                ? NormalizarDateTimeParaUtc(request.DataFinal.Value)
                : null;

            IQueryable<ProdutoEstoqueModel> produtosFornecedorQuery = _context.ProdutosEstoque
                .Where(produto => produto.LojaId == loja.Id && produto.FornecedorId == cliente.Id);

            if (dataInicialUtc.HasValue)
            {
                produtosFornecedorQuery = produtosFornecedorQuery.Where(produto => produto.Entrada >= dataInicialUtc.Value);
            }

            if (dataFinalUtc.HasValue)
            {
                produtosFornecedorQuery = produtosFornecedorQuery.Where(produto => produto.Entrada <= dataFinalUtc.Value);
            }

            if (request.Situacao.HasValue)
            {
                produtosFornecedorQuery = produtosFornecedorQuery.Where(produto => produto.Situacao == request.Situacao.Value);
            }

            IQueryable<ProdutoEstoqueModel> produtosComClienteQuery = _context.ProdutosEstoque
                .Where(produto => produto.LojaId == loja.Id)
                .Where(produto => produto.Movimentacoes
                    .Where(movimentacaoProduto => movimentacaoProduto.Movimentacao != null)
                    .OrderByDescending(movimentacaoProduto => movimentacaoProduto.Movimentacao!.Data)
                    .ThenByDescending(movimentacaoProduto => movimentacaoProduto.MovimentacaoId)
                    .Take(1)
                    .Any(movimentacaoProduto =>
                        movimentacaoProduto.Movimentacao!.ClienteId == cliente.Id
                        && (movimentacaoProduto.Movimentacao.Tipo == TipoMovimentacao.Venda
                            || movimentacaoProduto.Movimentacao.Tipo == TipoMovimentacao.Emprestimo)));

            if (dataInicialUtc.HasValue)
            {
                produtosComClienteQuery = produtosComClienteQuery.Where(produto => produto.Movimentacoes
                    .Where(movimentacaoProduto => movimentacaoProduto.Movimentacao != null)
                    .OrderByDescending(movimentacaoProduto => movimentacaoProduto.Movimentacao!.Data)
                    .ThenByDescending(movimentacaoProduto => movimentacaoProduto.MovimentacaoId)
                    .Take(1)
                    .Any(movimentacaoProduto => movimentacaoProduto.Movimentacao!.Data >= dataInicialUtc.Value));
            }

            if (dataFinalUtc.HasValue)
            {
                produtosComClienteQuery = produtosComClienteQuery.Where(produto => produto.Movimentacoes
                    .Where(movimentacaoProduto => movimentacaoProduto.Movimentacao != null)
                    .OrderByDescending(movimentacaoProduto => movimentacaoProduto.Movimentacao!.Data)
                    .ThenByDescending(movimentacaoProduto => movimentacaoProduto.MovimentacaoId)
                    .Take(1)
                    .Any(movimentacaoProduto => movimentacaoProduto.Movimentacao!.Data <= dataFinalUtc.Value));
            }

            if (request.Situacao.HasValue)
            {
                produtosComClienteQuery = produtosComClienteQuery.Where(produto => produto.Situacao == request.Situacao.Value);
            }

            int quantidadePecasCompradas = await _context.MovimentacoesProdutos
                .Where(item => item.Movimentacao != null
                    && item.Movimentacao.LojaId == loja.Id
                    && item.Movimentacao.ClienteId == cliente.Id
                    && item.Movimentacao.Tipo == TipoMovimentacao.Venda)
                .Where(item => !dataInicialUtc.HasValue || item.Movimentacao!.Data >= dataInicialUtc.Value)
                .Where(item => !dataFinalUtc.HasValue || item.Movimentacao!.Data <= dataFinalUtc.Value)
                .CountAsync(cancellationToken);

            int quantidadePecasVendidas = await _context.MovimentacoesProdutos
                .Where(item => item.Movimentacao != null
                    && item.Movimentacao.LojaId == loja.Id
                    && item.Movimentacao.Tipo == TipoMovimentacao.Venda
                    && item.Produto != null
                    && item.Produto.FornecedorId == cliente.Id)
                .Where(item => !dataInicialUtc.HasValue || item.Movimentacao!.Data >= dataInicialUtc.Value)
                .Where(item => !dataFinalUtc.HasValue || item.Movimentacao!.Data <= dataFinalUtc.Value)
                .CountAsync(cancellationToken);

            decimal valorAportadoLoja = await _context.PagamentosCredito
                .Where(item => item.LojaId == loja.Id
                    && item.ClienteId == cliente.Id
                    && item.Tipo == TipoPagamentoCredito.AdicionarCredito)
                .Where(item => !dataInicialUtc.HasValue || item.Data >= dataInicialUtc.Value)
                .Where(item => !dataFinalUtc.HasValue || item.Data <= dataFinalUtc.Value)
                .SumAsync(item => (decimal?)item.ValorDinheiro, cancellationToken) ?? 0m;

            decimal valorRetiradoLoja = await _context.PagamentosCredito
                .Where(item => item.LojaId == loja.Id
                    && item.ClienteId == cliente.Id
                    && item.Tipo == TipoPagamentoCredito.ResgatarCredito)
                .Where(item => !dataInicialUtc.HasValue || item.Data >= dataInicialUtc.Value)
                .Where(item => !dataFinalUtc.HasValue || item.Data <= dataFinalUtc.Value)
                .SumAsync(item => (decimal?)item.ValorDinheiro, cancellationToken) ?? 0m;

            List<ProdutoBuscaDto> produtosFornecedor = await produtosFornecedorQuery
                .OrderByDescending(produto => produto.Entrada)
                .ThenByDescending(produto => produto.Id)
                .Select(MapearProdutoBuscaDto())
                .ToListAsync(cancellationToken);

            List<ProdutoBuscaDto> produtosComCliente = await produtosComClienteQuery
                .OrderByDescending(produto => produto.Id)
                .Select(MapearProdutoBuscaDto())
                .ToListAsync(cancellationToken);

            return new ClienteDetalheDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Contato = cliente.Contato,
                Obs = cliente.Obs,
                Doacao = cliente.Doacao,
                LojaId = cliente.LojaId,
                UserId = cliente.UserId,
                UserNome = cliente.User != null ? cliente.User.Nome : null,
                UserEmail = cliente.User != null ? cliente.User.Email : null,
                QuantidadePecasCompradas = quantidadePecasCompradas,
                QuantidadePecasVendidas = quantidadePecasVendidas,
                ValorAportadoLoja = valorAportadoLoja,
                ValorRetiradoLoja = valorRetiradoLoja,
                ProdutosFornecedor = produtosFornecedor,
                ProdutosComCliente = produtosComCliente
            };
        }

        private async Task<Dictionary<int, decimal>> CalcularSaldosAntesDoPeriodoAsync(
            int lojaId,
            IReadOnlyCollection<int> clienteIds,
            DateTime dataInicialUtc,
            CancellationToken cancellationToken)
        {
            if (clienteIds.Count == 0)
            {
                return [];
            }

            List<SaldoClienteItem> saldosPagamentos = await _context.Pagamentos
                .Where(item =>
                    item.LojaId == lojaId
                    && clienteIds.Contains(item.ClienteId)
                    && item.Status == StatusPagamento.Pago
                    && item.Data < dataInicialUtc)
                .Select(item => new SaldoClienteItem
                {
                    ClienteId = item.ClienteId,
                    Valor = item.Natureza == NaturezaPagamento.Pagar ? item.Valor : -item.Valor
                })
                .ToListAsync(cancellationToken);

            List<SaldoClienteItem> saldosCredito = await _context.PagamentosCredito
                .Where(item =>
                    item.LojaId == lojaId
                    && clienteIds.Contains(item.ClienteId)
                    && item.Data < dataInicialUtc)
                .Select(item => new SaldoClienteItem
                {
                    ClienteId = item.ClienteId,
                    Valor = item.Tipo == TipoPagamentoCredito.AdicionarCredito ? item.ValorCredito : -item.ValorCredito
                })
                .ToListAsync(cancellationToken);

            return saldosPagamentos
                .Concat(saldosCredito)
                .GroupBy(item => item.ClienteId)
                .ToDictionary(
                    group => group.Key,
                    group => decimal.Round(group.Sum(item => item.Valor), 2, MidpointRounding.AwayFromZero));
        }

        private static string GerarNomeAbaUnico(string nomeCliente, ISet<string> nomesExistentes)
        {
            string baseNome = string.IsNullOrWhiteSpace(nomeCliente) ? "Cliente" : nomeCliente.Trim();

            foreach (char caractereInvalido in new[] { ':', '\\', '/', '?', '*', '[', ']' })
            {
                baseNome = baseNome.Replace(caractereInvalido, '-');
            }

            if (baseNome.Length > 31)
            {
                baseNome = baseNome[..31];
            }

            string nomeAtual = baseNome;
            int sufixo = 1;

            while (!nomesExistentes.Add(nomeAtual))
            {
                string textoSufixo = $" ({sufixo++})";
                int tamanhoBase = Math.Max(1, 31 - textoSufixo.Length);
                nomeAtual = $"{baseNome[..Math.Min(baseNome.Length, tamanhoBase)]}{textoSufixo}";
            }

            return nomeAtual;
        }

        private static void PreencherPlanilhaFechamentoCliente(
            IXLWorksheet worksheet,
            string nomeCliente,
            DateTime dataInicialUtc,
            DateTime dataFinalUtc,
            decimal debitoAnterior,
            decimal repasseBrutoDinheiro,
            decimal repasseLiquidoDinheiro,
            decimal repasseBrutoCredito,
            decimal repasseLiquidoCredito,
            IReadOnlyList<FechamentoClienteEntradaItem> entradas,
            IReadOnlyList<FechamentoClienteVendaItem> vendas)
        {
            XLColor corTitulo = XLColor.FromHtml("#1F3A5F");
            XLColor corTexto = XLColor.FromHtml("#4B5D70");
            XLColor corBorda = XLColor.FromHtml("#D7E0EA");
            XLColor corCardAzul = XLColor.FromHtml("#EEF4FF");
            XLColor corCardLaranja = XLColor.FromHtml("#FFF4E8");
            XLColor corCardVerde = XLColor.FromHtml("#EEFDF3");
            XLColor corCabecalhoTabela = XLColor.FromHtml("#E9EFF6");
            XLColor corLinhaAlternada = XLColor.FromHtml("#F8FBFF");
            XLColor corLinhaVazia = XLColor.FromHtml("#F8FAFC");

            worksheet.Style.Font.FontName = "Aptos";
            worksheet.Style.Font.FontSize = 11;
            worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Range("A1:K1").Merge();
            worksheet.Cell("A1").Value = "Fechamento do cliente";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 20;
            worksheet.Cell("A1").Style.Font.FontColor = XLColor.White;
            worksheet.Cell("A1").Style.Fill.BackgroundColor = corTitulo;
            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(1).Height = 32;

            worksheet.Range("A2:K2").Merge();
            worksheet.Cell("A2").Value = "Planilha detalhada com entradas, vendas e calculos de repasse";
            worksheet.Cell("A2").Style.Font.FontColor = corTexto;
            worksheet.Cell("A2").Style.Font.Italic = true;
            worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            AplicarCardTexto(worksheet, 4, 1, 3, 2, "Cliente", nomeCliente, corCardAzul, corTitulo, corBorda);
            AplicarCardTexto(
                worksheet,
                4,
                4,
                3,
                2,
                "Periodo",
                $"{dataInicialUtc:dd/MM/yyyy} a {dataFinalUtc:dd/MM/yyyy}",
                corCardAzul,
                corTitulo,
                corBorda);
            AplicarCardTexto(
                worksheet,
                4,
                7,
                3,
                2,
                "Debito previo",
                debitoAnterior > 0 ? $"R$ {debitoAnterior:0.00}" : "Sem debito",
                corCardLaranja,
                corTitulo,
                corBorda);
            AplicarCardTexto(
                worksheet,
                4,
                10,
                3,
                2,
                "Observacao",
                debitoAnterior > 0
                    ? $"Foi descontado R$ {debitoAnterior:0.00} de debito previo."
                    : "Nao havia debito previo para descontar.",
                corCardLaranja,
                corTitulo,
                corBorda);

            AplicarCardMoeda(worksheet, 8, 1, 3, 2, "Repasse bruto em dinheiro", repasseBrutoDinheiro, corCardAzul, corTitulo, corBorda);
            AplicarCardMoeda(worksheet, 8, 4, 3, 2, "Repasse liquido em dinheiro", repasseLiquidoDinheiro, corCardAzul, corTitulo, corBorda);
            AplicarCardMoeda(worksheet, 8, 7, 3, 2, "Repasse bruto em credito", repasseBrutoCredito, corCardVerde, corTitulo, corBorda);
            AplicarCardMoeda(worksheet, 8, 10, 3, 2, "Repasse liquido em credito", repasseLiquidoCredito, corCardVerde, corTitulo, corBorda);

            int linhaEntradas = 13;
            worksheet.Range(linhaEntradas, 1, linhaEntradas, 8).Merge();
            worksheet.Cell(linhaEntradas, 1).Value = "Pecas adicionadas na loja no periodo";
            AplicarTituloSecao(worksheet.Range(linhaEntradas, 1, linhaEntradas, 8), corTitulo);

            int cabecalhoEntradas = linhaEntradas + 1;
            string[] colunasEntrada = ["Id", "Valor", "Produto", "Marca", "Tamanho", "Cor", "Obs", "Data de entrada"];

            for (int indice = 0; indice < colunasEntrada.Length; indice++)
            {
                AplicarCabecalhoTabela(worksheet.Cell(cabecalhoEntradas, indice + 1), colunasEntrada[indice], corCabecalhoTabela, corTitulo, corBorda);
            }

            int linhaAtual = cabecalhoEntradas + 1;
            foreach (FechamentoClienteEntradaItem entrada in entradas)
            {
                worksheet.Cell(linhaAtual, 1).Value = entrada.Id;
                worksheet.Cell(linhaAtual, 2).Value = entrada.Valor;
                worksheet.Cell(linhaAtual, 3).Value = entrada.Produto;
                worksheet.Cell(linhaAtual, 4).Value = entrada.Marca;
                worksheet.Cell(linhaAtual, 5).Value = entrada.Tamanho;
                worksheet.Cell(linhaAtual, 6).Value = entrada.Cor;
                worksheet.Cell(linhaAtual, 7).Value = entrada.Observacao;
                worksheet.Cell(linhaAtual, 8).Value = entrada.DataEntrada;
                AplicarLinhaTabela(worksheet.Range(linhaAtual, 1, linhaAtual, 8), linhaAtual % 2 == 0 ? corLinhaAlternada : XLColor.White, corBorda);
                linhaAtual++;
            }

            if (entradas.Count == 0)
            {
                worksheet.Range(linhaAtual, 1, linhaAtual, 8).Merge();
                worksheet.Cell(linhaAtual, 1).Value = "Nenhuma peca adicionada no periodo.";
                AplicarLinhaVazia(worksheet.Range(linhaAtual, 1, linhaAtual, 8), corLinhaVazia, corTexto, corBorda);
                linhaAtual++;
            }
            else
            {
                worksheet.Range(cabecalhoEntradas + 1, 2, linhaAtual - 1, 2).Style.NumberFormat.Format = "R$ #,##0.00";
                worksheet.Range(cabecalhoEntradas + 1, 8, linhaAtual - 1, 8).Style.DateFormat.Format = "dd/MM/yyyy";
            }

            linhaAtual += 2;
            worksheet.Range(linhaAtual, 1, linhaAtual, 11).Merge();
            worksheet.Cell(linhaAtual, 1).Value = "Pecas vendidas no periodo";
            AplicarTituloSecao(worksheet.Range(linhaAtual, 1, linhaAtual, 11), corTitulo);

            int cabecalhoVendas = linhaAtual + 1;
            string[] colunasVenda = ["Id", "Id da venda", "Valor", "Valor Venda", "Produto", "Marca", "Tamanho", "Cor", "Descricao", "Data Entrada", "Data Saida"];

            for (int indice = 0; indice < colunasVenda.Length; indice++)
            {
                AplicarCabecalhoTabela(worksheet.Cell(cabecalhoVendas, indice + 1), colunasVenda[indice], corCabecalhoTabela, corTitulo, corBorda);
            }

            linhaAtual = cabecalhoVendas + 1;
            foreach (FechamentoClienteVendaItem venda in vendas)
            {
                worksheet.Cell(linhaAtual, 1).Value = venda.Id;
                worksheet.Cell(linhaAtual, 2).Value = venda.IdVenda;
                worksheet.Cell(linhaAtual, 3).Value = venda.Valor;
                worksheet.Cell(linhaAtual, 4).Value = venda.ValorVenda;
                worksheet.Cell(linhaAtual, 5).Value = venda.Produto;
                worksheet.Cell(linhaAtual, 6).Value = venda.Marca;
                worksheet.Cell(linhaAtual, 7).Value = venda.Tamanho;
                worksheet.Cell(linhaAtual, 8).Value = venda.Cor;
                worksheet.Cell(linhaAtual, 9).Value = venda.Descricao;
                worksheet.Cell(linhaAtual, 10).Value = venda.DataEntrada;
                worksheet.Cell(linhaAtual, 11).Value = venda.DataSaida;
                AplicarLinhaTabela(worksheet.Range(linhaAtual, 1, linhaAtual, 11), linhaAtual % 2 == 0 ? corLinhaAlternada : XLColor.White, corBorda);
                linhaAtual++;
            }

            if (vendas.Count == 0)
            {
                worksheet.Range(linhaAtual, 1, linhaAtual, 11).Merge();
                worksheet.Cell(linhaAtual, 1).Value = "Nenhuma peca vendida no periodo.";
                AplicarLinhaVazia(worksheet.Range(linhaAtual, 1, linhaAtual, 11), corLinhaVazia, corTexto, corBorda);
            }
            else
            {
                worksheet.Range(cabecalhoVendas + 1, 3, linhaAtual - 1, 4).Style.NumberFormat.Format = "R$ #,##0.00";
                worksheet.Range(cabecalhoVendas + 1, 10, linhaAtual - 1, 11).Style.DateFormat.Format = "dd/MM/yyyy";
            }

            worksheet.Columns("A:K").AdjustToContents();
            worksheet.Column("G").Width = Math.Max(22, worksheet.Column("G").Width);
            worksheet.Column("I").Width = Math.Max(26, worksheet.Column("I").Width);
            worksheet.Rows().AdjustToContents();
        }

        private static void AplicarTituloSecao(IXLRange range, XLColor corTitulo)
        {
            range.Style.Fill.BackgroundColor = corTitulo;
            range.Style.Font.FontColor = XLColor.White;
            range.Style.Font.Bold = true;
            range.Style.Font.FontSize = 13;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = corTitulo;
        }

        private static void AplicarCabecalhoTabela(IXLCell cell, string valor, XLColor corFundo, XLColor corTexto, XLColor corBorda)
        {
            cell.Value = valor;
            cell.Style.Fill.BackgroundColor = corFundo;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = corTexto;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = corBorda;
        }

        private static void AplicarLinhaTabela(IXLRange range, XLColor corFundo, XLColor corBorda)
        {
            range.Style.Fill.BackgroundColor = corFundo;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = corBorda;
            range.Style.Border.InsideBorderColor = corBorda;
        }

        private static void AplicarLinhaVazia(IXLRange range, XLColor corFundo, XLColor corTexto, XLColor corBorda)
        {
            range.Style.Fill.BackgroundColor = corFundo;
            range.Style.Font.FontColor = corTexto;
            range.Style.Font.Italic = true;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = corBorda;
        }

        private static void AplicarCardTexto(
            IXLWorksheet worksheet,
            int linhaInicial,
            int colunaInicial,
            int altura,
            int largura,
            string titulo,
            string valor,
            XLColor corFundo,
            XLColor corTitulo,
            XLColor corBorda)
        {
            IXLRange range = worksheet.Range(linhaInicial, colunaInicial, linhaInicial + altura - 1, colunaInicial + largura - 1);
            range.Merge();
            range.Style.Fill.BackgroundColor = corFundo;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            range.Style.Border.OutsideBorderColor = corBorda;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Alignment.WrapText = true;

            IXLCell cell = worksheet.Cell(linhaInicial, colunaInicial);
            cell.Value = $"{titulo}\n{valor}";
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = corTitulo;
        }

        private static void AplicarCardMoeda(
            IXLWorksheet worksheet,
            int linhaInicial,
            int colunaInicial,
            int altura,
            int largura,
            string titulo,
            decimal valor,
            XLColor corFundo,
            XLColor corTitulo,
            XLColor corBorda)
        {
            IXLRange range = worksheet.Range(linhaInicial, colunaInicial, linhaInicial + altura - 1, colunaInicial + largura - 1);
            range.Merge();
            range.Style.Fill.BackgroundColor = corFundo;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            range.Style.Border.OutsideBorderColor = corBorda;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            IXLCell cell = worksheet.Cell(linhaInicial, colunaInicial);
            cell.Value = $"{titulo}\nR$ {valor:0.00}";
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = corTitulo;
            cell.Style.Font.FontSize = 12;
            cell.Style.Alignment.WrapText = true;
        }

        private static DateTime NormalizarDateTimeParaUtc(DateTime data)
        {
            return data.Kind switch
            {
                DateTimeKind.Utc => data,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(data, DateTimeKind.Utc),
                _ => data.ToUniversalTime()
            };
        }

        private sealed class SaldoClienteItem
        {
            public int ClienteId { get; set; }

            public decimal Valor { get; set; }
        }

        private sealed class FechamentoClienteEntradaItem
        {
            public int Id { get; set; }

            public decimal Valor { get; set; }

            public required string Produto { get; set; }

            public required string Marca { get; set; }

            public required string Tamanho { get; set; }

            public required string Cor { get; set; }

            public required string Observacao { get; set; }

            public DateTime DataEntrada { get; set; }
        }

        private sealed class FechamentoClienteVendaItem
        {
            public int Id { get; set; }

            public int IdVenda { get; set; }

            public decimal Valor { get; set; }

            public decimal ValorVenda { get; set; }

            public required string Produto { get; set; }

            public required string Marca { get; set; }

            public required string Tamanho { get; set; }

            public required string Cor { get; set; }

            public required string Descricao { get; set; }

            public DateTime DataEntrada { get; set; }

            public DateTime DataSaida { get; set; }
        }

        private static System.Linq.Expressions.Expression<Func<ProdutoEstoqueModel, ProdutoBuscaDto>> MapearProdutoBuscaDto()
        {
            return produto => new ProdutoBuscaDto
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
                Consignado = produto.Consignado
            };
        }

        private Task<bool> ClientePossuiProdutosVinculadosAsync(int clienteId, CancellationToken cancellationToken)
        {
            return _context.ProdutosEstoque
                .AnyAsync(produto => produto.FornecedorId == clienteId, cancellationToken);
        }

        private Task<bool> ClientePossuiMovimentacoesVinculadasAsync(int clienteId, CancellationToken cancellationToken)
        {
            return _context.Movimentacoes
                .AnyAsync(movimentacao => movimentacao.ClienteId == clienteId, cancellationToken);
        }

        private Task<bool> ClientePossuiPagamentosVinculadosAsync(int clienteId, CancellationToken cancellationToken)
        {
            return _context.Pagamentos
                .AnyAsync(pagamento => pagamento.ClienteId == clienteId, cancellationToken);
        }

        private Task<bool> ClientePossuiPagamentosCreditoVinculadosAsync(int clienteId, CancellationToken cancellationToken)
        {
            return _context.PagamentosCredito
                .AnyAsync(pagamento => pagamento.ClienteId == clienteId, cancellationToken);
        }
    }
}
