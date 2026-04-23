using Microsoft.EntityFrameworkCore;

using Renova.Domain.Access;
using Renova.Domain.Model;

namespace Renova.Persistence
{
    public class RenovaDbContext(DbContextOptions<RenovaDbContext> options) : DbContext(options)
    {
        public DbSet<RenovaModel> Renova { get; set; }
        public DbSet<UsuarioModel> Usuarios { get; set; }
        public DbSet<LojaModel> Lojas { get; set; }
        public DbSet<FuncionarioModel> Funcionarios { get; set; }
        public DbSet<CargoModel> Cargos { get; set; }
        public DbSet<FuncionalidadeModel> Funcionalidades { get; set; }
        public DbSet<CargoFuncionalidadeModel> CargosFuncionalidades { get; set; }
        public DbSet<ClienteModel> Clientes { get; set; }
        public DbSet<ClienteCreditoModel> ClientesCreditos { get; set; }
        public DbSet<ConfigLojaModel> ConfiguracoesLoja { get; set; }
        public DbSet<ConfigLojaDescontoPermanenciaModel> ConfiguracoesLojaDescontosPermanencia { get; set; }
        public DbSet<ConfigLojaFormaPagamentoModel> ConfiguracoesLojaFormasPagamento { get; set; }
        public DbSet<GastoLojaModel> GastosLoja { get; set; }
        public DbSet<PagamentoModel> Pagamentos { get; set; }
        public DbSet<PagamentoCreditoModel> PagamentosCredito { get; set; }
        public DbSet<ProdutoEstoqueModel> ProdutosEstoque { get; set; }
        public DbSet<SolicitacaoModel> Solicitacoes { get; set; }
        public DbSet<ProdutoReferenciaModel> ProdutosReferencia { get; set; }
        public DbSet<MovimentacaoModel> Movimentacoes { get; set; }
        public DbSet<MovimentacaoProdutoModel> MovimentacoesProdutos { get; set; }
        public DbSet<MarcaModel> Marcas { get; set; }
        public DbSet<TamanhoModel> Tamanhos { get; set; }
        public DbSet<CorModel> Cores { get; set; }

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

            _ = modelBuilder.Entity<FuncionarioModel>(entity =>
            {
                _ = entity.ToTable("Funcionario");
                _ = entity.HasKey(p => new { p.UsuarioId, p.LojaId });
                _ = entity.Property(p => p.UsuarioId).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.CargoId).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasIndex(p => p.CargoId);
                _ = entity.HasOne(p => p.Usuario)
                    .WithMany(p => p.Funcionarios)
                    .HasForeignKey(p => p.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Funcionarios)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(p => p.Cargo)
                    .WithMany(p => p.Funcionarios)
                    .HasForeignKey(p => p.CargoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<CargoModel>(entity =>
            {
                _ = entity.ToTable("Cargo");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasIndex(p => new { p.LojaId, p.Nome }).IsUnique();
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Cargos)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<FuncionalidadeModel>(entity =>
            {
                _ = entity.ToTable("Funcionalidade");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedNever();
                _ = entity.Property(p => p.Chave).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.Grupo).HasMaxLength(100).IsRequired();
                _ = entity.Property(p => p.Descricao).HasMaxLength(500).IsRequired();
                _ = entity.HasIndex(p => p.Chave).IsUnique();
                _ = entity.HasData(FuncionalidadeCatalogo.Itens.Select(item => new FuncionalidadeModel
                {
                    Id = item.Id,
                    Chave = item.Chave,
                    Grupo = item.Grupo,
                    Descricao = item.Descricao
                }));
            });

            _ = modelBuilder.Entity<CargoFuncionalidadeModel>(entity =>
            {
                _ = entity.ToTable("CargoFuncionalidade");
                _ = entity.HasKey(p => new { p.CargoId, p.FuncionalidadeId });
                _ = entity.Property(p => p.CargoId).IsRequired();
                _ = entity.Property(p => p.FuncionalidadeId).IsRequired();
                _ = entity.HasOne(p => p.Cargo)
                    .WithMany(p => p.Funcionalidades)
                    .HasForeignKey(p => p.CargoId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(p => p.Funcionalidade)
                    .WithMany(p => p.Cargos)
                    .HasForeignKey(p => p.FuncionalidadeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<ClienteModel>(entity =>
            {
                _ = entity.ToTable("Cliente");
                _ = entity.ToTable(t => t.HasCheckConstraint("CK_Cliente_Contato_ApenasNumeros", "\"Contato\" !~ '[^0-9]' AND length(\"Contato\") IN (10, 11)"));
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Nome).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.Contato).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.Obs).HasMaxLength(1000).IsRequired(false);
                _ = entity.Property(p => p.Doacao).IsRequired().HasDefaultValue(false);
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.UserId).IsRequired(false);
                _ = entity.HasIndex(p => p.LojaId);
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

            _ = modelBuilder.Entity<ConfigLojaModel>(entity =>
            {
                _ = entity.ToTable("ConfigLoja");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.PercentualRepasseFornecedor).HasPrecision(5, 2).IsRequired();
                _ = entity.Property(p => p.PercentualRepasseVendedorCredito).HasPrecision(5, 2).IsRequired();
                _ = entity.Property(p => p.TempoPermanenciaProdutoMeses).IsRequired();
                _ = entity.HasIndex(p => p.LojaId).IsUnique();
                _ = entity.HasOne(p => p.Loja)
                    .WithOne(p => p.ConfigLoja)
                    .HasForeignKey<ConfigLojaModel>(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<ConfigLojaDescontoPermanenciaModel>(entity =>
            {
                _ = entity.ToTable("ConfigLojaDescontoPermanencia");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.ConfigLojaId).IsRequired();
                _ = entity.Property(p => p.APartirDeMeses).IsRequired();
                _ = entity.Property(p => p.PercentualDesconto).HasPrecision(5, 2).IsRequired();
                _ = entity.HasIndex(p => new { p.ConfigLojaId, p.APartirDeMeses }).IsUnique();
                _ = entity.HasOne(p => p.ConfigLoja)
                    .WithMany(p => p.DescontosPermanencia)
                    .HasForeignKey(p => p.ConfigLojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<ConfigLojaFormaPagamentoModel>(entity =>
            {
                _ = entity.ToTable("ConfigLojaFormaPagamento");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.ConfigLojaId).IsRequired();
                _ = entity.Property(p => p.Nome).HasMaxLength(100).IsRequired();
                _ = entity.Property(p => p.PercentualAjuste).HasPrecision(5, 2).IsRequired();
                _ = entity.HasIndex(p => new { p.ConfigLojaId, p.Nome }).IsUnique();
                _ = entity.HasOne(p => p.ConfigLoja)
                    .WithMany(p => p.FormasPagamento)
                    .HasForeignKey(p => p.ConfigLojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<ClienteCreditoModel>(entity =>
            {
                _ = entity.ToTable("ClienteCredito");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.ClienteId).IsRequired();
                _ = entity.Property(p => p.Valor).HasPrecision(18, 2).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasIndex(p => p.ClienteId).IsUnique();
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.CreditosClientes)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(p => p.Cliente)
                    .WithOne(p => p.Credito)
                    .HasForeignKey<ClienteCreditoModel>(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<ProdutoReferenciaModel>(entity =>
            {
                _ = entity.ToTable("ProdutoReferencia");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Valor).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasIndex(p => new { p.LojaId, p.Valor }).IsUnique();
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.ProdutosReferencia)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<MarcaModel>(entity =>
            {
                _ = entity.ToTable("Marca");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Valor).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasIndex(p => new { p.LojaId, p.Valor }).IsUnique();
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Marcas)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<TamanhoModel>(entity =>
            {
                _ = entity.ToTable("Tamanho");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Valor).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasIndex(p => new { p.LojaId, p.Valor }).IsUnique();
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Tamanhos)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<CorModel>(entity =>
            {
                _ = entity.ToTable("Cor");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Valor).HasMaxLength(200).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasIndex(p => new { p.LojaId, p.Valor }).IsUnique();
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Cores)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<ProdutoEstoqueModel>(entity =>
            {
                _ = entity.ToTable("ProdutoEstoque");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Preco).HasPrecision(18, 2).IsRequired();
                _ = entity.Property(p => p.ProdutoId).IsRequired();
                _ = entity.Property(p => p.MarcaId).IsRequired();
                _ = entity.Property(p => p.TamanhoId).IsRequired();
                _ = entity.Property(p => p.CorId).IsRequired();
                _ = entity.Property(p => p.FornecedorId).IsRequired();
                _ = entity.Property(p => p.Descricao).HasMaxLength(1000).IsRequired();
                _ = entity.Property(p => p.Entrada).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.Situacao).HasConversion<int>().IsRequired();
                _ = entity.Property(p => p.Consignado).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasOne(p => p.Produto)
                    .WithMany(p => p.ProdutosEstoque)
                    .HasForeignKey(p => p.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Marca)
                    .WithMany(p => p.ProdutosEstoque)
                    .HasForeignKey(p => p.MarcaId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Tamanho)
                    .WithMany(p => p.ProdutosEstoque)
                    .HasForeignKey(p => p.TamanhoId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Cor)
                    .WithMany(p => p.ProdutosEstoque)
                    .HasForeignKey(p => p.CorId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Fornecedor)
                    .WithMany(p => p.ProdutosFornecidos)
                    .HasForeignKey(p => p.FornecedorId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.ProdutosEstoque)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<SolicitacaoModel>(entity =>
            {
                _ = entity.ToTable("Solicitacao");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.ProdutoId).IsRequired(false);
                _ = entity.Property(p => p.MarcaId).IsRequired(false);
                _ = entity.Property(p => p.TamanhoId).IsRequired(false);
                _ = entity.Property(p => p.CorId).IsRequired(false);
                _ = entity.Property(p => p.ClienteId).IsRequired(false);
                _ = entity.Property(p => p.Descricao).HasMaxLength(1000).IsRequired(false);
                _ = entity.Property(p => p.PrecoMinimo).HasPrecision(18, 2).IsRequired(false);
                _ = entity.Property(p => p.PrecoMaximo).HasPrecision(18, 2).IsRequired(false);
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasOne(p => p.Produto)
                    .WithMany()
                    .HasForeignKey(p => p.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Marca)
                    .WithMany()
                    .HasForeignKey(p => p.MarcaId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Tamanho)
                    .WithMany()
                    .HasForeignKey(p => p.TamanhoId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Cor)
                    .WithMany()
                    .HasForeignKey(p => p.CorId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Cliente)
                    .WithMany(p => p.Solicitacoes)
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Solicitacoes)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<MovimentacaoModel>(entity =>
            {
                _ = entity.ToTable("Movimentacao");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.Tipo).HasConversion<int>().IsRequired();
                _ = entity.Property(p => p.Data).IsRequired();
                _ = entity.Property(p => p.ClienteId).IsRequired();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasOne(p => p.Cliente)
                    .WithMany(p => p.Movimentacoes)
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Movimentacoes)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<GastoLojaModel>(entity =>
            {
                _ = entity.ToTable("GastoLoja");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.Natureza).HasConversion<int>().IsRequired();
                _ = entity.Property(p => p.Valor).HasPrecision(18, 2).IsRequired();
                _ = entity.Property(p => p.Data).IsRequired();
                _ = entity.Property(p => p.Descricao).HasMaxLength(500).IsRequired(false);
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.GastosLoja)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<PagamentoModel>(entity =>
            {
                _ = entity.ToTable("Pagamento");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.MovimentacaoId).IsRequired(false);
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.ClienteId).IsRequired();
                _ = entity.Property(p => p.Natureza).HasConversion<int>().IsRequired();
                _ = entity.Property(p => p.Status).HasConversion<int>().IsRequired();
                _ = entity.Property(p => p.Descricao).HasMaxLength(500).IsRequired(false);
                _ = entity.Property(p => p.Valor).HasPrecision(18, 2).IsRequired();
                _ = entity.Property(p => p.Data).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasOne(p => p.Movimentacao)
                    .WithMany(p => p.Pagamentos)
                    .HasForeignKey(p => p.MovimentacaoId)
                    .OnDelete(DeleteBehavior.SetNull);
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.Pagamentos)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(p => p.Cliente)
                    .WithMany(p => p.Pagamentos)
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<PagamentoCreditoModel>(entity =>
            {
                _ = entity.ToTable("PagamentoCredito");
                _ = entity.HasKey(p => p.Id);
                _ = entity.Property(p => p.Id).ValueGeneratedOnAdd();
                _ = entity.Property(p => p.LojaId).IsRequired();
                _ = entity.Property(p => p.ClienteId).IsRequired();
                _ = entity.Property(p => p.Tipo).HasConversion<int>().IsRequired();
                _ = entity.Property(p => p.ConfigLojaFormaPagamentoId).IsRequired(false);
                _ = entity.Property(p => p.ValorCredito).HasPrecision(18, 2).IsRequired();
                _ = entity.Property(p => p.ValorDinheiro).HasPrecision(18, 2).IsRequired();
                _ = entity.Property(p => p.Data).IsRequired();
                _ = entity.HasIndex(p => p.LojaId);
                _ = entity.HasIndex(p => p.ConfigLojaFormaPagamentoId);
                _ = entity.HasOne(p => p.Loja)
                    .WithMany(p => p.PagamentosCredito)
                    .HasForeignKey(p => p.LojaId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(p => p.Cliente)
                    .WithMany(p => p.PagamentosCredito)
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                _ = entity.HasOne(p => p.ConfigLojaFormaPagamento)
                    .WithMany()
                    .HasForeignKey(p => p.ConfigLojaFormaPagamentoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            _ = modelBuilder.Entity<MovimentacaoProdutoModel>(entity =>
            {
                _ = entity.ToTable("MovimentacaoProduto");
                _ = entity.HasKey(p => new { p.MovimentacaoId, p.ProdutoId });
                _ = entity.Property(p => p.MovimentacaoId).IsRequired();
                _ = entity.Property(p => p.ProdutoId).IsRequired();
                _ = entity.Property(p => p.Desconto).HasPrecision(5, 2).IsRequired();
                _ = entity.HasOne(p => p.Movimentacao)
                    .WithMany(p => p.Produtos)
                    .HasForeignKey(p => p.MovimentacaoId)
                    .OnDelete(DeleteBehavior.Cascade);
                _ = entity.HasOne(p => p.Produto)
                    .WithMany(p => p.Movimentacoes)
                    .HasForeignKey(p => p.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
