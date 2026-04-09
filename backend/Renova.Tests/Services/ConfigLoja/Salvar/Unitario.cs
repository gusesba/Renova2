using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.ConfigLoja;
using Renova.Service.Parameters.ConfigLoja;
using Renova.Service.Services.ConfigLoja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.ConfigLoja.Salvar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task SaveAsyncDeveCriarConfiguracaoDeRepasseQuandoLojaAindaNaoPossuirRegistro()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            ConfigLojaDto resultado = await service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            });

            Assert.Equal(loja.Id, resultado.LojaId);
            Assert.Equal(45m, resultado.PercentualRepasseFornecedor);
            Assert.Equal(45m, resultado.PercentualRepasseVendedorCredito);
            ConfigLojaModel configSalva = await context.ConfiguracoesLoja.SingleAsync();
            Assert.Equal(loja.Id, configSalva.LojaId);
            Assert.Equal(45m, configSalva.PercentualRepasseFornecedor);
            Assert.Equal(45m, configSalva.PercentualRepasseVendedorCredito);
        }

        [Fact]
        public async Task SaveAsyncDeveAtualizarConfiguracaoDeRepasseQuandoLojaJaPossuirRegistro()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m, 45m);
            ConfigLojaService service = new(context);

            ConfigLojaDto resultado = await service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 50m,
                PercentualRepasseVendedorCredito = 60m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            });

            Assert.Equal(50m, resultado.PercentualRepasseFornecedor);
            Assert.Equal(60m, resultado.PercentualRepasseVendedorCredito);
            Assert.Single(context.ConfiguracoesLoja);
            Assert.Equal(50m, (await context.ConfiguracoesLoja.SingleAsync()).PercentualRepasseFornecedor);
            Assert.Equal(60m, (await context.ConfiguracoesLoja.SingleAsync()).PercentualRepasseVendedorCredito);
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirPercentualDeRepasseAoFornecedorForaDoIntervaloPermitido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 150m,
                PercentualRepasseVendedorCredito = 100m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirPercentualDeRepasseAoVendedorEmCreditoForaDoIntervaloPermitido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 150m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirQuandoRepasseEmCreditoForMenorQueRepasseNormal()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 30m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));

            Assert.Contains("maior ou igual", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirAlteracaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = 9999
            }));
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, string nomeLoja, string emailUsuario)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = emailUsuario,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            LojaModel loja = new()
            {
                Nome = nomeLoja,
                UsuarioId = usuario.Id
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();

            return loja;
        }

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(RenovaDbContext context, int lojaId, decimal percentualRepasseFornecedor, decimal percentualRepasseVendedorCredito)
        {
            ConfigLojaModel config = new()
            {
                LojaId = lojaId,
                PercentualRepasseFornecedor = percentualRepasseFornecedor,
                PercentualRepasseVendedorCredito = percentualRepasseVendedorCredito
            };

            _ = context.ConfiguracoesLoja.Add(config);
            _ = await context.SaveChangesAsync();

            return config;
        }
    }
}
