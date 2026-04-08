using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Services.Cliente;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Excluir
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        // Input: usuario autenticado e cliente existente na loja do usuario
        // Remove o cliente persistido
        // Retorna: conclusao sem erro e cliente ausente na base
        public async Task DeleteAsyncDeveExcluirClienteDaLojaDoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            ExcluirClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            await service.DeleteAsync(parametros);

            Assert.Empty(context.Clientes);
        }

        [Fact]
        // Input: usuario autenticado tentando excluir cliente de loja de outro usuario
        // Nao remove o cliente
        // Retorna: erro de autorizacao
        public async Task DeleteAsyncDeveImpedirExclusaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            UsuarioModel outroUsuario = new()
            {
                Nome = "Joao Souza",
                Email = "joao@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(outroUsuario);
            _ = await context.SaveChangesAsync();

            ExcluirClienteParametros parametros = new()
            {
                UsuarioId = outroUsuario.Id,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(parametros));

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync();
            Assert.Equal(cliente.Id, clienteSalvo.Id);
        }

        [Fact]
        // Input: cliente informado nao existe
        // Nao remove registro algum
        // Retorna: erro de entidade nao encontrada
        public async Task DeleteAsyncDeveFalharQuandoClienteNaoForEncontrado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            ExcluirClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = 999
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(parametros));
        }

        [Fact]
        // Input: cliente referenciado como fornecedor em produto
        // Nao remove o cliente enquanto existir produto relacionado
        // Retorna: mensagem adequada explicando o bloqueio
        public async Task DeleteAsyncDeveImpedirExclusaoQuandoClienteEstiverRelacionadoAProduto()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(context, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(context, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(context, loja.Id, "M");
            CorModel cor = await CriarCorAsync(context, loja.Id, "Azul");

            _ = context.ProdutosEstoque.Add(new ProdutoEstoqueModel
            {
                Preco = 100m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = cliente.Id,
                Descricao = "Produto vinculado",
                Entrada = DateTime.UtcNow,
                LojaId = loja.Id,
                Situacao = SituacaoProduto.Estoque,
                Consignado = false
            });
            _ = await context.SaveChangesAsync();

            ExcluirClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(parametros));

            Assert.Equal("Cliente possui produtos vinculados e nao pode ser excluido.", ex.Message);
            _ = Assert.Single(context.Clientes);
            _ = Assert.Single(context.ProdutosEstoque.Where(produtoAtual => produtoAtual.FornecedorId == cliente.Id));
        }

        [Fact]
        // Input: cliente referenciado em movimentacao
        // Nao remove o cliente enquanto existir movimentacao relacionada
        // Retorna: mensagem adequada explicando o bloqueio
        public async Task DeleteAsyncDeveImpedirExclusaoQuandoClienteEstiverRelacionadoAMovimentacao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            _ = context.Movimentacoes.Add(new MovimentacaoModel
            {
                Tipo = TipoMovimentacao.Venda,
                Data = DateTime.UtcNow,
                LojaId = loja.Id,
                ClienteId = cliente.Id
            });
            _ = await context.SaveChangesAsync();

            ExcluirClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(parametros));

            Assert.Equal("Cliente possui movimentacoes vinculadas e nao pode ser excluido.", ex.Message);
            _ = Assert.Single(context.Clientes);
            _ = Assert.Single(context.Movimentacoes.Where(movimentacao => movimentacao.ClienteId == cliente.Id));
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, string nomeLoja, string emailUsuario)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = emailUsuario,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            LojaModel loja = new()
            {
                Nome = nomeLoja,
                UsuarioId = usuario.Id
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();

            return loja;
        }

        private static async Task<ClienteModel> CriarClienteAsync(RenovaDbContext context, int lojaId, string nome, string contato)
        {
            ClienteModel cliente = new()
            {
                Nome = nome,
                Contato = contato,
                LojaId = lojaId
            };

            _ = context.Clientes.Add(cliente);
            _ = await context.SaveChangesAsync();

            return cliente;
        }

        private static async Task<ProdutoReferenciaModel> CriarProdutoReferenciaAsync(RenovaDbContext context, int lojaId, string valor)
        {
            ProdutoReferenciaModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.ProdutosReferencia.Add(entity);
            _ = await context.SaveChangesAsync();

            return entity;
        }

        private static async Task<MarcaModel> CriarMarcaAsync(RenovaDbContext context, int lojaId, string valor)
        {
            MarcaModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Marcas.Add(entity);
            _ = await context.SaveChangesAsync();

            return entity;
        }

        private static async Task<TamanhoModel> CriarTamanhoAsync(RenovaDbContext context, int lojaId, string valor)
        {
            TamanhoModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Tamanhos.Add(entity);
            _ = await context.SaveChangesAsync();

            return entity;
        }

        private static async Task<CorModel> CriarCorAsync(RenovaDbContext context, int lojaId, string valor)
        {
            CorModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Cores.Add(entity);
            _ = await context.SaveChangesAsync();

            return entity;
        }
    }
}
