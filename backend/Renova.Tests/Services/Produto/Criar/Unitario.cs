using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Produto;
using Renova.Service.Parameters.Produto;
using Renova.Service.Services.Produto;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.Criar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task CreateAsyncDeveCriarProdutoQuandoPayloadForValido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            CriarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, true);

            ProdutoService service = new(context);
            ProdutoDto resultado = await service.CreateAsync(command, new CriarProdutoParametros { UsuarioId = loja.UsuarioId });

            Assert.True(resultado.Id > 0);
            Assert.Equal(command.Preco, resultado.Preco);
            Assert.Equal(command.ProdutoId, resultado.ProdutoId);
            Assert.Equal(command.MarcaId, resultado.MarcaId);
            Assert.Equal(command.TamanhoId, resultado.TamanhoId);
            Assert.Equal(command.CorId, resultado.CorId);
            Assert.Equal(command.FornecedorId, resultado.FornecedorId);
            Assert.Equal(command.Descricao, resultado.Descricao);
            Assert.Equal(command.Entrada, resultado.Entrada);
            Assert.Equal(command.LojaId, resultado.LojaId);
            Assert.Equal(command.Situacao, resultado.Situacao);
            Assert.True(resultado.Consignado);

            ProdutoEstoqueModel produtoSalvo = await context.ProdutosEstoque.SingleAsync();
            Assert.Equal(resultado.Id, produtoSalvo.Id);
        }

        [Fact]
        public async Task CreateAsyncDeveRetornarSolicitacoesCompativeisQuandoHouverMatch()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoReferenciaModel produto = await context.ProdutosReferencia.SingleAsync(item => item.LojaId == loja.Id);
            MarcaModel marca = await context.Marcas.SingleAsync(item => item.LojaId == loja.Id);
            TamanhoModel tamanho = await context.Tamanhos.SingleAsync(item => item.LojaId == loja.Id);
            CorModel cor = await context.Cores.SingleAsync(item => item.LojaId == loja.Id);
            ClienteModel cliente = await context.Clientes.SingleAsync(item => item.LojaId == loja.Id);

            _ = context.Solicitacoes.Add(new SolicitacaoModel
            {
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                ClienteId = cliente.Id,
                Descricao = "Procura vestido azul",
                PrecoMinimo = 100m,
                PrecoMaximo = 200m,
                LojaId = loja.Id
            });
            _ = await context.SaveChangesAsync();

            ProdutoService service = new(context);
            ProdutoDto resultado = await service.CreateAsync(
                await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, true),
                new CriarProdutoParametros { UsuarioId = loja.UsuarioId });

            SolicitacaoCompativelDto match = Assert.Single(resultado.SolicitacoesCompativeis);
            Assert.Equal(cliente.Nome, match.Cliente);
            Assert.Equal("Procura vestido azul", match.Descricao);
        }

        [Fact]
        public async Task CreateAsyncDevePermitirFornecedorReferenciandoClienteDaMesmaLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ClienteModel fornecedor = await context.Clientes.SingleAsync(cliente => cliente.LojaId == loja.Id);
            CriarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, false);
            command.FornecedorId = fornecedor.Id;

            ProdutoService service = new(context);
            ProdutoDto resultado = await service.CreateAsync(command, new CriarProdutoParametros { UsuarioId = loja.UsuarioId });

            Assert.Equal(fornecedor.Id, resultado.FornecedorId);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirCriacaoQuandoFornecedorNaoPertencerALojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            LojaModel outraLoja = await CriarCenarioBaseAsync(context, "joao@renova.com");
            ClienteModel fornecedorDeOutraLoja = await context.Clientes.SingleAsync(cliente => cliente.LojaId == outraLoja.Id);
            CriarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, false);
            command.FornecedorId = fornecedorDeOutraLoja.Id;

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(command, new CriarProdutoParametros { UsuarioId = loja.UsuarioId }));

            Assert.Empty(context.ProdutosEstoque);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirCriacaoQuandoTabelaAuxiliarNaoPertencerALojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            LojaModel outraLoja = await CriarCenarioBaseAsync(context, "joao@renova.com");
            ProdutoReferenciaModel produtoOutraLoja = await context.ProdutosReferencia.SingleAsync(item => item.LojaId == outraLoja.Id);
            CriarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, false);
            command.ProdutoId = produtoOutraLoja.Id;

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(command, new CriarProdutoParametros { UsuarioId = loja.UsuarioId }));
        }

        [Fact]
        public async Task CreateAsyncDevePersistirSituacaoComValorDoEnumEsperado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoService service = new(context);
            ProdutoDto resultado = await service.CreateAsync(
                await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Emprestado, true),
                new CriarProdutoParametros { UsuarioId = loja.UsuarioId });

            Assert.Equal(SituacaoProduto.Emprestado, resultado.Situacao);
        }

        [Fact]
        public async Task CreateAsyncDevePersistirFlagConsignadoQuandoInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoService service = new(context);
            ProdutoDto resultado = await service.CreateAsync(
                await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, true),
                new CriarProdutoParametros { UsuarioId = loja.UsuarioId });

            Assert.True(resultado.Consignado);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirCriacaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            CriarProdutoCommand command = await CriarCommandValidoAsync(context, loja.Id, SituacaoProduto.Estoque, false);

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(command, new CriarProdutoParametros { UsuarioId = 9999 }));
        }

        [Fact]
        public async Task CreateProdutoAuxiliarDeveImpedirValorDuplicadoNaMesmaLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            _ = await CriarProdutoReferenciaAsync(context, loja.Id, "Vestido");

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProdutoAuxiliarAsync(
                new CriarProdutoAuxiliarCommand { Valor = "Vestido", LojaId = loja.Id },
                new CriarProdutoAuxiliarParametros { UsuarioId = loja.UsuarioId }));
        }

        [Fact]
        public async Task CreateMarcaAuxiliarDeveImpedirValorDuplicadoNaMesmaLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            _ = await CriarMarcaAsync(context, loja.Id, "Farm");

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateMarcaAsync(
                new CriarProdutoAuxiliarCommand { Valor = "Farm", LojaId = loja.Id },
                new CriarProdutoAuxiliarParametros { UsuarioId = loja.UsuarioId }));
        }

        [Fact]
        public async Task CreateTamanhoAuxiliarDeveImpedirValorDuplicadoNaMesmaLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            _ = await CriarTamanhoAsync(context, loja.Id, "M");

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateTamanhoAsync(
                new CriarProdutoAuxiliarCommand { Valor = "M", LojaId = loja.Id },
                new CriarProdutoAuxiliarParametros { UsuarioId = loja.UsuarioId }));
        }

        [Fact]
        public async Task CreateCorAuxiliarDeveImpedirValorDuplicadoNaMesmaLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            _ = await CriarCorAsync(context, loja.Id, "Preto");

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCorAsync(
                new CriarProdutoAuxiliarCommand { Valor = "Preto", LojaId = loja.Id },
                new CriarProdutoAuxiliarParametros { UsuarioId = loja.UsuarioId }));
        }

        [Fact]
        public async Task CreateTabelaAuxiliarDevePermitirMesmoValorEmLojasDiferentes()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel primeiraLoja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            LojaModel segundaLoja = await CriarLojaAsync(context, "Loja Bairro", "joao@renova.com");
            _ = await CriarMarcaAsync(context, primeiraLoja.Id, "Farm");

            ProdutoService service = new(context);
            ProdutoAuxiliarDto resultado = await service.CreateMarcaAsync(
                new CriarProdutoAuxiliarCommand { Valor = "Farm", LojaId = segundaLoja.Id },
                new CriarProdutoAuxiliarParametros { UsuarioId = segundaLoja.UsuarioId });

            Assert.True(resultado.Id > 0);
            Assert.Equal(segundaLoja.Id, resultado.LojaId);
            Assert.Equal(2, await context.Marcas.CountAsync(item => item.Valor == "Farm"));
        }

        private static async Task<CriarProdutoCommand> CriarCommandValidoAsync(RenovaDbContext context, int lojaId, SituacaoProduto situacao, bool consignado)
        {
            ProdutoReferenciaModel produto = await context.ProdutosReferencia.SingleAsync(item => item.LojaId == lojaId);
            MarcaModel marca = await context.Marcas.SingleAsync(item => item.LojaId == lojaId);
            TamanhoModel tamanho = await context.Tamanhos.SingleAsync(item => item.LojaId == lojaId);
            CorModel cor = await context.Cores.SingleAsync(item => item.LojaId == lojaId);
            ClienteModel fornecedor = await context.Clientes.SingleAsync(item => item.LojaId == lojaId);

            return new CriarProdutoCommand
            {
                Preco = 149.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Produto teste",
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = situacao,
                Consignado = consignado
            };
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
