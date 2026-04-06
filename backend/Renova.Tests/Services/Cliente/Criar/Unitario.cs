using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cliente;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Services.Cliente;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Criar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        // Input: usuario autenticado, loja do usuario e payload valido
        // Grava cliente vinculado a loja informada
        // Retorna: cliente criado com id, nome, contato, lojaId e userId
        public async Task CreateAsyncDeveCriarClienteParaLojaDoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44999990000",
                LojaId = loja.Id
            };

            CriarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId
            };

            ClienteService service = new(context);
            ClienteDto resultado = await service.CreateAsync(command, parametros);

            Assert.NotNull(resultado);
            Assert.True(resultado.Id > 0);
            Assert.Equal(command.Nome, resultado.Nome);
            Assert.Equal(command.Contato, resultado.Contato);
            Assert.Equal(command.LojaId, resultado.LojaId);
            Assert.Null(resultado.UserId);

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync();
            Assert.Equal(resultado.Id, clienteSalvo.Id);
            Assert.Equal(command.Nome, clienteSalvo.Nome);
            Assert.Equal(command.Contato, clienteSalvo.Contato);
            Assert.Equal(command.LojaId, clienteSalvo.LojaId);
            Assert.Null(clienteSalvo.UserId);
        }

        [Fact]
        // Input: payload valido com userId de uma conta existente
        // Vincula a conta cadastrada ao cliente criado
        // Retorna: cliente com userId preenchido
        public async Task CreateAsyncDeveAssociarContaExistenteAoClienteQuandoUserIdForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            UsuarioModel contaCliente = new()
            {
                Nome = "Cliente Vinculado",
                Email = "cliente@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(contaCliente);
            _ = await context.SaveChangesAsync();

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44999990000",
                LojaId = loja.Id,
                UserId = contaCliente.Id
            };

            CriarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId
            };

            ClienteService service = new(context);
            ClienteDto resultado = await service.CreateAsync(command, parametros);

            Assert.NotNull(resultado);
            Assert.True(resultado.Id > 0);
            Assert.Equal(contaCliente.Id, resultado.UserId);

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync();
            Assert.Equal(contaCliente.Id, clienteSalvo.UserId);
        }

        [Fact]
        // Input: loja ja possui cliente com o mesmo nome
        // Nao grava cliente duplicado na mesma loja
        // Retorna: erro de regra de negocio
        public async Task CreateAsyncDeveImpedirCriacaoQuandoLojaJaPossuirClienteComMesmoNome()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            _ = context.Clientes.Add(new ClienteModel
            {
                Nome = "Cliente A",
                Contato = "44999990000",
                LojaId = loja.Id
            });
            _ = await context.SaveChangesAsync();

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44888880000",
                LojaId = loja.Id
            };

            CriarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(command, parametros));

            _ = Assert.Single(context.Clientes.Where(cliente => cliente.LojaId == loja.Id));
        }

        [Fact]
        // Input: lojas diferentes com cliente de mesmo nome
        // Permite repeticao de nome entre lojas distintas
        // Retorna: cliente criado na segunda loja
        public async Task CreateAsyncDevePermitirMesmoNomeEmLojasDiferentes()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel primeiraLoja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            LojaModel segundaLoja = await CriarLojaAsync(context, "Loja Bairro", "joao@renova.com");

            _ = context.Clientes.Add(new ClienteModel
            {
                Nome = "Cliente A",
                Contato = "44999990000",
                LojaId = primeiraLoja.Id
            });
            _ = await context.SaveChangesAsync();

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44888880000",
                LojaId = segundaLoja.Id
            };

            CriarClienteParametros parametros = new()
            {
                UsuarioId = segundaLoja.UsuarioId
            };

            ClienteService service = new(context);
            ClienteDto resultado = await service.CreateAsync(command, parametros);

            Assert.NotNull(resultado);
            Assert.True(resultado.Id > 0);
            Assert.Equal(segundaLoja.Id, resultado.LojaId);
            _ = Assert.Single(context.Clientes.Where(cliente => cliente.LojaId == primeiraLoja.Id && cliente.Nome == command.Nome));
            _ = Assert.Single(context.Clientes.Where(cliente => cliente.LojaId == segundaLoja.Id && cliente.Nome == command.Nome));
        }

        [Fact]
        // Input: loja informada nao pertence ao usuario autenticado
        // Nao grava cliente em loja de outro usuario
        // Retorna: erro de autorizacao
        public async Task CreateAsyncDeveImpedirCriacaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            UsuarioModel outroUsuario = new()
            {
                Nome = "Joao Souza",
                Email = "joao@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(outroUsuario);
            _ = await context.SaveChangesAsync();

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44999990000",
                LojaId = loja.Id
            };

            CriarClienteParametros parametros = new()
            {
                UsuarioId = outroUsuario.Id
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(command, parametros));

            Assert.Empty(context.Clientes.Where(cliente => cliente.LojaId == loja.Id));
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, string nomeLoja, string emailUsuario)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = emailUsuario,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            LojaModel loja = new()
            {
                Nome = nomeLoja,
                UsuarioId = usuario.Id
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();

            return loja;
        }
    }
}
