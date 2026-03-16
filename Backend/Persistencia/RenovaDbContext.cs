using Microsoft.EntityFrameworkCore;

namespace Renova.Persistence
{
    public class RenovaDbContext : DbContext
    {
        public RenovaDbContext(DbContextOptions<RenovaDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}