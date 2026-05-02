using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Produto;
using Renova.Service.Parameters.Produto;
using Renova.Service.Services.Produto;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.Editar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task EditAsyncDeveEditarProdutoDaLojaDoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            EditarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Vendido, false);
            command.Descricao = "Produto editado";
            command.Preco = 249.90m;
            command.Etiqueta = "12";

            ProdutoService service = new(context);
            ProdutoDto resultado = await service.EditAsync(
                command,
                new EditarProdutoParametros { UsuarioId = loja.UsuarioId, ProdutoId = produto.Id });

            Assert.Equal(produto.Id, resultado.Id);
            Assert.Equal(12, resultado.Etiqueta);
            Assert.Equal(command.Descricao, resultado.Descricao);
            Assert.Equal(command.Preco, resultado.Preco);
            Assert.Equal(SituacaoProduto.Vendido, resultado.Situacao);
            Assert.False(resultado.Consignado);
        }

        [Fact]
        public async Task EditAsyncDeveImpedirEtiquetaDuplicadaNaMesmaLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel primeiroProduto = await CriarProdutoAsync(context, loja.Id, SituacaoProduto.Estoque, true, 4);
            ProdutoEstoqueModel segundoProduto = await CriarProdutoAsync(context, loja.Id, SituacaoProduto.Estoque, true, 9);
            EditarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            command.Etiqueta = primeiroProduto.Etiqueta.ToString();

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.EditAsync(
                command,
                new EditarProdutoParametros { UsuarioId = loja.UsuarioId, ProdutoId = segundoProduto.Id }));
        }

        [Fact]
        public async Task EditAsyncDeveAtualizarSituacaoDoProdutoQuandoPayloadForValido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            EditarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Emprestado, true);

            ProdutoService service = new(context);
            ProdutoDto resultado = await service.EditAsync(
                command,
                new EditarProdutoParametros { UsuarioId = loja.UsuarioId, ProdutoId = produto.Id });

            Assert.Equal(SituacaoProduto.Emprestado, resultado.Situacao);
        }

        [Fact]
        public async Task EditAsyncDeveAtualizarFlagConsignadoQuandoInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            EditarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, false);

            ProdutoService service = new(context);
            ProdutoDto resultado = await service.EditAsync(
                command,
                new EditarProdutoParametros { UsuarioId = loja.UsuarioId, ProdutoId = produto.Id });

            Assert.False(resultado.Consignado);
        }

        [Fact]
        public async Task EditAsyncDeveImpedirEdicaoQuandoFornecedorNaoPertencerALojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            LojaModel outraLoja = await CriarCenarioBaseAsync(context, "joao@renova.com");
            ClienteModel fornecedorOutraLoja = await context.Clientes.SingleAsync(cliente => cliente.LojaId == outraLoja.Id);
            EditarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            command.FornecedorId = fornecedorOutraLoja.Id;

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.EditAsync(
                command,
                new EditarProdutoParametros { UsuarioId = loja.UsuarioId, ProdutoId = produto.Id }));
        }

        [Fact]
        public async Task EditAsyncDeveImpedirEdicaoQuandoTabelaAuxiliarNaoPertencerALojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            LojaModel outraLoja = await CriarCenarioBaseAsync(context, "joao@renova.com");
            ProdutoReferenciaModel produtoOutraLoja = await context.ProdutosReferencia.SingleAsync(item => item.LojaId == outraLoja.Id);
            EditarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            command.ProdutoId = produtoOutraLoja.Id;

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.EditAsync(
                command,
                new EditarProdutoParametros { UsuarioId = loja.UsuarioId, ProdutoId = produto.Id }));
        }

        [Fact]
        public async Task EditAsyncDeveImpedirEdicaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, SituacaoProduto.Estoque, true);
            EditarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, true);

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.EditAsync(
                command,
                new EditarProdutoParametros { UsuarioId = 9999, ProdutoId = produto.Id }));
        }

        [Fact]
        public async Task EditAsyncDeveFalharQuandoProdutoNaoForEncontrado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            EditarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, true);

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.EditAsync(
                command,
                new EditarProdutoParametros { UsuarioId = loja.UsuarioId, ProdutoId = 999 }));
        }

        private static async Task<EditarProdutoCommand> CriarCommandValidoAsync(RenovaDbContext context, int lojaId, SituacaoProduto situacao, bool consignado)
        {
            ProdutoReferenciaModel produto = await context.ProdutosReferencia.SingleAsync(item => item.LojaId == lojaId);
            MarcaModel marca = await context.Marcas.SingleAsync(item => item.LojaId == lojaId);
            TamanhoModel tamanho = await context.Tamanhos.SingleAsync(item => item.LojaId == lojaId);
            CorModel cor = await context.Cores.SingleAsync(item => item.LojaId == lojaId);
            ClienteModel fornecedor = await context.Clientes.SingleAsync(item => item.LojaId == lojaId);

            return new EditarProdutoCommand
            {
                Preco = 149.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Produto teste editado",
                Entrada = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                Situacao = situacao,
                Consignado = consignado
            };
        }

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(
            RenovaDbContext context,
            int lojaId,
            SituacaoProduto situacao,
            bool consignado,
            int etiqueta = 0)
        {
            ProdutoReferenciaModel produto = await context.ProdutosReferencia.SingleAsync(item => item.LojaId == lojaId);
            MarcaModel marca = await context.Marcas.SingleAsync(item => item.LojaId == lojaId);
            TamanhoModel tamanho = await context.Tamanhos.SingleAsync(item => item.LojaId == lojaId);
            CorModel cor = await context.Cores.SingleAsync(item => item.LojaId == lojaId);
            ClienteModel fornecedor = await context.Clientes.SingleAsync(item => item.LojaId == lojaId);

            ProdutoEstoqueModel entity = new()
            {
                Etiqueta = etiqueta,
                Preco = 99.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Produto original",
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = situacao,
                Consignado = consignado
            };

            _ = context.ProdutosEstoque.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<LojaModel> CriarCenarioBaseAsync(RenovaDbContext context, string emailUsuario)
        {
            LojaModel loja = await CriarLojaAsync(context, $"Loja {emailUsuario}", emailUsuario);
            _ = await CriarProdutoReferenciaAsync(context, loja.Id, "Vestido");
            _ = await CriarMarcaAsync(context, loja.Id, "Farm");
            _ = await CriarTamanhoAsync(context, loja.Id, "M");
            _ = await CriarCorAsync(context, loja.Id, "Azul");
            _ = await CriarClienteAsync(context, loja.Id, "Fornecedor", "44999990000");
            return loja;
        }

        private static async Task<UsuarioModel> CriarUsuarioAsync(RenovaDbContext context, string email)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = email,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();
            return usuario;
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, string nomeLoja, string emailUsuario)
        {
            UsuarioModel usuario = await CriarUsuarioAsync(context, emailUsuario);
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
