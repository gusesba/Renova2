using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Parameters.Pagamento;
using Renova.Service.Services.Pagamento;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Pagamento.Pendencia
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetPendenciasAsyncDeveRetornarApenasClientesComCreditoDiferenteDeZero()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "pendencias-lista@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel clienteComCredito = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001");
            ClienteModel clienteSemCredito = await CriarClienteAsync(context, loja.Id, "Fornecedor B", "44999990002");

            context.ClientesCreditos.AddRange(
                new ClienteCreditoModel
                {
                    LojaId = loja.Id,
                    ClienteId = clienteComCredito.Id,
                    Valor = 80m
                },
                new ClienteCreditoModel
                {
                    LojaId = loja.Id,
                    ClienteId = clienteSemCredito.Id,
                    Valor = 0m
                });
            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);

            IReadOnlyList<ClientePendenciaDto> resultado = await service.GetPendenciasAsync(loja.Id, usuario.Id);

            ClientePendenciaDto item = Assert.Single(resultado);
            Assert.Equal(clienteComCredito.Id, item.ClienteId);
            Assert.Equal(80m, item.Credito);
        }

        [Fact]
        public async Task UpdatePendenciasAsyncDeveConverterPagamentosPendentesEmCreditoEMarcarComoPagoSemGerarPagamentoCredito()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "pendencias-atualizar@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel fornecedorA = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001");
            ClienteModel fornecedorB = await CriarClienteAsync(context, loja.Id, "Fornecedor B", "44999990002");
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(context, loja.Id, fornecedorA.Id);

            _ = context.ClientesCreditos.Add(new ClienteCreditoModel
            {
                LojaId = loja.Id,
                ClienteId = fornecedorA.Id,
                Valor = 10m
            });

            context.Pagamentos.AddRange(
                new PagamentoModel
                {
                    MovimentacaoId = movimentacao.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorA.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pendente,
                    Valor = 50m,
                    Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = movimentacao.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorB.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pendente,
                    Valor = 35m,
                    Data = new DateTime(2026, 4, 10, 18, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = movimentacao.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorA.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pago,
                    Valor = 20m,
                    Data = new DateTime(2026, 4, 8, 10, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = movimentacao.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorA.Id,
                    Natureza = NaturezaPagamento.Receber,
                    Status = StatusPagamento.Pendente,
                    Valor = 70m,
                    Data = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = movimentacao.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorA.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pendente,
                    Valor = 40m,
                    Data = new DateTime(2026, 4, 11, 10, 0, 0, DateTimeKind.Utc)
                });
            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);

            AtualizarPendenciasDto resultado = await service.UpdatePendenciasAsync(
                new AtualizarPendenciasCommand
                {
                    LojaId = loja.Id,
                    Data = new DateTime(2026, 4, 10)
                },
                new AtualizarPendenciasParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(3, resultado.QuantidadeOrdensAtualizadas);
            Assert.Equal(15m, resultado.ValorTotalCredito);
            Assert.Equal(-10m, await ObterCreditoAsync(context, fornecedorA.Id));
            Assert.Equal(35m, await ObterCreditoAsync(context, fornecedorB.Id));
            Assert.Equal(4, await context.Pagamentos.CountAsync(item =>
                item.Status == StatusPagamento.Pago
                && (item.Valor == 20m || item.Valor == 35m || item.Valor == 50m || item.Valor == 70m)));
            Assert.Equal(0, await context.PagamentosCredito.CountAsync());
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

        private static async Task<MovimentacaoModel> CriarMovimentacaoAsync(RenovaDbContext context, int lojaId, int clienteId)
        {
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

        private static async Task<decimal> ObterCreditoAsync(RenovaDbContext context, int clienteId)
        {
            ClienteCreditoModel credito = await context.ClientesCreditos.SingleAsync(item => item.ClienteId == clienteId);
            return credito.Valor;
        }
    }
}
