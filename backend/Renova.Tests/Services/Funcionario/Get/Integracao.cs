using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

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

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dono.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"/api/funcionario?lojaId={lojaId}",
                new { usuarioId = funcionario.Usuario.Id });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            FuncionarioDto? body = await response.Content.ReadFromJsonAsync<FuncionarioDto>();

            Assert.NotNull(body);
            Assert.Equal(funcionario.Usuario.Id, body.UsuarioId);
            Assert.Equal(lojaId, body.LojaId);
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

            _ = context.Funcionarios.Add(new FuncionarioModel
            {
                UsuarioId = usuarioId,
                LojaId = lojaId
            });
            _ = await context.SaveChangesAsync();
        }
    }
}
