using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cliente;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Services.Cliente;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Editar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        // Input: usuario autenticado, cliente existente na loja do usuario e payload valido
        // Atualiza os dados do cliente mantendo o vinculo com a loja
        // Retorna: cliente editado com id, nome, contato, lojaId e userId atualizados
        public async Task EditAsyncDeveEditarClienteDaLojaDoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            UsuarioModel contaCliente = new()
            {
                Nome = "Cliente Vinculado",
                Email = "cliente@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(contaCliente);
            _ = await context.SaveChangesAsync();

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "44888880000",
                UserId = contaCliente.Id
            };

            EditarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            ClienteDto resultado = await service.EditAsync(command, parametros);

            Assert.NotNull(resultado);
            Assert.Equal(cliente.Id, resultado.Id);
            Assert.Equal(command.Nome, resultado.Nome);
            Assert.Equal(command.Contato, resultado.Contato);
            Assert.Equal(cliente.LojaId, resultado.LojaId);
            Assert.Equal(command.UserId, resultado.UserId);

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync(clienteAtual => clienteAtual.Id == cliente.Id);
            Assert.Equal(command.Nome, clienteSalvo.Nome);
            Assert.Equal(command.Contato, clienteSalvo.Contato);
            Assert.Equal(command.UserId, clienteSalvo.UserId);
        }

        [Fact]
        // Input: contato editado com mascara e caracteres nao numericos
        // Normaliza o valor antes de persistir
        // Retorna: cliente atualizado somente com digitos no contato
        public async Task EditAsyncDeveManterApenasDigitosNoContato()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "(44) 98888-7777"
            };

            EditarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            ClienteDto resultado = await service.EditAsync(command, parametros);

            Assert.Equal("44988887777", resultado.Contato);

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync(clienteAtual => clienteAtual.Id == cliente.Id);
            Assert.Equal("44988887777", clienteSalvo.Contato);
        }

        [Fact]
        // Input: contato editado sem 10 ou 11 digitos
        // Impede a persistencia apos a normalizacao
        // Retorna: erro de validacao
        public async Task EditAsyncDeveFalharQuandoContatoNaoPossuirQuantidadeValidaDeDigitos()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "9999-999"
            };

            EditarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.EditAsync(command, parametros));
        }

        [Fact]
        // Input: cliente existente sendo editado sem alterar o proprio nome
        // Permite salvar a edicao sem tratar o proprio registro como duplicado
        // Retorna: cliente editado com sucesso
        public async Task EditAsyncDevePermitirManterMesmoNomeQuandoPertencerAoMesmoCliente()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            EditarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44888880000"
            };

            EditarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            ClienteDto resultado = await service.EditAsync(command, parametros);

            Assert.Equal(cliente.Id, resultado.Id);
            Assert.Equal("Cliente A", resultado.Nome);
            Assert.Equal(command.Contato, resultado.Contato);
            _ = Assert.Single(context.Clientes.Where(clienteAtual => clienteAtual.LojaId == loja.Id && clienteAtual.Nome == "Cliente A"));
        }

        [Fact]
        // Input: loja ja possui outro cliente com o nome informado na edicao
        // Nao salva a alteracao para evitar duplicidade de nome na mesma loja
        // Retorna: erro de regra de negocio
        public async Task EditAsyncDeveImpedirEdicaoQuandoLojaJaPossuirOutroClienteComMesmoNome()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            _ = await CriarClienteAsync(context, loja.Id, "Cliente B", "44888880000");

            EditarClienteCommand command = new()
            {
                Nome = "Cliente B",
                Contato = "44777770000"
            };

            EditarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.EditAsync(command, parametros));

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync(clienteAtual => clienteAtual.Id == cliente.Id);
            Assert.Equal("Cliente A", clienteSalvo.Nome);
            Assert.Equal("44999990000", clienteSalvo.Contato);
        }

        [Fact]
        // Input: outra loja possui cliente com o mesmo nome informado
        // Permite a edicao porque a restricao vale apenas dentro da mesma loja
        // Retorna: cliente editado com sucesso
        public async Task EditAsyncDevePermitirMesmoNomeQuandoClienteDuplicadoEstiverEmOutraLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel primeiraLoja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            LojaModel segundaLoja = await CriarLojaAsync(context, "Loja Bairro", "joao@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, primeiraLoja.Id, "Cliente A", "44999990000");
            _ = await CriarClienteAsync(context, segundaLoja.Id, "Cliente B", "44888880000");

            EditarClienteCommand command = new()
            {
                Nome = "Cliente B",
                Contato = "44777770000"
            };

            EditarClienteParametros parametros = new()
            {
                UsuarioId = primeiraLoja.UsuarioId,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            ClienteDto resultado = await service.EditAsync(command, parametros);

            Assert.Equal(cliente.Id, resultado.Id);
            Assert.Equal(command.Nome, resultado.Nome);
            Assert.Equal(command.Contato, resultado.Contato);
            _ = Assert.Single(context.Clientes.Where(clienteAtual => clienteAtual.LojaId == segundaLoja.Id && clienteAtual.Nome == command.Nome));
        }

        [Fact]
        // Input: usuario autenticado tenta editar cliente de loja que nao lhe pertence
        // Nao salva alteracoes em cliente de outro usuario
        // Retorna: erro de autorizacao
        public async Task EditAsyncDeveImpedirEdicaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
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

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "44888880000"
            };

            EditarClienteParametros parametros = new()
            {
                UsuarioId = outroUsuario.Id,
                ClienteId = cliente.Id
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.EditAsync(command, parametros));

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync(clienteAtual => clienteAtual.Id == cliente.Id);
            Assert.Equal("Cliente A", clienteSalvo.Nome);
            Assert.Equal("44999990000", clienteSalvo.Contato);
        }

        [Fact]
        // Input: cliente informado nao existe
        // Nao realiza atualizacao
        // Retorna: erro de entidade nao encontrada
        public async Task EditAsyncDeveFalharQuandoClienteNaoForEncontrado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "44888880000"
            };

            EditarClienteParametros parametros = new()
            {
                UsuarioId = loja.UsuarioId,
                ClienteId = 999
            };

            ClienteService service = new(context);
            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.EditAsync(command, parametros));
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
