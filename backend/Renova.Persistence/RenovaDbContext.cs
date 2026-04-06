using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;

namespace Renova.Persistence
{
    public class RenovaDbContext(DbContextOptions<RenovaDbContext> options) : DbContext(options)
    {
        public DbSet<RenovaModel> Renova { get; set; }
        public DbSet<UsuarioModel> Usuarios { get; set; }
        public DbSet<LojaModel> Lojas { get; set; }
        public DbSet<ClienteModel> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<RenovaModel>(entity =>
            {
                _ = entity.ToTable("Renova");
                _ = entity.HasKey(p => p.Campo1);
                _ = entity.Property(p => p.Campo1).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Campo2).HasMaxLength(500).IsRequired();
                _ = entity.Property(p => p.Campo3);
                _ = entity.HasIndex(p => p.Campo1);
            });

            _ = modelBuilder.Entity<UsuarioModel>(entity =>
            {
                _ = entity.ToTable("Usuario");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.Email).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.SenhaHash).HasMaxLength(500).IsRequired();
                _ = entity.HasIndex(p => p.Email).IsUnique();
            });

            _ = modelBuilder.Entity<LojaModel>(entity =>
            {
                _ = entity.ToTable("Loja");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.UsuarioId).IsRequired();
                _ = entity.HasIndex(p => new { p.UsuarioId, p.Nome }).IsUnique();
                _ = entity.HasOne(p => p.Usuario)
                    .WithMany(p => p.Lojas)
                    .HasForeignKey(p => p.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<ClienteModel>(entity =>
            {
                _ = entity.ToTable("Cliente");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.Contato).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.UserId).IsRequired(false);
                _ = entity.HasIndex(p => new { p.LojaId, p.Nome }).IsUnique();
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Clientes)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(p => p.User)
                    .WithMany(p => p.Clientes)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
