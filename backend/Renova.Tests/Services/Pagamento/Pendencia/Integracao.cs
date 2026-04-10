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

namespace Renova.Tests.Services.Pagamento.Pendencia
{
    public class Integracao
    {
        [Fact]
        public async Task PostAtualizarPendenciasDeveConverterPagamentosPendentesEmCredito()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "pendencias-api@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990001");
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(factory, loja.Id, fornecedor.Id);
            _ = await CriarPagamentoAsync(
                factory,
                movimentacao.Id,
                loja.Id,
                fornecedor.Id,
                90m,
                NaturezaPagamento.Receber,
                new DateTime(2026, 4, 10, 15, 0, 0, DateTimeKind.Utc));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/pagamento/pendencia/atualizar", new AtualizarPendenciasCommand
            {
                LojaId = loja.Id,
                Data = new DateTime(2026, 4, 10)
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            AtualizarPendenciasDto? body = await response.Content.ReadFromJsonAsync<AtualizarPendenciasDto>();
            Assert.NotNull(body);
            Assert.Equal(1, body.QuantidadeOrdensAtualizadas);
            Assert.Equal(-90m, body.ValorTotalCredito);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            Assert.Equal(-90m, context.ClientesCreditos.Single(item => item.ClienteId == fornecedor.Id).Valor);
            Assert.Equal(StatusPagamento.Pago, context.Pagamentos.Single().Status);
            Assert.Empty(context.PagamentosCredito);
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

        private static async Task<MovimentacaoModel> CriarMovimentacaoAsync(RenovaApiFactory factory, int lojaId, int clienteId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            MovimentacaoModel movimentacao = new()
            {
                LojaId = lojaId,
                ClienteId = clienteId,
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc)
            };

            _ = context.Movimentacoes.Add(movimentacao);
            _ = await context.SaveChangesAsync();
            return movimentacao;
        }

        private static async Task<PagamentoModel> CriarPagamentoAsync(
            RenovaApiFactory factory,
            int movimentacaoId,
            int lojaId,
            int clienteId,
            decimal valor,
            NaturezaPagamento natureza,
            DateTime data)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            PagamentoModel pagamento = new()
            {
                MovimentacaoId = movimentacaoId,
                LojaId = lojaId,
                ClienteId = clienteId,
                Natureza = natureza,
                Status = StatusPagamento.Pendente,
                Valor = valor,
                Data = data
            };

            _ = context.Pagamentos.Add(pagamento);
            _ = await context.SaveChangesAsync();
            return pagamento;
        }
    }
}
