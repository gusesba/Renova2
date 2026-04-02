using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Auth.Login
{
    public class Integracao
    {
        [Fact]
        //Input: payload de login valido
        //Nao grava novo usuario no banco
        //Retorna: usuario e token
        public async Task PostLoginDeveRetornarUsuarioTokenQuandoCredenciaisForemValidas()
        {
            await using RenovaApiFactory factory = new();

            using (IServiceScope scope = factory.Services.CreateScope())
            {
                RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

                _ = context.Usuarios.Add(new UsuarioModel
                {
                    Nome = "Maria da Silva",
                    Email = "maria@renova.com",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123")
                });
                _ = await context.SaveChangesAsync();
            }

            HttpClient client = factory.CreateClient();

            LoginCommand command = new()
            {
                Email = "maria@renova.com",
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", command);
            UsuarioTokenDto? body = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal("maria@renova.com", body.Usuario.Email);
            Assert.False(string.IsNullOrWhiteSpace(body.Token));
        }

        [Fact]
        //Input: payload com email inexistente
        //Nao autentica usuario
        //Retorna: nao autorizado
        public async Task PostLoginDeveRetornarNaoAutorizadoQuandoEmailNaoExistir()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            LoginCommand command = new()
            {
                Email = "inexistente@renova.com",
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", command);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        //Input: payload com senha invalida
        //Nao autentica usuario
        //Retorna: nao autorizado
        public async Task PostLoginDeveRetornarNaoAutorizadoQuandoSenhaForInvalida()
        {
            await using RenovaApiFactory factory = new();

            using (IServiceScope scope = factory.Services.CreateScope())
            {
                RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

                _ = context.Usuarios.Add(new UsuarioModel
                {
                    Nome = "Maria da Silva",
                    Email = "maria@renova.com",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123")
                });
                _ = await context.SaveChangesAsync();
            }

            HttpClient client = factory.CreateClient();

            LoginCommand command = new()
            {
                Email = "maria@renova.com",
                Senha = "SenhaErrada@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", command);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        //Input: payload de login sem os campos necessarios
        //Nao autentica usuario
        //Retorna: bad request
        public async Task PostLoginDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            LoginCommand command = new()
            {
                Email = string.Empty,
                Senha = string.Empty
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}