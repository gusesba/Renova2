using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Domain.Settings;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Services.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Auth.Login
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        //Input: email e senha validos
        //Nao grava novo usuario no banco
        //Retorna: usuario e token
        public async Task LoginAsyncDeveRetornarUsuarioTokenQuandoCredenciaisForemValidas()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();
            _ = context.Usuarios.Add(new UsuarioModel
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123")
            });
            _ = await context.SaveChangesAsync();

            JwtSettings jwtSettings = JwtTokenAssert.CreateTestingSettings();
            JwtTokenService jwtTokenService = new(Options.Create(jwtSettings));
            AuthService service = new(context, jwtTokenService);

            LoginCommand command = new()
            {
                Email = "maria@renova.com",
                Senha = "Senha@123"
            };

            UsuarioTokenDto resultado = await service.LoginAsync(command);

            Assert.NotNull(resultado);
            Assert.Equal("maria@renova.com", resultado.Usuario.Email);
            Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        }

        [Fact]
        //Input: email inexistente
        //Nao autentica usuario
        //Retorna: erro de regra de negocio
        public async Task LoginAsyncDeveImpedirAutenticacaoQuandoEmailNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();
            JwtSettings jwtSettings = JwtTokenAssert.CreateTestingSettings();
            JwtTokenService jwtTokenService = new(Options.Create(jwtSettings));
            AuthService service = new(context, jwtTokenService);

            LoginCommand command = new()
            {
                Email = "inexistente@renova.com",
                Senha = "Senha@123"
            };

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(command));
        }

        [Fact]
        //Input: senha invalida
        //Nao autentica usuario
        //Retorna: erro de regra de negocio
        public async Task LoginAsyncDeveImpedirAutenticacaoQuandoSenhaForInvalida()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();
            _ = context.Usuarios.Add(new UsuarioModel
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123")
            });
            _ = await context.SaveChangesAsync();

            JwtSettings jwtSettings = JwtTokenAssert.CreateTestingSettings();
            JwtTokenService jwtTokenService = new(Options.Create(jwtSettings));
            AuthService service = new(context, jwtTokenService);

            LoginCommand command = new()
            {
                Email = "maria@renova.com",
                Senha = "SenhaErrada@123"
            };

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(command));
        }
    }
}