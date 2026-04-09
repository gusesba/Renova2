using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Pagamento;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Pagamento.CriarCredito
{
    public class Integracao
    {
        [Fact]
        public async Task PostPagamentoCreditoDeveRetornarCreatedQuandoAdicionarCreditoForValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-pagamento-credito@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/pagamento/credito", new CriarPagamentoCreditoCommand
            {
                LojaId = loja.Id,
                ClienteId = cliente.Id,
                Tipo = TipoPagamentoCredito.AdicionarCredito,
                ValorCredito = 150m,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc)
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            PagamentoCreditoDto? body = await response.Content.ReadFromJsonAsync<PagamentoCreditoDto>();
            Assert.NotNull(body);
            Assert.Equal(150m, body.ValorCredito);
            Assert.Equal(150m, body.ValorDinheiro);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            Assert.Single(context.PagamentosCredito);
            Assert.Equal(150m, context.ClientesCreditos.Single(item => item.ClienteId == cliente.Id).Valor);
        }

        [Fact]
        public async Task PostPagamentoCreditoDeveRetornarConflictQuandoResgateNaoPossuirSaldoSuficiente()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-pagamento-resgate@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990001");
            _ = await CriarConfigLojaAsync(factory, loja.Id, 45m, 60m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/pagamento/credito", new CriarPagamentoCreditoCommand
            {
                LojaId = loja.Id,
                ClienteId = fornecedor.Id,
                Tipo = TipoPagamentoCredito.ResgatarCredito,
                ValorCredito = 120m,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc)
            });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("credito positivo", body, StringComparison.OrdinalIgnoreCase);
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

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(
            RenovaApiFactory factory,
            int lojaId,
            decimal percentualRepasseFornecedor,
            decimal percentualRepasseVendedorCredito)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ConfigLojaModel config = new()
            {
                LojaId = lojaId,
                PercentualRepasseFornecedor = percentualRepasseFornecedor,
                PercentualRepasseVendedorCredito = percentualRepasseVendedorCredito,
                TempoPermanenciaProdutoMeses = 6
            };

            _ = context.ConfiguracoesLoja.Add(config);
            _ = await context.SaveChangesAsync();
            return config;
        }
    }
}
