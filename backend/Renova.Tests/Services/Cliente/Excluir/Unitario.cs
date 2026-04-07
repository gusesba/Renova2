using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Services.Cliente;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Excluir
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        private const string RelacionamentosAtivosNaoImplementados = "Relacionamentos ativos de cliente ainda nao implementados.";

        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        // Input: usuario autenticado e cliente existente na loja do usuario
        // Remove o cliente persistido
        // Retorna: conclusao sem erro e cliente ausente na base
        public async Task DeleteAsyncDeveExcluirClienteDaLojaDoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            ExcluirClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            await service.DeleteAsync(parametros);

            Assert.Empty(context.Clientes);
        }

        [Fact]
        // Input: usuario autenticado tentando excluir cliente de loja de outro usuario
        // Nao remove o cliente
        // Retorna: erro de autorizacao
        public async Task DeleteAsyncDeveImpedirExclusaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            UsuarioModel outroUsuario = new()
            {
                Nome = "Joao Souza",
                Email = "joao@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(outroUsuario);
            _ = await context.SaveChangesAsync();

            ExcluirClienteParametros parametros = new()
            {
                UsuarioId = outroUsuario.Id,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(parametros));

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync();
            Assert.Equal(cliente.Id, clienteSalvo.Id);
        }

        [Fact]
        // Input: cliente informado nao existe
        // Nao remove registro algum
        // Retorna: erro de entidade nao encontrada
        public async Task DeleteAsyncDeveFalharQuandoClienteNaoForEncontrado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            ExcluirClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = 999
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(parametros));
        }

        [Fact(Skip = RelacionamentosAtivosNaoImplementados)]
        // Input: cliente com relacionamentos ativos
        // Nao remove o cliente enquanto houver dependencias de negocio
        // Retorna: mensagem adequada explicando o bloqueio
        public async Task DeleteAsyncDeveImpedirExclusaoQuandoClientePossuirRelacionamentosAtivos()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            // Arrange
            // TODO: quando existirem tabelas relacionadas com cliente, criar dependencias ativas
            // TODO: exemplos esperados: pedidos em aberto, fiados ativos, agendamentos pendentes ou contratos vinculados

            // Act
            // TODO: executar service.DeleteAsync(...).

            // Assert
            // TODO: validar InvalidOperationException com mensagem de negocio adequada
            // TODO: sugestao de mensagem: "Cliente possui relacionamentos ativos e nao pode ser excluido."
            // TODO: validar que o cliente permanece salvo na base.
            await Task.CompletedTask;
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

        private static async Task<ClienteModel> CriarClienteAsync(RenovaDbContext context, int lojaId, string nome, string contato)
        {
            ClienteModel cliente = new()
            {
                Nome = nome,
                Contato = contato,
                LojaId = lojaId
            };

            _ = context.Clientes.Add(cliente);
            _ = await context.SaveChangesAsync();

            return cliente;
        }
    }
}
