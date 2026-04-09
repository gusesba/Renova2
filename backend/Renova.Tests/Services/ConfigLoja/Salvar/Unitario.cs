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
                PercentualRepasseFornecedor = 45m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            });

            Assert.Equal(loja.Id, resultado.LojaId);
            Assert.Equal(45m, resultado.PercentualRepasseFornecedor);
            ConfigLojaModel configSalva = await context.ConfiguracoesLoja.SingleAsync();
            Assert.Equal(loja.Id, configSalva.LojaId);
            Assert.Equal(45m, configSalva.PercentualRepasseFornecedor);
        }

        [Fact]
        public async Task SaveAsyncDeveAtualizarConfiguracaoDeRepasseQuandoLojaJaPossuirRegistro()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m);
            ConfigLojaService service = new(context);

            ConfigLojaDto resultado = await service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 50m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            });

            Assert.Equal(50m, resultado.PercentualRepasseFornecedor);
            Assert.Single(context.ConfiguracoesLoja);
            Assert.Equal(50m, (await context.ConfiguracoesLoja.SingleAsync()).PercentualRepasseFornecedor);
        }

        [Fact]
        public async Task SaveAsyncDeveImpedirPercentualDeRepasseForaDoIntervaloPermitido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ConfigLojaService service = new(context);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveAsync(new SalvarConfigLojaCommand
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 150m
            }, new SalvarConfigLojaParametros
            {
                UsuarioId = loja.UsuarioId
            }));
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
                PercentualRepasseFornecedor = 45m
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

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(RenovaDbContext context, int lojaId, decimal percentualRepasseFornecedor)
        {
            ConfigLojaModel config = new()
            {
                LojaId = lojaId,
                PercentualRepasseFornecedor = percentualRepasseFornecedor
            };

            _ = context.ConfiguracoesLoja.Add(config);
            _ = await context.SaveChangesAsync();

            return config;
        }
    }
}
