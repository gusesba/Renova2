using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.GetAuxiliar
{
    public class Integracao
    {
        [Fact]
        public async Task GetMarcaDeveRetornarOkComAuxiliaresDaLojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            LojaModel lojaExterna = await CriarLojaAsync(factory, outroUsuario.Usuario.Id, "Loja Externa");

            _ = await CriarMarcaAsync(factory, loja.Id, "Farm");
            _ = await CriarMarcaAsync(factory, loja.Id, "Animale");
            _ = await CriarMarcaAsync(factory, outraLoja.Id, "Shoulder");
            _ = await CriarMarcaAsync(factory, lojaExterna.Id, "Forum");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto/marca?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoAuxiliarDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoAuxiliarDto>>();

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Collection(body.Itens,
                item => Assert.Equal("Animale", item.Valor),
                item => Assert.Equal("Farm", item.Valor));
        }

        [Fact]
        public async Task GetReferenciaDeveAplicarFiltroOrdenacaoEPaginacao()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido curto");
            _ = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido midi");
            _ = await CriarProdutoReferenciaAsync(factory, loja.Id, "Blazer");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto/referencia?lojaId={loja.Id}&valor=vestido&ordenarPor=valor&direcao=desc&pagina=2&tamanhoPagina=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoAuxiliarDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoAuxiliarDto>>();

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Equal(2, body.TotalPaginas);
            ProdutoAuxiliarDto item = Assert.Single(body.Itens);
            Assert.Equal("Vestido curto", item.Valor);
        }

        [Fact]
        public async Task GetCorDeveRetornarBadRequestQuandoLojaIdNaoForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync("/api/produto/cor");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTamanhoDeveRetornarUnauthorizedQuandoLojaFiltradaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");

            _ = await CriarTamanhoAsync(factory, loja.Id, "M");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto/tamanho?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string email)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", new CadastroCommand
            {
                Nome = "Usuario de Teste",
                Email = email,
                Senha = "Senha@123"
            });

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
    }
}
