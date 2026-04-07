#pragma warning disable xUnit1004

using Microsoft.EntityFrameworkCore;

using Renova.Persistence;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Editar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact(Skip = "Implementar quando a funcionalidade de edicao de cliente estiver disponivel.")]
        // Input: usuario autenticado, cliente existente na loja do usuario e payload valido
        // Atualiza os dados do cliente mantendo o vinculo com a loja
        // Retorna: cliente editado com id, nome, contato, lojaId e userId atualizados
        public Task EditAsyncDeveEditarClienteDaLojaDoUsuarioAutenticado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando a funcionalidade de edicao de cliente estiver disponivel.")]
        // Input: cliente existente sendo editado sem alterar o proprio nome
        // Permite salvar a edicao sem tratar o proprio registro como duplicado
        // Retorna: cliente editado com sucesso
        public Task EditAsyncDevePermitirManterMesmoNomeQuandoPertencerAoMesmoCliente()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando a funcionalidade de edicao de cliente estiver disponivel.")]
        // Input: loja ja possui outro cliente com o nome informado na edicao
        // Nao salva a alteracao para evitar duplicidade de nome na mesma loja
        // Retorna: erro de regra de negocio
        public Task EditAsyncDeveImpedirEdicaoQuandoLojaJaPossuirOutroClienteComMesmoNome()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando a funcionalidade de edicao de cliente estiver disponivel.")]
        // Input: outra loja possui cliente com o mesmo nome informado
        // Permite a edicao porque a restricao vale apenas dentro da mesma loja
        // Retorna: cliente editado com sucesso
        public Task EditAsyncDevePermitirMesmoNomeQuandoClienteDuplicadoEstiverEmOutraLoja()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando a funcionalidade de edicao de cliente estiver disponivel.")]
        // Input: usuario autenticado tenta editar cliente de loja que nao lhe pertence
        // Nao salva alteracoes em cliente de outro usuario
        // Retorna: erro de autorizacao
        public Task EditAsyncDeveImpedirEdicaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando a funcionalidade de edicao de cliente estiver disponivel.")]
        // Input: cliente informado nao existe
        // Nao realiza atualizacao
        // Retorna: erro de entidade nao encontrada
        public Task EditAsyncDeveFalharQuandoClienteNaoForEncontrado()
        {
            return Task.CompletedTask;
        }
    }
}

#pragma warning restore xUnit1004
