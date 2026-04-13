using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Parameters.Pagamento;
using Renova.Service.Services.Pagamento;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Pagamento.CriarCredito
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task CreateCreditoAsyncDeveAdicionarCreditoComConversaoUmParaUm()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ConfigLojaFormaPagamentoModel formaPagamento = await CriarFormaPagamentoAsync(context, loja.Id, "Cartao credito", 4m);

            PagamentoService service = new(context);
            PagamentoCreditoDto resultado = await service.CreateCreditoAsync(
                new CriarPagamentoCreditoCommand
                {
                    LojaId = loja.Id,
                    ClienteId = cliente.Id,
                    Tipo = TipoPagamentoCredito.AdicionarCredito,
                    ConfigLojaFormaPagamentoId = formaPagamento.Id,
                    ValorCredito = 150m,
                    Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc)
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(TipoPagamentoCredito.AdicionarCredito, resultado.Tipo);
            Assert.Equal(formaPagamento.Id, resultado.ConfigLojaFormaPagamentoId);
            Assert.Equal("Cartao credito", resultado.FormaPagamentoNome);
            Assert.Equal(150m, resultado.ValorCredito);
            Assert.Equal(156m, resultado.ValorDinheiro);
            Assert.Equal(150m, await ObterCreditoAsync(context, cliente.Id));

            PagamentoCreditoModel pagamento = await context.PagamentosCredito.SingleAsync();
            Assert.Equal(formaPagamento.Id, pagamento.ConfigLojaFormaPagamentoId);
            Assert.Equal(150m, pagamento.ValorCredito);
            Assert.Equal(156m, pagamento.ValorDinheiro);
        }

        [Fact]
        public async Task CreateCreditoAsyncDeveResgatarCreditoComBaseNosPercentuaisDeRepasse()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel fornecedor = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001");
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m, 60m);
            _ = context.ClientesCreditos.Add(new ClienteCreditoModel
            {
                LojaId = loja.Id,
                ClienteId = fornecedor.Id,
                Valor = 300m
            });
            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);
            PagamentoCreditoDto resultado = await service.CreateCreditoAsync(
                new CriarPagamentoCreditoCommand
                {
                    LojaId = loja.Id,
                    ClienteId = fornecedor.Id,
                    Tipo = TipoPagamentoCredito.ResgatarCredito,
                    ValorCredito = 120m,
                    Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc)
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(TipoPagamentoCredito.ResgatarCredito, resultado.Tipo);
            Assert.Equal(120m, resultado.ValorCredito);
            Assert.Equal(90m, resultado.ValorDinheiro);
            Assert.Equal(180m, await ObterCreditoAsync(context, fornecedor.Id));
        }

        [Fact]
        public async Task CreateCreditoAsyncDeveAplicarDescontoDaFormaPagamentoNoValorDinheiro()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria-desconto-forma@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ConfigLojaFormaPagamentoModel formaPagamento = await CriarFormaPagamentoAsync(context, loja.Id, "Pix", -5m);

            PagamentoService service = new(context);
            PagamentoCreditoDto resultado = await service.CreateCreditoAsync(
                new CriarPagamentoCreditoCommand
                {
                    LojaId = loja.Id,
                    ClienteId = cliente.Id,
                    Tipo = TipoPagamentoCredito.AdicionarCredito,
                    ConfigLojaFormaPagamentoId = formaPagamento.Id,
                    ValorCredito = 100m,
                    Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc)
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(95m, resultado.ValorDinheiro);
            Assert.Equal("Pix", resultado.FormaPagamentoNome);
        }

        [Fact]
        public async Task CreateCreditoAsyncDeveImpedirResgateQuandoClienteNaoPossuirSaldoSuficiente()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel fornecedor = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001");
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m, 60m);

            PagamentoService service = new(context);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCreditoAsync(
                new CriarPagamentoCreditoCommand
                {
                    LojaId = loja.Id,
                    ClienteId = fornecedor.Id,
                    Tipo = TipoPagamentoCredito.ResgatarCredito,
                    ValorCredito = 120m,
                    Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc)
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                }));

            Assert.Contains("credito positivo", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateCreditoAsyncDeveImpedirResgateQuandoCreditoDoClienteNaoForPositivo()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel fornecedor = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001");
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m, 60m);
            _ = context.ClientesCreditos.Add(new ClienteCreditoModel
            {
                LojaId = loja.Id,
                ClienteId = fornecedor.Id,
                Valor = -10m
            });
            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCreditoAsync(
                new CriarPagamentoCreditoCommand
                {
                    LojaId = loja.Id,
                    ClienteId = fornecedor.Id,
                    Tipo = TipoPagamentoCredito.ResgatarCredito,
                    ValorCredito = 5m,
                    Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc)
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                }));

            Assert.Contains("credito positivo", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateCreditoAsyncDeveNormalizarDataUnspecifiedParaUtc()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria-data-unspecified@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente B", "44999990002");
            ConfigLojaFormaPagamentoModel formaPagamento = await CriarFormaPagamentoAsync(context, loja.Id, "Cartao credito", 4m);

            PagamentoService service = new(context);
            PagamentoCreditoDto resultado = await service.CreateCreditoAsync(
                new CriarPagamentoCreditoCommand
                {
                    LojaId = loja.Id,
                    ClienteId = cliente.Id,
                    Tipo = TipoPagamentoCredito.AdicionarCredito,
                    ConfigLojaFormaPagamentoId = formaPagamento.Id,
                    ValorCredito = 50m,
                    Data = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Unspecified)
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(DateTimeKind.Utc, resultado.Data.Kind);

            PagamentoCreditoModel pagamento = await context.PagamentosCredito.SingleAsync();
            Assert.Equal(DateTimeKind.Utc, pagamento.Data.Kind);
            Assert.Equal(new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), pagamento.Data);
        }

        [Fact]
        public async Task CreateCreditoAsyncDeveImpedirPagamentoDoClienteSemFormaPagamento()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria-sem-forma@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            PagamentoService service = new(context);

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateCreditoAsync(
                new CriarPagamentoCreditoCommand
                {
                    LojaId = loja.Id,
                    ClienteId = cliente.Id,
                    Tipo = TipoPagamentoCredito.AdicionarCredito,
                    ValorCredito = 50m,
                    Data = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new CriarPagamentoCreditoParametros
                {
                    UsuarioId = usuario.Id
                }));
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

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(
            RenovaDbContext context,
            int lojaId,
            decimal percentualRepasseFornecedor,
            decimal percentualRepasseVendedorCredito)
        {
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

        private static async Task<ConfigLojaFormaPagamentoModel> CriarFormaPagamentoAsync(
            RenovaDbContext context,
            int lojaId,
            string nome,
            decimal percentualAjuste)
        {
            ConfigLojaModel config = await context.ConfiguracoesLoja
                .Include(item => item.FormasPagamento)
                .SingleOrDefaultAsync(item => item.LojaId == lojaId)
                ?? new ConfigLojaModel
                {
                    LojaId = lojaId,
                    PercentualRepasseFornecedor = 45m,
                    PercentualRepasseVendedorCredito = 60m,
                    TempoPermanenciaProdutoMeses = 6
                };

            if (config.Id == 0)
            {
                _ = context.ConfiguracoesLoja.Add(config);
            }

            ConfigLojaFormaPagamentoModel formaPagamento = new()
            {
                Nome = nome,
                PercentualAjuste = percentualAjuste
            };

            config.FormasPagamento.Add(formaPagamento);
            _ = await context.SaveChangesAsync();
            return formaPagamento;
        }

        private static async Task<decimal> ObterCreditoAsync(RenovaDbContext context, int clienteId)
        {
            ClienteCreditoModel credito = await context.ClientesCreditos.SingleAsync(item => item.ClienteId == clienteId);
            return credito.Valor;
        }
    }
}
