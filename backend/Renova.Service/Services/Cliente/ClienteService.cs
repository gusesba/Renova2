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
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveisMinhasPecas = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<ProdutoEstoqueModel, int>>)(produto => produto.Id),
            ["loja"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Loja != null ? produto.Loja.Nome : string.Empty),
            ["produto"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Produto != null ? produto.Produto.Valor : string.Empty),
            ["descricao"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Descricao),
            ["marca"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Marca != null ? produto.Marca.Valor : string.Empty),
            ["tamanho"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Tamanho != null ? produto.Tamanho.Valor : string.Empty),
            ["cor"] = (Expression<Func<ProdutoEstoqueModel, string>>)(produto => produto.Cor != null ? produto.Cor.Valor : string.Empty),
            ["preco"] = (Expression<Func<ProdutoEstoqueModel, decimal>>)(produto => produto.Preco),
            ["entrada"] = (Expression<Func<ProdutoEstoqueModel, DateTime>>)(produto => produto.Entrada),
            ["situacao"] = (Expression<Func<ProdutoEstoqueModel, SituacaoProduto>>)(produto => produto.Situacao)
        };

        private static string? NormalizarObs(string? obs)
        {
            if (string.IsNullOrWhiteSpace(obs))
            {
                return null;
            }

            return obs.Trim();
        }

        public Task<byte[]> ExportClosingAsync(
            ExportarFechamentoClientesQuery request,
            ObterClientesParametros parametros,
            CancellationToken cancellationToken = default)
        {
            return ExportMovementClosingAsync(request, parametros, cancellationToken);
        }

        public async Task<byte[]> ExportProductClosingAsync(
            ExportarFechamentoClientesQuery request,
            ObterClientesParametros parametros,
            CancellationToken cancellationToken = default)
        {
            (LojaModel loja, ConfigLojaModel _, DateTime dataInicialUtc, DateTime dataFinalUtc) = await PrepararExportacaoFechamentoAsync(
                request,
                parametros,
                cancellationToken);

            List<ClienteModel> clientesElegiveis = await _context.Clientes
                .Where(cliente =>
                    cliente.LojaId == request.LojaId!.Value
                    && !cliente.Doacao
                    && _context.ProdutosEstoque.Any(produto =>
                        produto.LojaId == request.LojaId.Value
                        && produto.FornecedorId == cliente.Id
                        && produto.Entrada >= dataInicialUtc
                        && produto.Entrada <= dataFinalUtc))
                .OrderBy(cliente => cliente.Nome)
                .ThenBy(cliente => cliente.Id)
                .ToListAsync(cancellationToken);

            using XLWorkbook workbook = new();
            HashSet<string> nomesAbas = [];

            foreach (ClienteModel cliente in clientesElegiveis)
            {
                List<FechamentoClienteProdutoItem> produtos = await _context.ProdutosEstoque
                    .Where(produto =>
                        produto.LojaId == request.LojaId!.Value
                        && produto.FornecedorId == cliente.Id
                        && produto.Entrada >= dataInicialUtc
                        && produto.Entrada <= dataFinalUtc)
                    .OrderBy(produto => produto.Entrada)
                    .ThenBy(produto => produto.Id)
                    .Select(produto => new FechamentoClienteProdutoItem
                    {
                        Id = produto.Id,
                        Valor = produto.Preco,
                        Produto = produto.Produto != null ? produto.Produto.Valor : string.Empty,
                        Marca = produto.Marca != null ? produto.Marca.Valor : string.Empty,
                        Tamanho = produto.Tamanho != null ? produto.Tamanho.Valor : string.Empty,
                        Cor = produto.Cor != null ? produto.Cor.Valor : string.Empty,
                        Observacao = produto.Descricao,
                        Situacao = produto.Situacao,
                        Consignado = produto.Consignado,
                        DataEntrada = produto.Entrada
                    })
                    .ToListAsync(cancellationToken);

                IXLWorksheet worksheet = workbook.Worksheets.Add(GerarNomeAbaUnico(cliente.Nome, nomesAbas));
                PreencherPlanilhaFechamentoProdutos(
                    worksheet,
                    loja.Nome,
                    cliente,
                    dataInicialUtc,
                    dataFinalUtc,
                    produtos);
            }

            if (workbook.Worksheets.Count == 0)
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Resumo");
                worksheet.Cell(1, 1).Value = "Nenhum cliente elegivel para o fechamento de produtos no periodo informado.";
                worksheet.Cell(2, 1).Value = "Pre-requisito aplicado: cliente com pelo menos um produto cadastrado como fornecedor no periodo.";
                worksheet.Columns().AdjustToContents();
            }

            using MemoryStream stream = new();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportMovementClosingAsync(
            ExportarFechamentoClientesQuery request,
            ObterClientesParametros parametros,
            CancellationToken cancellationToken = default)
        {
            (LojaModel loja, ConfigLojaModel config, DateTime dataInicialUtc, DateTime dataFinalUtc) = await PrepararExportacaoFechamentoAsync(
                request,
                parametros,
                cancellationToken);

            List<ClienteModel> clientesElegiveis = await _context.Clientes
                .Include(cliente => cliente.Credito)
                .Where(cliente =>
                    cliente.LojaId == request.LojaId!.Value
                    && !cliente.Doacao
                    && (
                        _context.MovimentacoesProdutos.Any(item =>
                            item.Movimentacao != null
                            && item.Produto != null
                            && item.Movimentacao.LojaId == request.LojaId.Value
                            && item.Produto.FornecedorId == cliente.Id
                            && item.Movimentacao.Tipo == TipoMovimentacao.Venda
                            && item.Movimentacao.Data >= dataInicialUtc
                            && item.Movimentacao.Data <= dataFinalUtc)
                        || _context.MovimentacoesProdutos.Any(item =>
                            item.Movimentacao != null
                            && item.Produto != null
                            && item.Movimentacao.LojaId == request.LojaId.Value
                            && item.Movimentacao.ClienteId == cliente.Id
                            && item.Movimentacao.Tipo == TipoMovimentacao.Venda
                            && item.Movimentacao.Data >= dataInicialUtc
                            && item.Movimentacao.Data <= dataFinalUtc)))
                .OrderBy(cliente => cliente.Nome)
                .ThenBy(cliente => cliente.Id)
                .ToListAsync(cancellationToken);

            using XLWorkbook workbook = new();
            HashSet<string> nomesAbas = [];

            foreach (ClienteModel cliente in clientesElegiveis)
            {
                List<FechamentoClienteVendaMovimentoItem> vendas = await _context.MovimentacoesProdutos
                    .Where(item =>
                        item.Movimentacao != null
                        && item.Produto != null
                        && item.Produto.LojaId == request.LojaId!.Value
                        && item.Produto.FornecedorId == cliente.Id
                        && item.Movimentacao.Tipo == TipoMovimentacao.Venda
                        && item.Movimentacao.Data >= dataInicialUtc
                        && item.Movimentacao.Data <= dataFinalUtc)
                    .OrderBy(item => item.Movimentacao!.Data)
                    .ThenBy(item => item.ProdutoId)
                    .Select(item => new FechamentoClienteVendaMovimentoItem
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
                        Comprador = item.Movimentacao != null && item.Movimentacao.Cliente != null ? item.Movimentacao.Cliente.Nome : string.Empty,
                        DataEntrada = item.Produto != null ? item.Produto.Entrada : default,
                        DataSaida = item.Movimentacao != null ? item.Movimentacao.Data : default
                    })
                    .ToListAsync(cancellationToken);

                List<FechamentoClienteCompraMovimentoItem> compras = await _context.MovimentacoesProdutos
                    .Where(item =>
                        item.Movimentacao != null
                        && item.Produto != null
                        && item.Movimentacao.LojaId == request.LojaId!.Value
                        && item.Movimentacao.ClienteId == cliente.Id
                        && item.Movimentacao.Tipo == TipoMovimentacao.Venda
                        && item.Movimentacao.Data >= dataInicialUtc
                        && item.Movimentacao.Data <= dataFinalUtc)
                    .OrderBy(item => item.Movimentacao!.Data)
                    .ThenBy(item => item.ProdutoId)
                    .Select(item => new FechamentoClienteCompraMovimentoItem
                    {
                        Id = item.ProdutoId,
                        IdVenda = item.MovimentacaoId,
                        Valor = item.Produto != null ? item.Produto.Preco : 0m,
                        ValorPago = item.Produto != null
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
                        Fornecedor = item.Produto != null && item.Produto.Fornecedor != null ? item.Produto.Fornecedor.Nome : string.Empty,
                        DataEntrada = item.Produto != null ? item.Produto.Entrada : default,
                        DataSaida = item.Movimentacao != null ? item.Movimentacao.Data : default
                    })
                    .ToListAsync(cancellationToken);

                decimal valorTotalVendas = decimal.Round(vendas.Sum(item => item.ValorVenda), 2, MidpointRounding.AwayFromZero);
                decimal valorTotalCompras = decimal.Round(compras.Sum(item => item.ValorPago), 2, MidpointRounding.AwayFromZero);
                decimal valorReceberCreditoPeriodo = decimal.Round(
                    valorTotalVendas * (config.PercentualRepasseVendedorCredito / 100m),
                    2,
                    MidpointRounding.AwayFromZero);
                decimal valorReceberEspeciePeriodo = decimal.Round(
                    valorTotalVendas * (config.PercentualRepasseFornecedor / 100m),
                    2,
                    MidpointRounding.AwayFromZero);
                decimal saldoContaCreditoAtual = decimal.Round(cliente.Credito?.Valor ?? 0m, 2, MidpointRounding.AwayFromZero);
                decimal saldoFinalCredito = decimal.Round(saldoContaCreditoAtual + valorReceberCreditoPeriodo, 2, MidpointRounding.AwayFromZero);
                decimal valorFinalCredito = saldoFinalCredito > 0m ? saldoFinalCredito : 0m;
                decimal valorFinalEspecie = ConverterCreditoEmEspecie(valorFinalCredito, config);
                decimal saldoDevedorFinal = saldoFinalCredito < 0m ? -saldoFinalCredito : 0m;

                IXLWorksheet worksheet = workbook.Worksheets.Add(GerarNomeAbaUnico(cliente.Nome, nomesAbas));
                PreencherPlanilhaFechamentoMovimentacoes(
                    worksheet,
                    loja.Nome,
                    cliente,
                    dataInicialUtc,
                    dataFinalUtc,
                    saldoContaCreditoAtual,
                    valorTotalVendas,
                    valorTotalCompras,
                    valorReceberCreditoPeriodo,
                    valorReceberEspeciePeriodo,
                    valorFinalCredito,
                    valorFinalEspecie,
                    saldoDevedorFinal,
                    vendas,
                    compras);
            }

            if (workbook.Worksheets.Count == 0)
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Resumo");
                worksheet.Cell(1, 1).Value = "Nenhum cliente elegivel para o fechamento de movimentacoes no periodo informado.";
                worksheet.Cell(2, 1).Value = "Pre-requisito aplicado: cliente com ao menos uma venda dos seus itens ou uma compra realizada no periodo.";
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

            _ = await _context.ObterLojaAcessivelAoUsuarioAsync(cliente.LojaId, parametros.UsuarioId, cancellationToken);
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

            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId.Value, parametros.UsuarioId, cancellationToken);
            await _authorizationService.EnsurePermissionAsync(loja.Id, parametros.UsuarioId, FuncionalidadeCatalogo.ClientesVisualizar, cancellationToken);

            IQueryable<ClienteModel> query = _context.Clientes
                .Where(cliente => cliente.LojaId == loja.Id);

            if (request.Id.HasValue)
            {
                query = query.Where(cliente => cliente.Id == request.Id.Value);
            }

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

        public async Task<PaginacaoDto<ClienteProdutoAreaDto>> GetMyProductsAsync(
            ObterMinhasPecasQuery request,
            int usuarioId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedUserExistsAsync(usuarioId, cancellationToken);

            IQueryable<ProdutoEstoqueModel> query = _context.ProdutosEstoque
                .Where(produto => produto.Fornecedor != null && produto.Fornecedor.UserId == usuarioId);

            IQueryable<ClienteProdutoAreaDto> queryProjetada = BuildClientAreaProductsQuery(
                ApplyMyProductsFilters(query, request),
                request);

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        public async Task<PaginacaoDto<ClienteProdutoAreaDto>> GetMyCustomerProductsAsync(
            ObterMinhasPecasQuery request,
            int usuarioId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedUserExistsAsync(usuarioId, cancellationToken);

            IQueryable<ProdutoEstoqueModel> query = _context.ProdutosEstoque
                .Where(produto => produto.Movimentacoes
                    .Where(item => item.Movimentacao != null)
                    .OrderByDescending(item => item.Movimentacao!.Data)
                    .ThenByDescending(item => item.MovimentacaoId)
                    .Take(1)
                    .Any(item =>
                        item.Movimentacao!.Cliente != null
                        && item.Movimentacao.Cliente.UserId == usuarioId
                        && (item.Movimentacao.Tipo == TipoMovimentacao.Venda
                            || item.Movimentacao.Tipo == TipoMovimentacao.Emprestimo)));

            IQueryable<ClienteProdutoAreaDto> queryProjetada = BuildClientAreaProductsQuery(
                ApplyMyProductsFilters(query, request),
                request);

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        private async Task EnsureAuthenticatedUserExistsAsync(int usuarioId, CancellationToken cancellationToken)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == usuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }
        }

        private IQueryable<ProdutoEstoqueModel> ApplyMyProductsFilters(
            IQueryable<ProdutoEstoqueModel> query,
            ObterMinhasPecasQuery request)
        {
            if (request.Id.HasValue)
            {
                query = query.Where(produto => produto.Id == request.Id.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Loja))
            {
                string lojaFiltro = request.Loja.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Loja != null && produto.Loja.Nome.ToLower().Contains(lojaFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Produto))
            {
                string produtoFiltro = request.Produto.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Produto != null && produto.Produto.Valor.ToLower().Contains(produtoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Descricao))
            {
                string descricaoFiltro = request.Descricao.Trim().ToLowerInvariant();
                query = query.Where(produto => produto.Descricao.ToLower().Contains(descricaoFiltro));
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

            return query;
        }

        private IQueryable<ClienteProdutoAreaDto> BuildClientAreaProductsQuery(
            IQueryable<ProdutoEstoqueModel> query,
            ObterMinhasPecasQuery request)
        {
            return query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveisMinhasPecas, "entrada")
                .ThenByDescending(produto => produto.Id)
                .Select(produto => new ClienteProdutoAreaDto
                {
                    Id = produto.Id,
                    Etiqueta = produto.Etiqueta,
                    Preco = produto.Situacao == SituacaoProduto.Vendido
                        ? (
                            produto.Movimentacoes
                                .Where(item => item.Movimentacao != null && item.Movimentacao.Tipo == TipoMovimentacao.Venda)
                                .OrderByDescending(item => item.Movimentacao!.Data)
                                .ThenByDescending(item => item.MovimentacaoId)
                                .Select(item => (decimal?)decimal.Round(
                                    produto.Preco * ((100m - item.Desconto) / 100m),
                                    2,
                                    MidpointRounding.AwayFromZero))
                                .FirstOrDefault() ?? produto.Preco
                        )
                        : produto.Preco,
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
                    LojaNome = produto.Loja != null ? produto.Loja.Nome : string.Empty,
                    Situacao = produto.Situacao,
                    Consignado = produto.Consignado
                });
        }

        private async Task<(LojaModel Loja, ConfigLojaModel Config, DateTime DataInicialUtc, DateTime DataFinalUtc)> PrepararExportacaoFechamentoAsync(
            ExportarFechamentoClientesQuery request,
            ObterClientesParametros parametros,
            CancellationToken cancellationToken)
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

            await _authorizationService.EnsurePermissionAsync(
                request.LojaId.Value,
                parametros.UsuarioId,
                FuncionalidadeCatalogo.ClientesExportarFechamento,
                cancellationToken);

            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId.Value, parametros.UsuarioId, cancellationToken);
            ConfigLojaModel config = await _context.ConfiguracoesLoja
                .SingleOrDefaultAsync(item => item.LojaId == request.LojaId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Loja nao possui configuracao de repasse ao fornecedor.");

            return (loja, config, dataInicialUtc, dataFinalUtc);
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

        private static void PreencherPlanilhaFechamentoProdutos(
            IXLWorksheet worksheet,
            string nomeLoja,
            ClienteModel cliente,
            DateTime dataInicialUtc,
            DateTime dataFinalUtc,
            IReadOnlyList<FechamentoClienteProdutoItem> produtos)
        {
            XLColor corTitulo = XLColor.FromHtml("#345C49");
            XLColor corTexto = XLColor.FromHtml("#4B5D52");
            XLColor corBorda = XLColor.FromHtml("#D8D2C5");
            XLColor corFundo = XLColor.FromHtml("#FAF7F2");
            XLColor corCardClaro = XLColor.FromHtml("#F4EBDD");
            XLColor corCardDestaque = XLColor.FromHtml("#E7D2B6");
            XLColor corCabecalhoTabela = XLColor.FromHtml("#E9DED0");
            XLColor corLinhaAlternada = XLColor.FromHtml("#FDFBF7");
            XLColor corLinhaVazia = XLColor.FromHtml("#F6F1E8");

            worksheet.Style.Font.FontName = "Aptos";
            worksheet.Style.Font.FontSize = 11;
            worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Style.Fill.BackgroundColor = corFundo;
            worksheet.ShowGridLines = false;
            worksheet.RowHeight = 22;

            worksheet.Range("A1:J1").Merge();
            worksheet.Cell("A1").Value = nomeLoja.ToUpperInvariant();
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 15;
            worksheet.Cell("A1").Style.Font.FontColor = corTitulo;
            worksheet.Cell("A1").Style.Fill.BackgroundColor = corFundo;
            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(1).Height = 24;

            worksheet.Range("A4:J4").Merge();
            worksheet.Cell("A4").Value = $"FECHAMENTO DE PRODUTOS DE {dataInicialUtc:MMMM/yyyy}".ToUpperInvariant();
            worksheet.Cell("A4").Style.Font.Bold = true;
            worksheet.Cell("A4").Style.Font.FontSize = 20;
            worksheet.Cell("A4").Style.Font.FontColor = corTitulo;
            worksheet.Cell("A4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(4).Height = 28;

            worksheet.Range("A5:J5").Merge();
            worksheet.Cell("A5").Value = "Lista de produtos cadastrados no periodo com o cliente como fornecedor.";
            worksheet.Cell("A5").Style.Font.FontColor = corTexto;
            worksheet.Cell("A5").Style.Font.Italic = true;
            worksheet.Cell("A5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            AplicarCardTexto(worksheet, 7, 1, 3, 2, "Dados do Cliente", $"Nome: {cliente.Nome}", corCardClaro, corTitulo, corBorda);
            AplicarCardTexto(
                worksheet,
                7,
                4,
                3,
                2,
                "Periodo",
                $"{dataInicialUtc:dd/MM/yyyy} a {dataFinalUtc:dd/MM/yyyy}",
                corCardClaro,
                corTitulo,
                corBorda);
            AplicarCardTexto(
                worksheet,
                7,
                7,
                3,
                2,
                "Cliente ID",
                cliente.Id.ToString(),
                corCardClaro,
                corTitulo,
                corBorda);
            AplicarCardTexto(
                worksheet,
                7,
                10,
                3,
                1,
                "Pecas no periodo",
                produtos.Count.ToString(),
                corCardDestaque,
                corTitulo,
                corBorda);

            int linhaTabela = 12;
            worksheet.Range(linhaTabela, 1, linhaTabela, 10).Merge();
            worksheet.Cell(linhaTabela, 1).Value = "Pecas cadastradas no periodo";
            AplicarTituloSecao(worksheet.Range(linhaTabela, 1, linhaTabela, 10), corTitulo);

            int cabecalho = linhaTabela + 1;
            string[] colunas = ["ID", "Preco", "Produto", "Marca", "Tamanho", "Cor", "Descricao", "Data de Entrada", "Situacao", "Consignado"];

            for (int indice = 0; indice < colunas.Length; indice++)
            {
                AplicarCabecalhoTabela(worksheet.Cell(cabecalho, indice + 1), colunas[indice], corCabecalhoTabela, corTitulo, corBorda);
            }

            int linhaAtual = cabecalho + 1;
            foreach (FechamentoClienteProdutoItem produto in produtos)
            {
                worksheet.Cell(linhaAtual, 1).Value = produto.Id;
                worksheet.Cell(linhaAtual, 2).Value = produto.Valor;
                worksheet.Cell(linhaAtual, 3).Value = produto.Produto;
                worksheet.Cell(linhaAtual, 4).Value = produto.Marca;
                worksheet.Cell(linhaAtual, 5).Value = produto.Tamanho;
                worksheet.Cell(linhaAtual, 6).Value = produto.Cor;
                worksheet.Cell(linhaAtual, 7).Value = produto.Observacao;
                worksheet.Cell(linhaAtual, 8).Value = produto.DataEntrada;
                worksheet.Cell(linhaAtual, 9).Value = produto.Situacao.ToString();
                worksheet.Cell(linhaAtual, 10).Value = produto.Consignado ? "Sim" : "Nao";
                AplicarLinhaTabela(worksheet.Range(linhaAtual, 1, linhaAtual, 10), linhaAtual % 2 == 0 ? corLinhaAlternada : XLColor.White, corBorda);
                linhaAtual++;
            }

            if (produtos.Count == 0)
            {
                worksheet.Range(linhaAtual, 1, linhaAtual, 10).Merge();
                worksheet.Cell(linhaAtual, 1).Value = "Nenhum produto cadastrado no periodo.";
                AplicarLinhaVazia(worksheet.Range(linhaAtual, 1, linhaAtual, 10), corLinhaVazia, corTexto, corBorda);
            }
            else
            {
                worksheet.Range(cabecalho + 1, 2, linhaAtual - 1, 2).Style.NumberFormat.Format = "R$ #,##0.00";
                worksheet.Range(cabecalho + 1, 8, linhaAtual - 1, 8).Style.DateFormat.Format = "dd/MM/yyyy";
            }

            worksheet.Columns("A:J").AdjustToContents();
            worksheet.Column("G").Width = Math.Max(26, worksheet.Column("G").Width);
            worksheet.Rows().AdjustToContents();
        }

        private static void PreencherPlanilhaFechamentoMovimentacoes(
            IXLWorksheet worksheet,
            string nomeLoja,
            ClienteModel cliente,
            DateTime dataInicialUtc,
            DateTime dataFinalUtc,
            decimal saldoContaCreditoAtual,
            decimal valorTotalVendas,
            decimal valorTotalCompras,
            decimal valorReceberCreditoPeriodo,
            decimal valorReceberEspeciePeriodo,
            decimal valorFinalCredito,
            decimal valorFinalEspecie,
            decimal saldoDevedorFinal,
            IReadOnlyList<FechamentoClienteVendaMovimentoItem> vendas,
            IReadOnlyList<FechamentoClienteCompraMovimentoItem> compras)
        {
            XLColor corTitulo = XLColor.FromHtml("#345C49");
            XLColor corTexto = XLColor.FromHtml("#4B5D52");
            XLColor corBorda = XLColor.FromHtml("#D8D2C5");
            XLColor corFundo = XLColor.FromHtml("#FAF7F2");
            XLColor corCardClaro = XLColor.FromHtml("#F4EBDD");
            XLColor corCardDestaque = XLColor.FromHtml("#E7D2B6");
            XLColor corCardCredito = XLColor.FromHtml("#E4EFE7");
            XLColor corCabecalhoTabela = XLColor.FromHtml("#E9DED0");
            XLColor corLinhaAlternada = XLColor.FromHtml("#FDFBF7");
            XLColor corLinhaVazia = XLColor.FromHtml("#F6F1E8");

            worksheet.Style.Font.FontName = "Aptos";
            worksheet.Style.Font.FontSize = 11;
            worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Style.Fill.BackgroundColor = corFundo;
            worksheet.ShowGridLines = false;
            worksheet.RowHeight = 22;

            worksheet.Range("A1:M1").Merge();
            worksheet.Cell("A1").Value = nomeLoja.ToUpperInvariant();
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 15;
            worksheet.Cell("A1").Style.Font.FontColor = corTitulo;
            worksheet.Cell("A1").Style.Fill.BackgroundColor = corFundo;
            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(1).Height = 24;

            worksheet.Range("A4:M4").Merge();
            worksheet.Cell("A4").Value = $"FECHAMENTO DE MOVIMENTACOES DE {dataInicialUtc:MMMM/yyyy}".ToUpperInvariant();
            worksheet.Cell("A4").Style.Font.Bold = true;
            worksheet.Cell("A4").Style.Font.FontSize = 20;
            worksheet.Cell("A4").Style.Font.FontColor = corTitulo;
            worksheet.Cell("A4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(4).Height = 28;

            worksheet.Range("A5:M5").Merge();
            worksheet.Cell("A5").Value = "Vendas dos itens do cliente, compras realizadas no periodo e resumo da conta credito.";
            worksheet.Cell("A5").Style.Font.FontColor = corTexto;
            worksheet.Cell("A5").Style.Font.Italic = true;
            worksheet.Cell("A5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            AplicarCardTexto(worksheet, 7, 1, 3, 2, "Dados do Cliente", $"Nome: {cliente.Nome}", corCardClaro, corTitulo, corBorda);
            AplicarCardTexto(
                worksheet,
                7,
                4,
                3,
                2,
                "Periodo",
                $"{dataInicialUtc:dd/MM/yyyy} a {dataFinalUtc:dd/MM/yyyy}",
                corCardClaro,
                corTitulo,
                corBorda);
            AplicarCardMoeda(worksheet, 7, 7, 3, 2, "Conta credito atual", saldoContaCreditoAtual, corCardDestaque, corTitulo, corBorda);
            AplicarCardMoeda(worksheet, 7, 10, 3, 2, "A receber no periodo (credito)", valorReceberCreditoPeriodo, corCardCredito, corTitulo, corBorda);
            AplicarCardMoeda(worksheet, 7, 13, 3, 1, "A receber no periodo (especie)", valorReceberEspeciePeriodo, corCardCredito, corTitulo, corBorda);

            AplicarCardMoeda(worksheet, 11, 1, 3, 2, "Total de vendas", valorTotalVendas, corCardClaro, corTitulo, corBorda);
            AplicarCardMoeda(worksheet, 11, 4, 3, 2, "Total de compras", valorTotalCompras, corCardClaro, corTitulo, corBorda);
            AplicarCardMoeda(worksheet, 11, 7, 3, 2, "Saldo final em credito", valorFinalCredito, corCardCredito, corTitulo, corBorda);
            AplicarCardMoeda(worksheet, 11, 10, 3, 2, "Saldo final em especie", valorFinalEspecie, corCardCredito, corTitulo, corBorda);
            AplicarCardTexto(
                worksheet,
                11,
                13,
                3,
                1,
                saldoDevedorFinal > 0 ? "Saldo devedor" : "Status",
                saldoDevedorFinal > 0 ? $"R$ {saldoDevedorFinal:0.00}" : "Fechamento positivo",
                corCardDestaque,
                corTitulo,
                corBorda);

            int linhaVendas = 16;
            worksheet.Range(linhaVendas, 1, linhaVendas, 13).Merge();
            worksheet.Cell(linhaVendas, 1).Value = "Vendas dos itens do cliente";
            AplicarTituloSecao(worksheet.Range(linhaVendas, 1, linhaVendas, 13), corTitulo);

            int cabecalhoVendas = linhaVendas + 1;
            string[] colunasVenda = ["ID", "ID da Venda", "Preco", "Preco de Venda", "Produto", "Marca", "Tamanho", "Cor", "Descricao", "Data de Entrada", "Data de Saida", "Comprador", "Fornecedor"];

            for (int indice = 0; indice < colunasVenda.Length; indice++)
            {
                AplicarCabecalhoTabela(worksheet.Cell(cabecalhoVendas, indice + 1), colunasVenda[indice], corCabecalhoTabela, corTitulo, corBorda);
            }

            int linhaAtual = cabecalhoVendas + 1;
            foreach (FechamentoClienteVendaMovimentoItem venda in vendas)
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
                worksheet.Cell(linhaAtual, 12).Value = venda.Comprador;
                worksheet.Cell(linhaAtual, 13).Value = cliente.Nome;
                AplicarLinhaTabela(worksheet.Range(linhaAtual, 1, linhaAtual, 13), linhaAtual % 2 == 0 ? corLinhaAlternada : XLColor.White, corBorda);
                linhaAtual++;
            }

            if (vendas.Count == 0)
            {
                worksheet.Range(linhaAtual, 1, linhaAtual, 13).Merge();
                worksheet.Cell(linhaAtual, 1).Value = "Nenhuma venda de item do cliente no periodo.";
                AplicarLinhaVazia(worksheet.Range(linhaAtual, 1, linhaAtual, 13), corLinhaVazia, corTexto, corBorda);
                linhaAtual++;
            }
            else
            {
                worksheet.Range(cabecalhoVendas + 1, 3, linhaAtual - 1, 4).Style.NumberFormat.Format = "R$ #,##0.00";
                worksheet.Range(cabecalhoVendas + 1, 10, linhaAtual - 1, 11).Style.DateFormat.Format = "dd/MM/yyyy";
            }

            linhaAtual += 2;
            worksheet.Range(linhaAtual, 1, linhaAtual, 13).Merge();
            worksheet.Cell(linhaAtual, 1).Value = "Compras realizadas pelo cliente";
            AplicarTituloSecao(worksheet.Range(linhaAtual, 1, linhaAtual, 13), corTitulo);

            int cabecalhoCompras = linhaAtual + 1;
            string[] colunasCompra = ["ID", "ID da Venda", "Preco", "Preco de Venda", "Produto", "Marca", "Tamanho", "Cor", "Descricao", "Data de Entrada", "Data de Saida", "Fornecedor", "Comprador"];

            for (int indice = 0; indice < colunasCompra.Length; indice++)
            {
                AplicarCabecalhoTabela(worksheet.Cell(cabecalhoCompras, indice + 1), colunasCompra[indice], corCabecalhoTabela, corTitulo, corBorda);
            }

            linhaAtual = cabecalhoCompras + 1;
            foreach (FechamentoClienteCompraMovimentoItem compra in compras)
            {
                worksheet.Cell(linhaAtual, 1).Value = compra.Id;
                worksheet.Cell(linhaAtual, 2).Value = compra.IdVenda;
                worksheet.Cell(linhaAtual, 3).Value = compra.Valor;
                worksheet.Cell(linhaAtual, 4).Value = compra.ValorPago;
                worksheet.Cell(linhaAtual, 5).Value = compra.Produto;
                worksheet.Cell(linhaAtual, 6).Value = compra.Marca;
                worksheet.Cell(linhaAtual, 7).Value = compra.Tamanho;
                worksheet.Cell(linhaAtual, 8).Value = compra.Cor;
                worksheet.Cell(linhaAtual, 9).Value = compra.Descricao;
                worksheet.Cell(linhaAtual, 10).Value = compra.DataEntrada;
                worksheet.Cell(linhaAtual, 11).Value = compra.DataSaida;
                worksheet.Cell(linhaAtual, 12).Value = compra.Fornecedor;
                worksheet.Cell(linhaAtual, 13).Value = cliente.Nome;
                AplicarLinhaTabela(worksheet.Range(linhaAtual, 1, linhaAtual, 13), linhaAtual % 2 == 0 ? corLinhaAlternada : XLColor.White, corBorda);
                linhaAtual++;
            }

            if (compras.Count == 0)
            {
                worksheet.Range(linhaAtual, 1, linhaAtual, 13).Merge();
                worksheet.Cell(linhaAtual, 1).Value = "Nenhuma compra realizada pelo cliente no periodo.";
                AplicarLinhaVazia(worksheet.Range(linhaAtual, 1, linhaAtual, 13), corLinhaVazia, corTexto, corBorda);
            }
            else
            {
                worksheet.Range(cabecalhoCompras + 1, 3, linhaAtual - 1, 4).Style.NumberFormat.Format = "R$ #,##0.00";
                worksheet.Range(cabecalhoCompras + 1, 10, linhaAtual - 1, 11).Style.DateFormat.Format = "dd/MM/yyyy";
            }

            worksheet.Columns("A:M").AdjustToContents();
            worksheet.Column("I").Width = Math.Max(26, worksheet.Column("I").Width);
            worksheet.Column("L").Width = Math.Max(18, worksheet.Column("L").Width);
            worksheet.Column("M").Width = Math.Max(18, worksheet.Column("M").Width);
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

        private static decimal ConverterCreditoEmEspecie(decimal valorCredito, ConfigLojaModel config)
        {
            if (valorCredito <= 0m || config.PercentualRepasseVendedorCredito <= 0m)
            {
                return 0m;
            }

            return decimal.Round(
                valorCredito * config.PercentualRepasseFornecedor / config.PercentualRepasseVendedorCredito,
                2,
                MidpointRounding.AwayFromZero);
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

        private sealed class FechamentoClienteProdutoItem
        {
            public int Id { get; set; }

            public decimal Valor { get; set; }

            public required string Produto { get; set; }

            public required string Marca { get; set; }

            public required string Tamanho { get; set; }

            public required string Cor { get; set; }

            public required string Observacao { get; set; }

            public SituacaoProduto Situacao { get; set; }

            public bool Consignado { get; set; }

            public DateTime DataEntrada { get; set; }
        }

        private sealed class FechamentoClienteVendaMovimentoItem
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

            public required string Comprador { get; set; }

            public DateTime DataEntrada { get; set; }

            public DateTime DataSaida { get; set; }
        }

        private sealed class FechamentoClienteCompraMovimentoItem
        {
            public int Id { get; set; }

            public int IdVenda { get; set; }

            public decimal Valor { get; set; }

            public decimal ValorPago { get; set; }

            public required string Produto { get; set; }

            public required string Marca { get; set; }

            public required string Tamanho { get; set; }

            public required string Cor { get; set; }

            public required string Descricao { get; set; }

            public required string Fornecedor { get; set; }

            public DateTime DataEntrada { get; set; }

            public DateTime DataSaida { get; set; }
        }

        private static System.Linq.Expressions.Expression<Func<ProdutoEstoqueModel, ProdutoBuscaDto>> MapearProdutoBuscaDto()
        {
            return produto => new ProdutoBuscaDto
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
