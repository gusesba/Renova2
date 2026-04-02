using Microsoft.EntityFrameworkCore;
using Renova.Persistence;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Auth.Cadastro
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        //Input: x
        //Grava usuário no banco com senha hash
        //Retorna usuário e token
        public async Task CreateAsync_DeveSalvarComSenhaHashERetornarUsuarioToken()
        {

        }

        [Fact]
        //Input: email já cadastrado
        //Não grava novo usuário no banco
        //Retorna: erro de regra de negócio
        public async Task CreateAsync_DeveImpedirCadastroQuandoEmailJaExistir()
        {

        }
    }
}
