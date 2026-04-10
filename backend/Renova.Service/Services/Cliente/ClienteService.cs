using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cliente;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Queries.Cliente;
using System.Linq.Expressions;

namespace Renova.Service.Services.Cliente
{
    public class ClienteService(RenovaDbContext context) : IClienteService
    {
        private readonly RenovaDbContext _context = context;
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<ClienteModel, int>>)(cliente => cliente.Id),
            ["nome"] = (Expression<Func<ClienteModel, string>>)(cliente => cliente.Nome),
            ["contato"] = (Expression<Func<ClienteModel, string>>)(cliente => cliente.Contato)
        };

        public async Task<ClienteDto> CreateAsync(CriarClienteCommand request, CriarClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();
            string contatoNormalizado = request.Contato.KeepOnlyDigits();

            if (contatoNormalizado.Length is not (10 or 11))
            {
                throw new ArgumentException("Contato deve conter 10 ou 11 numeros.", nameof(request));
            }

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == request.LojaId, cancellationToken);

            if (loja is null || loja.UsuarioId != parametros.UsuarioId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

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

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            ClienteModel? cliente = await _context.Clientes
                .SingleOrDefaultAsync(clienteAtual => clienteAtual.Id == parametros.ClienteId, cancellationToken);

            if (cliente is null)
            {
                throw new KeyNotFoundException("Cliente informado nao foi encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == cliente.LojaId, cancellationToken);

            if (loja is null || loja.UsuarioId != parametros.UsuarioId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

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
            cliente.Doacao = request.Doacao;
            cliente.UserId = request.UserId;

            _ = await _context.SaveChangesAsync(cancellationToken);

            return new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Contato = cliente.Contato,
                Doacao = cliente.Doacao,
                LojaId = cliente.LojaId,
                UserId = cliente.UserId,
                UserNome = null,
                UserEmail = null
            };
        }

        public async Task DeleteAsync(ExcluirClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            ClienteModel? cliente = await _context.Clientes
                .SingleOrDefaultAsync(clienteAtual => clienteAtual.Id == parametros.ClienteId, cancellationToken);

            if (cliente is null)
            {
                throw new KeyNotFoundException("Cliente informado nao foi encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == cliente.LojaId, cancellationToken);

            if (loja is null || loja.UsuarioId != parametros.UsuarioId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

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

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == request.LojaId.Value, cancellationToken);

            if (loja is null || loja.UsuarioId != parametros.UsuarioId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

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

            LojaModel loja = await ObterLojaDoUsuarioAsync(request.LojaId.Value, parametros.UsuarioId, cancellationToken);

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

        private async Task<LojaModel> ObterLojaDoUsuarioAsync(int lojaId, int usuarioId, CancellationToken cancellationToken)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == usuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == lojaId, cancellationToken);

            if (loja is null || loja.UsuarioId != usuarioId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

            return loja;
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
