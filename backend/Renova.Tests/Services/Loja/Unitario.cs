using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Loja;
using Renova.Service.Parameters.Loja;
using Renova.Service.Services.Loja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        //Input: usuario autenticado e nome de loja valido
        //Grava loja vinculada ao usuario autenticado
        //Retorna: loja criada com id e nome
        public async Task CreateAsyncDeveCriarLojaParaUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = new()
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            CriarLojaCommand command = new()
            {
                Nome = "Loja Centro"
            };

            CriarLojaParametros parametros = new()
            {
                UsuarioId = usuario.Id
            };

            LojaService service = new(context);
            LojaDto resultado = await service.CreateAsync(command, parametros);

            Assert.NotNull(resultado);
            Assert.True(resultado.Id > 0);
            Assert.Equal(command.Nome, resultado.Nome);

            LojaModel lojaSalva = await context.Lojas.SingleAsync();
            Assert.Equal(resultado.Id, lojaSalva.Id);
            Assert.Equal(command.Nome, lojaSalva.Nome);
            Assert.Equal(usuario.Id, lojaSalva.UsuarioId);
        }

        [Fact]
        //Input: usuario autenticado com loja de mesmo nome ja cadastrada
        //Nao grava nova loja duplicada para o mesmo usuario
        //Retorna: erro de regra de negocio
        public async Task CreateAsyncDeveImpedirCriacaoQuandoUsuarioJaPossuirLojaComMesmoNome()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = new()
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            LojaModel lojaExistente = new()
            {
                Nome = "Loja Centro",
                UsuarioId = usuario.Id
            };

            _ = context.Lojas.Add(lojaExistente);
            _ = await context.SaveChangesAsync();

            CriarLojaCommand command = new()
            {
                Nome = "Loja Centro"
            };

            CriarLojaParametros parametros = new()
            {
                UsuarioId = usuario.Id
            };

            LojaService service = new(context);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(command, parametros));

            Assert.Single(context.Lojas);
        }

        [Fact]
        //Input: usuarios diferentes criando lojas com o mesmo nome
        //Permite nomes repetidos entre usuarios distintos
        //Retorna: loja criada para o segundo usuario
        public async Task CreateAsyncDevePermitirMesmoNomeParaUsuariosDiferentes()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel primeiroUsuario = new()
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                SenhaHash = "hash"
            };

            UsuarioModel segundoUsuario = new()
            {
                Nome = "Joao Souza",
                Email = "joao@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(primeiroUsuario);
            _ = context.Usuarios.Add(segundoUsuario);
            _ = await context.SaveChangesAsync();

            LojaModel lojaExistente = new()
            {
                Nome = "Loja Centro",
                UsuarioId = primeiroUsuario.Id
            };

            _ = context.Lojas.Add(lojaExistente);
            _ = await context.SaveChangesAsync();

            CriarLojaCommand command = new()
            {
                Nome = "Loja Centro"
            };

            CriarLojaParametros parametros = new()
            {
                UsuarioId = segundoUsuario.Id
            };

            LojaService service = new(context);
            LojaDto resultado = await service.CreateAsync(command, parametros);

            Assert.NotNull(resultado);
            Assert.True(resultado.Id > 0);
            Assert.Equal(command.Nome, resultado.Nome);
            Assert.Single(context.Lojas.Where(loja => loja.UsuarioId == primeiroUsuario.Id && loja.Nome == command.Nome));
            Assert.Single(context.Lojas.Where(loja => loja.UsuarioId == segundoUsuario.Id && loja.Nome == command.Nome));
        }
    }
}
