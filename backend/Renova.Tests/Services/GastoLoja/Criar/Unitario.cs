using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.GastoLoja;
using Renova.Service.Parameters.GastoLoja;
using Renova.Service.Services.GastoLoja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.GastoLoja.Criar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task CreateAsyncDeveCriarGastoLojaComDescricaoNormalizada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "gasto-loja-criar@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            GastoLojaService service = new(context);
            GastoLojaDto resultado = await service.CreateAsync(
                new CriarGastoLojaCommand
                {
                    LojaId = loja.Id,
                    Natureza = NaturezaGastoLoja.Pagamento,
                    Valor = 123.456m,
                    Data = new DateTime(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc),
                    Descricao = "  Conta de luz  "
                },
                new OperacaoGastoLojaParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(NaturezaGastoLoja.Pagamento, resultado.Natureza);
            Assert.Equal(123.46m, resultado.Valor);
            Assert.Equal("Conta de luz", resultado.Descricao);

            GastoLojaModel entity = await context.GastosLoja.SingleAsync();
            Assert.Equal(loja.Id, entity.LojaId);
            Assert.Equal("Conta de luz", entity.Descricao);
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
    }
}
