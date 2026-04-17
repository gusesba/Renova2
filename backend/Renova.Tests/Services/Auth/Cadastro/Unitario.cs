using System.IdentityModel.Tokens.Jwt;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Domain.Settings;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Services.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Auth.Cadastro
{
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
        public async Task CreateAsyncDeveSalvarComSenhaHashERetornarUsuarioToken()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();
            JwtSettings jwtSettings = JwtTokenAssert.CreateTestingSettings();
            JwtTokenService jwtTokenService = new(Options.Create(jwtSettings));
            AuthService service = new(context, jwtTokenService);
            DateTime emissaoMinima = DateTime.UtcNow;

            CadastroCommand command = new()
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                Senha = "Senha@123"
            };

            UsuarioTokenDto resultado = await service.CreateAsync(command);
            DateTime emissaoMaxima = DateTime.UtcNow;
            UsuarioModel salvoNoBanco = await context.Usuarios.SingleAsync();
            _ = JwtTokenAssert.Validate(resultado.Token, jwtSettings);
            JwtSecurityToken jwt = JwtTokenAssert.Read(resultado.Token);

            Assert.NotNull(resultado);
            Assert.Equal("Maria da Silva", resultado.Usuario.Nome);
            Assert.Equal("maria@renova.com", resultado.Usuario.Email);
            Assert.Equal("maria@renova.com", jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value);
            Assert.InRange(jwt.ValidTo, emissaoMinima.AddDays(7).AddSeconds(-1), emissaoMaxima.AddDays(7).AddSeconds(1));
            Assert.NotEqual(command.Senha, salvoNoBanco.SenhaHash);
            Assert.False(string.IsNullOrWhiteSpace(salvoNoBanco.SenhaHash));
        }

        [Fact]
        //Input: email ja cadastrado
        //Nao grava novo usuario no banco
        //Retorna: erro de regra de negocio
        public async Task CreateAsyncDeveImpedirCadastroQuandoEmailJaExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();
            _ = context.Usuarios.Add(new UsuarioModel
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                SenhaHash = "hash-existente"
            });
            _ = await context.SaveChangesAsync();

            JwtSettings jwtSettings = JwtTokenAssert.CreateTestingSettings();
            JwtTokenService jwtTokenService = new(Options.Create(jwtSettings));
            AuthService service = new(context, jwtTokenService);

            CadastroCommand command = new()
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                Senha = "Senha@123"
            };

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(command));
        }

        [Fact]
        //Input: senha em texto puro
        //Nunca persiste senha em texto puro
        //Retorna: usuario com senha protegida e token
        public async Task CreateAsyncDevePersistirSenhaApenasComoHash()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();
            JwtSettings jwtSettings = JwtTokenAssert.CreateTestingSettings();
            JwtTokenService jwtTokenService = new(Options.Create(jwtSettings));
            AuthService service = new(context, jwtTokenService);

            CadastroCommand command = new()
            {
                Nome = "Joao Souza",
                Email = "joao@renova.com",
                Senha = "Senha@123"
            };

            UsuarioTokenDto resultado = await service.CreateAsync(command);
            UsuarioModel salvoNoBanco = await context.Usuarios.SingleAsync();
            _ = JwtTokenAssert.Validate(resultado.Token, jwtSettings);
            JwtSecurityToken jwt = JwtTokenAssert.Read(resultado.Token);

            Assert.NotNull(resultado);
            Assert.NotEqual(command.Senha, salvoNoBanco.SenhaHash);
            Assert.Equal(resultado.Usuario.Email, salvoNoBanco.Email);
            Assert.Equal(resultado.Usuario.Nome, salvoNoBanco.Nome);
            Assert.Equal("joao@renova.com", jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value);
        }
    }
}
