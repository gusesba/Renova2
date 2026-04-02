using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Commands.Renova;
using Renova.Service.Queries.Renova;
using Renova.Service.Services.Renova;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Renova
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task CreateAsyncDeveSalvarERetornarEntidade()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();
            RenovaService service = new(context);

            RenovaCommand command = new()
            {
                Campo2 = "teste",
                Campo3 = 123
            };

            RenovaModel resultado = await service.CreateAsync(command);

            Assert.NotNull(resultado);
            Assert.Equal("teste", resultado.Campo2);
            Assert.Equal(123, resultado.Campo3);

            RenovaModel salvoNoBanco = await context.Renova.SingleAsync();

            Assert.Equal(resultado.Campo1, salvoNoBanco.Campo1);
            Assert.Equal("teste", salvoNoBanco.Campo2);
            Assert.Equal(123, salvoNoBanco.Campo3);
        }

        [Fact]
        public async Task GetAsyncDeveRetornarRegistroQuandoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            RenovaModel entidade = new()
            {
                Campo2 = "existente",
                Campo3 = 50
            };

            _ = context.Renova.Add(entidade);
            _ = await context.SaveChangesAsync();

            RenovaService service = new(context);

            RenovaQuery query = new()
            {
                CampoQuery = entidade.Campo1
            };

            RenovaModel? resultado = await service.GetAsync(query);

            Assert.NotNull(resultado);
            Assert.Equal(entidade.Campo1, resultado.Campo1);
            Assert.Equal("existente", resultado.Campo2);
            Assert.Equal(50, resultado.Campo3);
        }

        [Fact]
        public async Task GetAsyncDeveRetornarNullQuandoNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();
            RenovaService service = new(context);

            RenovaQuery query = new()
            {
                CampoQuery = 999
            };

            RenovaModel? resultado = await service.GetAsync(query);

            Assert.Null(resultado);
        }
    }
}