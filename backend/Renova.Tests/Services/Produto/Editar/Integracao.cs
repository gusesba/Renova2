using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Produto;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.Editar
{
    public class Integracao
    {
        [Fact]
        public async Task PutProdutoDeveRetornarOkQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/produto/{produto.Id}", new EditarProdutoCommand
            {
                Preco = 249.90m,
                ProdutoId = produtoRef.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Vestido azul editado",
                Entrada = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                Situacao = SituacaoProduto.Vendido,
                Consignado = false
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ProdutoDto? body = await response.Content.ReadFromJsonAsync<ProdutoDto>();
            Assert.NotNull(body);
            Assert.Equal(produto.Id, body.Id);
            Assert.Equal("Vestido azul editado", body.Descricao);
            Assert.Equal(SituacaoProduto.Vendido, body.Situacao);
            Assert.False(body.Consignado);
        }

        [Fact]
        public async Task PutProdutoDeveRetornarOkQuandoAtualizarSituacaoEConsignado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/produto/{produto.Id}", new EditarProdutoCommand
            {
                Preco = 199.90m,
                ProdutoId = produtoRef.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Vestido azul midi",
                Entrada = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                Situacao = SituacaoProduto.Emprestado,
                Consignado = true
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ProdutoDto? body = await response.Content.ReadFromJsonAsync<ProdutoDto>();
            Assert.NotNull(body);
            Assert.Equal(SituacaoProduto.Emprestado, body.Situacao);
            Assert.True(body.Consignado);
        }

        [Fact]
        public async Task PutProdutoDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/produto/{produto.Id}", new EditarProdutoCommand
            {
                Preco = 249.90m,
                ProdutoId = produtoRef.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Produto editado",
                Entrada = DateTime.UtcNow,
                Situacao = SituacaoProduto.Vendido
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutProdutoDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/produto/{produto.Id}", new EditarProdutoCommand
            {
                Preco = 249.90m,
                ProdutoId = produtoRef.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Produto editado",
                Entrada = DateTime.UtcNow,
                Situacao = SituacaoProduto.Vendido
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutProdutoDeveRetornarBadRequestQuandoFornecedorNaoPertencerALojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ClienteModel fornecedorOutraLoja = await CriarClienteAsync(factory, outraLoja.Id, "Fornecedor B", "44888880000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/produto/{produto.Id}", new EditarProdutoCommand
            {
                Preco = 249.90m,
                ProdutoId = produtoRef.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedorOutraLoja.Id,
                Descricao = "Produto editado",
                Entrada = DateTime.UtcNow,
                Situacao = SituacaoProduto.Vendido
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutProdutoDeveRetornarBadRequestQuandoTabelaAuxiliarNaoPertencerALojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            ProdutoReferenciaModel produtoOutraLoja = await CriarProdutoReferenciaAsync(factory, outraLoja.Id, "Blazer");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/produto/{produto.Id}", new EditarProdutoCommand
            {
                Preco = 249.90m,
                ProdutoId = produtoOutraLoja.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Produto editado",
                Entrada = DateTime.UtcNow,
                Situacao = SituacaoProduto.Vendido
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutProdutoDeveRetornarNotFoundQuandoProdutoNaoForEncontrado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/produto/999", new EditarProdutoCommand
            {
                Preco = 249.90m,
                ProdutoId = produtoRef.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Produto editado",
                Entrada = DateTime.UtcNow,
                Situacao = SituacaoProduto.Vendido
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string email)
        {
            CadastroCommand command = new()
            {
                Nome = "Usuario de Teste",
                Email = email,
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", command);
            _ = response.EnsureSuccessStatusCode();
            UsuarioTokenDto? resultado = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();
            return Assert.IsType<UsuarioTokenDto>(resultado);
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaApiFactory factory, int usuarioId, string nome)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            LojaModel loja = new()
            {
                Nome = nome,
                UsuarioId = usuarioId
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();
            return loja;
        }

        private static async Task<ClienteModel> CriarClienteAsync(RenovaApiFactory factory, int lojaId, string nome, string contato)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
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

        private static async Task<ProdutoReferenciaModel> CriarProdutoReferenciaAsync(RenovaApiFactory factory, int lojaId, string valor)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            ProdutoReferenciaModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.ProdutosReferencia.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<MarcaModel> CriarMarcaAsync(RenovaApiFactory factory, int lojaId, string valor)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            MarcaModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Marcas.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<TamanhoModel> CriarTamanhoAsync(RenovaApiFactory factory, int lojaId, string valor)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            TamanhoModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Tamanhos.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<CorModel> CriarCorAsync(RenovaApiFactory factory, int lojaId, string valor)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            CorModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Cores.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaApiFactory factory, int lojaId, int produtoId, int marcaId, int tamanhoId, int corId, int fornecedorId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            ProdutoEstoqueModel entity = new()
            {
                Preco = 199.90m,
                ProdutoId = produtoId,
                MarcaId = marcaId,
                TamanhoId = tamanhoId,
                CorId = corId,
                FornecedorId = fornecedorId,
                Descricao = "Vestido azul midi",
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = true
            };

            _ = context.ProdutosEstoque.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }
    }
}
