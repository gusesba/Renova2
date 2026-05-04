using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Access;
using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Funcionario.Get
{
    public class Integracao
    {
        [Fact]
        public async Task GetFuncionariosDeveRetornarFuncionariosDaLojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto dono = await CriarUsuarioAutenticadoAsync(client, "dona@renova.com");
            UsuarioTokenDto funcionarioA = await CriarUsuarioAutenticadoAsync(client, "funcionarioa@renova.com");
            UsuarioTokenDto funcionarioB = await CriarUsuarioAutenticadoAsync(client, "funcionariob@renova.com");

            int lojaId = await CriarLojaAsync(factory, dono.Usuario.Id, "Loja Centro");
            await VincularFuncionarioAsync(factory, funcionarioA.Usuario.Id, lojaId);
            await VincularFuncionarioAsync(factory, funcionarioB.Usuario.Id, lojaId);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dono.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/funcionario?lojaId={lojaId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<FuncionarioDto>? body = await response.Content.ReadFromJsonAsync<List<FuncionarioDto>>();

            Assert.NotNull(body);
            Assert.Collection(body,
                funcionario => Assert.Equal(funcionarioA.Usuario.Email, funcionario.Email),
                funcionario => Assert.Equal(funcionarioB.Usuario.Email, funcionario.Email));
        }

        [Fact]
        public async Task PostFuncionarioDeveCriarVinculoParaLojaDoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto dono = await CriarUsuarioAutenticadoAsync(client, "dona@renova.com");
            UsuarioTokenDto funcionario = await CriarUsuarioAutenticadoAsync(client, "funcionario@renova.com");

            int lojaId = await CriarLojaAsync(factory, dono.Usuario.Id, "Loja Centro");
            int cargoId = await CriarCargoAsync(factory, lojaId);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dono.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"/api/funcionario?lojaId={lojaId}",
                new { usuarioId = funcionario.Usuario.Id, cargoId });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            FuncionarioDto? body = await response.Content.ReadFromJsonAsync<FuncionarioDto>();

            Assert.NotNull(body);
            Assert.Equal(funcionario.Usuario.Id, body.UsuarioId);
            Assert.Equal(lojaId, body.LojaId);
            Assert.Equal(cargoId, body.CargoId);
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string email)
        {
            CadastroCommand command = new()
            {
                Nome = email.Split('@')[0],
                Email = email,
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

            _ = response.EnsureSuccessStatusCode();

            UsuarioTokenDto? resultado = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();

            return Assert.IsType<UsuarioTokenDto>(resultado);
        }

        private static async Task<int> CriarLojaAsync(RenovaApiFactory factory, int usuarioId, string nome)
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

            return loja.Id;
        }

        private static async Task VincularFuncionarioAsync(RenovaApiFactory factory, int usuarioId, int lojaId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            CargoModel cargo = await ObterOuCriarCargoAsync(context, lojaId);

            _ = context.Funcionarios.Add(new FuncionarioModel
            {
                UsuarioId = usuarioId,
                LojaId = lojaId,
                CargoId = cargo.Id
            });
            _ = await context.SaveChangesAsync();
        }

        private static async Task<int> CriarCargoAsync(RenovaApiFactory factory, int lojaId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            CargoModel cargo = await ObterOuCriarCargoAsync(context, lojaId);
            return cargo.Id;
        }

        private static async Task<CargoModel> ObterOuCriarCargoAsync(RenovaDbContext context, int lojaId)
        {
            CargoModel? cargo = context.Cargos.SingleOrDefault(item => item.LojaId == lojaId && item.Nome == "Funcionario");

            if (cargo is not null)
            {
                return cargo;
            }

            cargo = new CargoModel
            {
                Nome = "Funcionario",
                LojaId = lojaId,
                Funcionalidades = [.. FuncionalidadeCatalogo.Itens.Select(item => new CargoFuncionalidadeModel
                {
                    FuncionalidadeId = item.Id
                })]
            };

            _ = context.Cargos.Add(cargo);
            _ = await context.SaveChangesAsync();
            return cargo;
        }
    }
}
