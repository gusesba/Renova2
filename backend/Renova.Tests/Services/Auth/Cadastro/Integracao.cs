using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Domain.Settings;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Auth.Cadastro
{
    public class Integracao
    {
        [Fact]
        //Input: payload de cadastro valido
        //Grava usuario no banco com senha hash
        //Retorna: usuario e token
        public async Task PostCadastroDeveSalvarComSenhaHashERetornarUsuarioToken()
        {
            await using var factory = new RenovaApiFactory();
            HttpClient client = factory.CreateClient();
            JwtSettings jwtSettings = JwtTokenAssert.CreateTestingSettings();

            var command = new CadastroCommand
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            UsuarioTokenDto? body = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();
            _ = JwtTokenAssert.Validate(body!.Token, jwtSettings);
            JwtSecurityToken jwt = JwtTokenAssert.Read(body.Token);

            Assert.NotNull(body);
            Assert.Equal(command.Nome, body.Usuario.Nome);
            Assert.Equal(command.Email, body.Usuario.Email);
            Assert.Equal(command.Email, jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            UsuarioModel salvo = await context.Usuarios.SingleAsync();

            Assert.Equal(command.Nome, salvo.Nome);
            Assert.Equal(command.Email, salvo.Email);
            Assert.NotEqual(command.Senha, salvo.SenhaHash);
        }

        [Fact]
        //Input: payload com email ja cadastrado
        //Nao grava novo usuario no banco
        //Retorna: conflito de cadastro
        public async Task PostCadastroDeveRetornarConflitoQuandoEmailJaExistir()
        {
            await using var factory = new RenovaApiFactory();

            using (IServiceScope scope = factory.Services.CreateScope())
            {
                RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

                _ = context.Usuarios.Add(new UsuarioModel
                {
                    Nome = "Usuario Existente",
                    Email = "duplicado@renova.com",
                    SenhaHash = "hash-existente"
                });
                _ = await context.SaveChangesAsync();
            }

            HttpClient client = factory.CreateClient();

            var command = new CadastroCommand
            {
                Nome = "Novo Usuario",
                Email = "duplicado@renova.com",
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        //Input: payload de cadastro invalido
        //Nao grava usuario no banco
        //Retorna: erro de validacao
        public async Task PostCadastroDeveRetornarErroValidacaoQuandoPayloadForInvalido()
        {
            await using var factory = new RenovaApiFactory();
            HttpClient client = factory.CreateClient();

            var command = new CadastroCommand
            {
                Nome = string.Empty,
                Email = "email-invalido",
                Senha = "123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}