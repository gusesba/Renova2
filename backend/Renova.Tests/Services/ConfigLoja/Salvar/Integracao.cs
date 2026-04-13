using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.ConfigLoja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.ConfigLoja.Salvar
{
    public class Integracao
    {
        [Fact]
        public async Task PutConfigLojaDeveRetornarOkQuandoUsuarioAutenticadoEnviarPercentualValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/config-loja", new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 6,
                FormasPagamento =
                [
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = "Cartao credito", PercentualAjuste = 4.5m },
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = "Pix", PercentualAjuste = -3m }
                ],
                DescontosPermanencia =
                [
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 3, PercentualDesconto = 10m },
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 6, PercentualDesconto = 15m }
                ]
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ConfigLojaDto? body = await response.Content.ReadFromJsonAsync<ConfigLojaDto>();
            Assert.NotNull(body);
            Assert.Equal(loja.Id, body.LojaId);
            Assert.Equal(45m, body.PercentualRepasseFornecedor);
            Assert.Equal(45m, body.PercentualRepasseVendedorCredito);
            Assert.Equal(6, body.TempoPermanenciaProdutoMeses);
            Assert.Collection(body.DescontosPermanencia,
                item =>
                {
                    Assert.Equal(3, item.APartirDeMeses);
                    Assert.Equal(10m, item.PercentualDesconto);
                },
                item =>
                {
                    Assert.Equal(6, item.APartirDeMeses);
                    Assert.Equal(15m, item.PercentualDesconto);
                });
            Assert.Collection(body.FormasPagamento,
                item =>
                {
                    Assert.Equal("Cartao credito", item.Nome);
                    Assert.Equal(4.5m, item.PercentualAjuste);
                },
                item =>
                {
                    Assert.Equal("Pix", item.Nome);
                    Assert.Equal(-3m, item.PercentualAjuste);
                });

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            ConfigLojaModel config = Assert.Single(context.ConfiguracoesLoja
                .Include(item => item.DescontosPermanencia)
                .Include(item => item.FormasPagamento));
            Assert.Equal(loja.Id, config.LojaId);
            Assert.Equal(45m, config.PercentualRepasseFornecedor);
            Assert.Equal(45m, config.PercentualRepasseVendedorCredito);
            Assert.Equal(6, config.TempoPermanenciaProdutoMeses);
            Assert.Collection(config.DescontosPermanencia.OrderBy(item => item.APartirDeMeses),
                item =>
                {
                    Assert.Equal(3, item.APartirDeMeses);
                    Assert.Equal(10m, item.PercentualDesconto);
                },
                item =>
                {
                    Assert.Equal(6, item.APartirDeMeses);
                    Assert.Equal(15m, item.PercentualDesconto);
                });
            Assert.Collection(config.FormasPagamento.OrderBy(item => item.Nome),
                item =>
                {
                    Assert.Equal("Cartao credito", item.Nome);
                    Assert.Equal(4.5m, item.PercentualAjuste);
                },
                item =>
                {
                    Assert.Equal("Pix", item.Nome);
                    Assert.Equal(-3m, item.PercentualAjuste);
                });
        }

        [Fact]
        public async Task PutConfigLojaDeveRetornarBadRequestQuandoPercentualForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/config-loja", new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 150m,
                PercentualRepasseVendedorCredito = 100m,
                TempoPermanenciaProdutoMeses = 6
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutConfigLojaDeveRetornarBadRequestQuandoRepasseEmCreditoForMenorQueRepasseNormal()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-credito@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/config-loja", new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 30m,
                TempoPermanenciaProdutoMeses = 6
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutConfigLojaDeveRetornarBadRequestQuandoTempoPermanenciaProdutoForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-tempo@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/config-loja", new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 0
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutConfigLojaDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/config-loja", new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 6
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutConfigLojaDeveRetornarBadRequestQuandoHouverMesesDuplicadosNosDescontosDePermanencia()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-desconto@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/config-loja", new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 6,
                DescontosPermanencia =
                [
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 3, PercentualDesconto = 10m },
                    new SalvarConfigLojaDescontoPermanenciaCommand { APartirDeMeses = 3, PercentualDesconto = 15m }
                ]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutConfigLojaDeveRetornarBadRequestQuandoHouverFormasPagamentoDuplicadas()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-forma@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/config-loja", new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = 6,
                FormasPagamento =
                [
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = "Pix", PercentualAjuste = 0m },
                    new SalvarConfigLojaFormaPagamentoCommand { Nome = " pix ", PercentualAjuste = 1m }
                ]
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
    }
}
