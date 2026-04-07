#pragma warning disable xUnit1004

using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Editar
{
    public class Integracao
    {
        [Fact(Skip = "Implementar quando o endpoint de edicao de cliente estiver disponivel.")]
        // Input: usuario autenticado e payload valido para cliente existente
        // Atualiza cliente da loja do usuario autenticado
        // Retorna: ok com cliente editado
        public Task PutClienteDeveRetornarOkQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando o endpoint de edicao de cliente estiver disponivel.")]
        // Input: usuario autenticado tentando renomear cliente para nome ja existente na mesma loja
        // Nao salva a alteracao para evitar duplicidade
        // Retorna: conflict
        public Task PutClienteDeveRetornarConflictQuandoLojaJaPossuirOutroClienteComMesmoNome()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando o endpoint de edicao de cliente estiver disponivel.")]
        // Input: usuario autenticado mantendo o mesmo nome do cliente editado
        // Permite a alteracao dos demais campos sem conflito
        // Retorna: ok
        public Task PutClienteDeveRetornarOkQuandoClienteMantiverOProprioNome()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando o endpoint de edicao de cliente estiver disponivel.")]
        // Input: payload valido sem usuario autenticado
        // Nao salva alteracoes
        // Retorna: unauthorized
        public Task PutClienteDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando o endpoint de edicao de cliente estiver disponivel.")]
        // Input: usuario autenticado tentando editar cliente de loja de outro usuario
        // Nao salva alteracoes
        // Retorna: unauthorized
        public Task PutClienteDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando o endpoint de edicao de cliente estiver disponivel.")]
        // Input: payload invalido para cliente existente
        // Nao salva alteracoes
        // Retorna: bad request
        public Task PutClienteDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Implementar quando o endpoint de edicao de cliente estiver disponivel.")]
        // Input: cliente inexistente
        // Nao salva alteracoes
        // Retorna: not found
        public Task PutClienteDeveRetornarNotFoundQuandoClienteNaoForEncontrado()
        {
            return Task.CompletedTask;
        }
    }
}

#pragma warning restore xUnit1004
