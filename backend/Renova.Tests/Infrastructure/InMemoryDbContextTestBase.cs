using Microsoft.EntityFrameworkCore;

namespace Renova.Tests.Infrastructure
{
    public abstract class InMemoryDbContextTestBase<TContext>
        where TContext : DbContext
    {
        protected TContext CriarContextoEmMemoria()
        {
            DbContextOptions<TContext> options = new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return CriarContexto(options);
        }

        protected abstract TContext CriarContexto(DbContextOptions<TContext> options);
    }
}