using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Parameters.Pagamento;
using Renova.Service.Services.Pagamento;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Pagamento.Criar
{
    public class ManualUnitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task CreateManualAsyncDeveCriarPagamentoFaturadoSemMovimentacaoEAumentarCreditoQuandoNaturezaForPagar()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "manual-pagar@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001");

            PagamentoService service = new(context);
            PagamentoDto resultado = await service.CreateManualAsync(
                new CriarPagamentoManualCommand
                {
                    LojaId = loja.Id,
                    ClienteId = cliente.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Valor = 75m,
                    Data = new DateTime(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc),
                    Descricao = "Repasse manual"
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(StatusPagamento.Pago, resultado.Status);
            Assert.Null(resultado.MovimentacaoId);
            Assert.Equal("Repasse manual", resultado.Descricao);
            Assert.Equal(75m, resultado.Valor);

            PagamentoModel pagamento = await context.Pagamentos.SingleAsync();
            Assert.Null(pagamento.MovimentacaoId);
            Assert.Equal("Repasse manual", pagamento.Descricao);
            Assert.Equal(75m, await ObterCreditoAsync(context, cliente.Id));
        }

        [Fact]
        public async Task CreateManualAsyncDeveDebitarCreditoQuandoNaturezaForReceber()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "manual-receber@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            _ = context.ClientesCreditos.Add(new ClienteCreditoModel
            {
                LojaId = loja.Id,
                ClienteId = cliente.Id,
                Valor = 120m
            });
            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);
            _ = await service.CreateManualAsync(
                new CriarPagamentoManualCommand
                {
                    LojaId = loja.Id,
                    ClienteId = cliente.Id,
                    Natureza = NaturezaPagamento.Receber,
                    Valor = 20m,
                    Data = new DateTime(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc)
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(100m, await ObterCreditoAsync(context, cliente.Id));
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

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, int usuarioId, string nome)
        {
            LojaModel loja = new()
            {
                Nome = nome,
                UsuarioId = usuarioId
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

        private static async Task<decimal> ObterCreditoAsync(RenovaDbContext context, int clienteId)
        {
            ClienteCreditoModel credito = await context.ClientesCreditos.SingleAsync(item => item.ClienteId == clienteId);
            return credito.Valor;
        }
    }
}
