using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands;
using Renova.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Renova.Tests.Services.Auth.Cadastro;

public class Integracao
{
    [Fact]
    //Input: payload de cadastro valido
    //Grava usuario no banco com senha hash
    //Retorna: usuario e token
    public async Task PostCadastro_DeveSalvarComSenhaHashERetornarUsuarioToken()
    {
        await using var factory = new RenovaApiFactory();
        var client = factory.CreateClient();

        var command = new CadastroCommand
        {
            Nome = "Maria da Silva",
            Email = "maria@renova.com",
            Senha = "Senha@123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();

        Assert.NotNull(body);
        Assert.Equal(command.Nome, body!.Usuario.Nome);
        Assert.Equal(command.Email, body.Usuario.Email);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

        var salvo = await context.Usuarios.SingleAsync();

        Assert.Equal(command.Nome, salvo.Nome);
        Assert.Equal(command.Email, salvo.Email);
        Assert.NotEqual(command.Senha, salvo.SenhaHash);
    }

    [Fact]
    //Input: payload com email ja cadastrado
    //Nao grava novo usuario no banco
    //Retorna: conflito de cadastro
    public async Task PostCadastro_DeveRetornarConflitoQuandoEmailJaExistir()
    {
        await using var factory = new RenovaApiFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            context.Usuarios.Add(new UsuarioModel
            {
                Nome = "Usuario Existente",
                Email = "duplicado@renova.com",
                SenhaHash = "hash-existente"
            });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();

        var command = new CadastroCommand
        {
            Nome = "Novo Usuario",
            Email = "duplicado@renova.com",
            Senha = "Senha@123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    //Input: payload de cadastro invalido
    //Nao grava usuario no banco
    //Retorna: erro de validacao
    public async Task PostCadastro_DeveRetornarErroValidacaoQuandoPayloadForInvalido()
    {
        await using var factory = new RenovaApiFactory();
        var client = factory.CreateClient();

        var command = new CadastroCommand
        {
            Nome = string.Empty,
            Email = "email-invalido",
            Senha = "123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
