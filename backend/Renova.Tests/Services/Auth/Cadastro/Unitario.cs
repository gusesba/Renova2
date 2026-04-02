using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Services.Auth;
using Renova.Tests.Infrastructure;
using System.IdentityModel.Tokens.Jwt;

namespace Renova.Tests.Services.Auth.Cadastro;

public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
{
    protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
    {
        return new RenovaDbContext(options);
    }

    [Fact]
    //Input: x
    //Grava usuario no banco com senha hash
    //Retorna usuario e token
    public async Task CreateAsync_DeveSalvarComSenhaHashERetornarUsuarioToken()
    {
        await using var context = CriarContextoEmMemoria();
        var jwtSettings = JwtTokenAssert.CreateTestingSettings();
        var jwtTokenService = new JwtTokenService(Options.Create(jwtSettings));
        var service = new AuthService(context, jwtTokenService);

        var command = new CadastroCommand
        {
            Nome = "Maria da Silva",
            Email = "maria@renova.com",
            Senha = "Senha@123"
        };

        var resultado = await service.CreateAsync(command);
        var salvoNoBanco = await context.Usuarios.SingleAsync();
        _ = JwtTokenAssert.Validate(resultado.Token, jwtSettings);
        var jwt = JwtTokenAssert.Read(resultado.Token);

        Assert.NotNull(resultado);
        Assert.Equal("Maria da Silva", resultado.Usuario.Nome);
        Assert.Equal("maria@renova.com", resultado.Usuario.Email);
        Assert.Equal("maria@renova.com", jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value);
        Assert.NotEqual(command.Senha, salvoNoBanco.SenhaHash);
        Assert.False(string.IsNullOrWhiteSpace(salvoNoBanco.SenhaHash));
    }

    [Fact]
    //Input: email ja cadastrado
    //Nao grava novo usuario no banco
    //Retorna: erro de regra de negocio
    public async Task CreateAsync_DeveImpedirCadastroQuandoEmailJaExistir()
    {
        await using var context = CriarContextoEmMemoria();
        context.Usuarios.Add(new UsuarioModel
        {
            Nome = "Maria da Silva",
            Email = "maria@renova.com",
            SenhaHash = "hash-existente"
        });
        await context.SaveChangesAsync();

        var jwtSettings = JwtTokenAssert.CreateTestingSettings();
        var jwtTokenService = new JwtTokenService(Options.Create(jwtSettings));
        var service = new AuthService(context, jwtTokenService);

        var command = new CadastroCommand
        {
            Nome = "Maria da Silva",
            Email = "maria@renova.com",
            Senha = "Senha@123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(command));
    }

    [Fact]
    //Input: senha em texto puro
    //Nunca persiste senha em texto puro
    //Retorna: usuario com senha protegida e token
    public async Task CreateAsync_DevePersistirSenhaApenasComoHash()
    {
        await using var context = CriarContextoEmMemoria();
        var jwtSettings = JwtTokenAssert.CreateTestingSettings();
        var jwtTokenService = new JwtTokenService(Options.Create(jwtSettings));
        var service = new AuthService(context, jwtTokenService);

        var command = new CadastroCommand
        {
            Nome = "Joao Souza",
            Email = "joao@renova.com",
            Senha = "Senha@123"
        };

        var resultado = await service.CreateAsync(command);
        var salvoNoBanco = await context.Usuarios.SingleAsync();
        _ = JwtTokenAssert.Validate(resultado.Token, jwtSettings);
        var jwt = JwtTokenAssert.Read(resultado.Token);

        Assert.NotNull(resultado);
        Assert.NotEqual(command.Senha, salvoNoBanco.SenhaHash);
        Assert.Equal(resultado.Usuario.Email, salvoNoBanco.Email);
        Assert.Equal(resultado.Usuario.Nome, salvoNoBanco.Nome);
        Assert.Equal("joao@renova.com", jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value);
    }
}
