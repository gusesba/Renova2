using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Parameters.GastoLoja;
using Renova.Service.Queries.GastoLoja;
using Renova.Service.Services.GastoLoja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.GastoLoja.Get
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetAllAsyncDeveAplicarOrdenacaoEPaginacao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "gasto-loja-get@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            context.GastosLoja.AddRange(
                new GastoLojaModel
                {
                    LojaId = loja.Id,
                    Natureza = NaturezaGastoLoja.Pagamento,
                    Valor = 80m,
                    Data = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    Descricao = "Conta de agua"
                },
                new GastoLojaModel
                {
                    LojaId = loja.Id,
                    Natureza = NaturezaGastoLoja.Pagamento,
                    Valor = 20m,
                    Data = new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc),
                    Descricao = "Cabides"
                },
                new GastoLojaModel
                {
                    LojaId = loja.Id,
                    Natureza = NaturezaGastoLoja.Recebimento,
                    Valor = 50m,
                    Data = new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc),
                    Descricao = "Reembolso"
                });
            _ = await context.SaveChangesAsync();

            GastoLojaService service = new(context);
            PaginacaoDto<GastoLojaBuscaDto> resultado = await service.GetAllAsync(
                new ObterGastosLojaQuery
                {
                    LojaId = loja.Id,
                    Natureza = NaturezaGastoLoja.Pagamento,
                    OrdenarPor = "valor",
                    Direcao = "asc",
                    Pagina = 2,
                    TamanhoPagina = 1
                },
                new OperacaoGastoLojaParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(2, resultado.TotalPaginas);

            GastoLojaBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Conta de agua", item.Descricao);
            Assert.Equal(80m, item.Valor);
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
