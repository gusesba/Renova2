using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Funcionario;
using Renova.Service.Parameters.Funcionario;
using Renova.Service.Services.Funcionario;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Funcionario.Criar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task CreateAsyncDeveCriarVinculoEntreUsuarioELoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel dono = new()
            {
                Nome = "Dona Loja",
                Email = "dona@renova.com",
                SenhaHash = "hash"
            };

            UsuarioModel funcionarioUsuario = new()
            {
                Nome = "Funcionario Um",
                Email = "funcionario@renova.com",
                SenhaHash = "hash"
            };

            context.Usuarios.AddRange(dono, funcionarioUsuario);
            _ = await context.SaveChangesAsync();

            LojaModel loja = new()
            {
                Nome = "Loja Centro",
                UsuarioId = dono.Id
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();

            CargoModel cargo = await CriarCargoAsync(context, loja.Id);

            FuncionarioService service = new(context);
            FuncionarioDto resultado = await service.CreateAsync(
                new CriarFuncionarioCommand
                {
                    UsuarioId = funcionarioUsuario.Id,
                    CargoId = cargo.Id
                },
                new CriarFuncionarioParametros
                {
                    UsuarioAutenticadoId = dono.Id,
                    LojaId = loja.Id
                });

            Assert.Equal(funcionarioUsuario.Id, resultado.UsuarioId);
            Assert.Equal(loja.Id, resultado.LojaId);
            Assert.Equal(funcionarioUsuario.Nome, resultado.Nome);
            Assert.Equal(funcionarioUsuario.Email, resultado.Email);
            Assert.Equal(cargo.Id, resultado.CargoId);

            FuncionarioModel vinculo = await context.Funcionarios.SingleAsync();
            Assert.Equal(funcionarioUsuario.Id, vinculo.UsuarioId);
            Assert.Equal(loja.Id, vinculo.LojaId);
            Assert.Equal(cargo.Id, vinculo.CargoId);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirFuncionarioDuplicadoNaMesmaLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel dono = new()
            {
                Nome = "Dona Loja",
                Email = "dona@renova.com",
                SenhaHash = "hash"
            };

            UsuarioModel funcionarioUsuario = new()
            {
                Nome = "Funcionario Um",
                Email = "funcionario@renova.com",
                SenhaHash = "hash"
            };

            context.Usuarios.AddRange(dono, funcionarioUsuario);
            _ = await context.SaveChangesAsync();

            LojaModel loja = new()
            {
                Nome = "Loja Centro",
                UsuarioId = dono.Id
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();

            CargoModel cargo = await CriarCargoAsync(context, loja.Id);

            _ = context.Funcionarios.Add(new FuncionarioModel
            {
                UsuarioId = funcionarioUsuario.Id,
                LojaId = loja.Id,
                CargoId = cargo.Id
            });
            _ = await context.SaveChangesAsync();

            FuncionarioService service = new(context);

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync(
                    new CriarFuncionarioCommand
                    {
                        UsuarioId = funcionarioUsuario.Id,
                        CargoId = cargo.Id
                    },
                    new CriarFuncionarioParametros
                    {
                        UsuarioAutenticadoId = dono.Id,
                        LojaId = loja.Id
                    }));

            _ = Assert.Single(context.Funcionarios);
        }

        private static async Task<CargoModel> CriarCargoAsync(RenovaDbContext context, int lojaId)
        {
            CargoModel cargo = new()
            {
                Nome = "Funcionario",
                LojaId = lojaId
            };

            _ = context.Cargos.Add(cargo);
            _ = await context.SaveChangesAsync();
            return cargo;
        }
    }
}
