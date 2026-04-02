using Microsoft.EntityFrameworkCore;

using Renova.Persistence;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        private const string MotivoPendente = "TODO: implementar quando command/model/service de Loja estiverem disponiveis.";

        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact(Skip = MotivoPendente)]
        //Input: usuario autenticado e nome de loja valido
        //Grava loja vinculada ao usuario autenticado
        //Retorna: loja criada com id e nome
        public async Task CreateAsyncDeveCriarLojaParaUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            // Arrange
            // TODO: criar usuario autenticado de teste.
            // TODO: instanciar LojaService.
            // TODO: montar command com Nome.

            // Act
            // TODO: executar CreateAsync.

            // Assert
            // TODO: validar retorno com Id e Nome.
            // TODO: validar persistencia da loja vinculada ao usuario.
        }

        [Fact(Skip = MotivoPendente)]
        //Input: usuario autenticado com loja de mesmo nome ja cadastrada
        //Nao grava nova loja duplicada para o mesmo usuario
        //Retorna: erro de regra de negocio
        public async Task CreateAsyncDeveImpedirCriacaoQuandoUsuarioJaPossuirLojaComMesmoNome()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            // Arrange
            // TODO: criar usuario com loja existente.
            // TODO: instanciar LojaService.
            // TODO: montar command com nome repetido para o mesmo usuario.

            // Act
            // TODO: executar CreateAsync.

            // Assert
            // TODO: validar excecao de conflito/regra de negocio.
            // TODO: validar que nenhuma nova loja foi persistida.
        }

        [Fact(Skip = MotivoPendente)]
        //Input: usuarios diferentes criando lojas com o mesmo nome
        //Permite nomes repetidos entre usuarios distintos
        //Retorna: loja criada para o segundo usuario
        public async Task CreateAsyncDevePermitirMesmoNomeParaUsuariosDiferentes()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            // Arrange
            // TODO: criar dois usuarios distintos.
            // TODO: persistir loja existente para o primeiro usuario.
            // TODO: instanciar LojaService para o segundo usuario.

            // Act
            // TODO: executar CreateAsync com o mesmo nome.

            // Assert
            // TODO: validar criacao da loja para o segundo usuario.
            // TODO: validar existencia de duas lojas com mesmo nome e usuarios diferentes.
        }
    }
}
