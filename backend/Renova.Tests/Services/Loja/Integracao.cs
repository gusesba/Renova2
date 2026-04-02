using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja
{
    public class Integracao
    {
        private const string MotivoPendente = "TODO: implementar quando endpoint/autorizacao de Loja estiverem disponiveis.";

        [Fact(Skip = MotivoPendente)]
        //Input: usuario autenticado e payload valido
        //Grava loja vinculada ao usuario autenticado
        //Retorna: created com id e nome
        public async Task PostLojaDeveRetornarCreatedQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            // Arrange
            // TODO: criar usuario de teste.
            // TODO: autenticar requisicao com bearer token.
            // TODO: montar payload com nome da loja.

            // Act
            // TODO: enviar POST para /api/loja.

            // Assert
            // TODO: validar status code Created.
            // TODO: validar corpo com id e nome.
            // TODO: validar persistencia da loja no banco.
        }

        [Fact(Skip = MotivoPendente)]
        //Input: usuario autenticado com loja de mesmo nome ja cadastrada
        //Nao grava nova loja duplicada para o mesmo usuario
        //Retorna: conflict
        public async Task PostLojaDeveRetornarConflictQuandoUsuarioJaPossuirLojaComMesmoNome()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            // Arrange
            // TODO: criar usuario com loja existente.
            // TODO: autenticar requisicao com bearer token do mesmo usuario.
            // TODO: montar payload com nome repetido.

            // Act
            // TODO: enviar POST para /api/loja.

            // Assert
            // TODO: validar status code Conflict.
            // TODO: validar que nao houve duplicacao no banco.
        }

        [Fact(Skip = MotivoPendente)]
        //Input: payload valido sem usuario autenticado
        //Nao grava loja
        //Retorna: unauthorized
        public async Task PostLojaDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            // Arrange
            // TODO: montar payload com nome da loja sem token de autenticacao.

            // Act
            // TODO: enviar POST para /api/loja.

            // Assert
            // TODO: validar status code Unauthorized.
            // TODO: validar que nenhuma loja foi persistida.
        }

        [Fact(Skip = MotivoPendente)]
        //Input: usuario autenticado com payload invalido
        //Nao grava loja
        //Retorna: bad request
        public async Task PostLojaDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            // Arrange
            // TODO: criar usuario de teste.
            // TODO: autenticar requisicao com bearer token.
            // TODO: montar payload invalido para nome da loja.

            // Act
            // TODO: enviar POST para /api/loja.

            // Assert
            // TODO: validar status code BadRequest.
            // TODO: validar que nenhuma loja foi persistida.
        }
    }
}
