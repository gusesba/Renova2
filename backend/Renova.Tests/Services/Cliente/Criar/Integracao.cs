namespace Renova.Tests.Services.Cliente.Criar
{
    public class Integracao
    {
        [Fact(Skip = "Esqueleto do teste de criacao de cliente ainda pendente de implementacao da feature.")]
        // Input: usuario autenticado e payload valido
        // Grava cliente na loja do usuario autenticado
        // Retorna: created com cliente criado
        public Task PostClienteDeveRetornarCreatedQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "Esqueleto do teste de criacao de cliente ainda pendente de implementacao da feature.")]
        // Input: usuario autenticado com cliente de mesmo nome na mesma loja
        // Nao grava cliente duplicado
        // Retorna: conflict
        public Task PostClienteDeveRetornarConflictQuandoLojaJaPossuirClienteComMesmoNome()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "Esqueleto do teste de criacao de cliente ainda pendente de implementacao da feature.")]
        // Input: payload valido sem usuario autenticado
        // Nao grava cliente
        // Retorna: unauthorized
        public Task PostClienteDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "Esqueleto do teste de criacao de cliente ainda pendente de implementacao da feature.")]
        // Input: usuario autenticado com payload invalido
        // Nao grava cliente
        // Retorna: bad request
        public Task PostClienteDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "Esqueleto do teste de criacao de cliente ainda pendente de implementacao da feature.")]
        // Input: usuario autenticado tentando criar cliente em loja de outro usuario
        // Nao grava cliente
        // Retorna: unauthorized
        public Task PostClienteDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            throw new NotImplementedException();
        }
    }
}
