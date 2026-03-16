using System.Text;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;

namespace Renova.Persistence;

public class RenovaDbContext : DbContext
{
    public RenovaDbContext(DbContextOptions<RenovaDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioSessao> UsuarioSessoes => Set<UsuarioSessao>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<CargoPermissao> CargoPermissoes => Set<CargoPermissao>();
    public DbSet<UsuarioLoja> UsuarioLojas => Set<UsuarioLoja>();
    public DbSet<UsuarioLojaCargo> UsuarioLojaCargos => Set<UsuarioLojaCargo>();
    public DbSet<UsuarioAcessoEvento> UsuarioAcessoEventos => Set<UsuarioAcessoEvento>();
    public DbSet<AuditoriaEvento> AuditoriaEventos => Set<AuditoriaEvento>();

    public DbSet<Loja> Lojas => Set<Loja>();
    public DbSet<LojaConfiguracao> LojaConfiguracoes => Set<LojaConfiguracao>();
    public DbSet<ConjuntoCatalogo> ConjuntoCatalogos => Set<ConjuntoCatalogo>();

    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<PessoaLoja> PessoaLojas => Set<PessoaLoja>();
    public DbSet<PessoaContaBancaria> PessoaContasBancarias => Set<PessoaContaBancaria>();

    public DbSet<ProdutoNome> ProdutoNomes => Set<ProdutoNome>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Tamanho> Tamanhos => Set<Tamanho>();
    public DbSet<Cor> Cores => Set<Cor>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Colecao> Colecoes => Set<Colecao>();

    public DbSet<LojaRegraComercial> LojaRegrasComerciais => Set<LojaRegraComercial>();
    public DbSet<FornecedorRegraComercial> FornecedorRegrasComerciais => Set<FornecedorRegraComercial>();
    public DbSet<MeioPagamento> MeiosPagamento => Set<MeioPagamento>();

    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<PecaCondicaoComercial> PecaCondicoesComerciais => Set<PecaCondicaoComercial>();
    public DbSet<PecaImagem> PecaImagens => Set<PecaImagem>();
    public DbSet<PecaHistoricoPreco> PecaHistoricosPreco => Set<PecaHistoricoPreco>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();

    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<VendaItem> VendaItens => Set<VendaItem>();
    public DbSet<VendaPagamento> VendaPagamentos => Set<VendaPagamento>();

    public DbSet<ContaCreditoLoja> ContasCreditoLoja => Set<ContaCreditoLoja>();
    public DbSet<MovimentacaoCreditoLoja> MovimentacoesCreditoLoja => Set<MovimentacaoCreditoLoja>();

    public DbSet<ObrigacaoFornecedor> ObrigacoesFornecedor => Set<ObrigacaoFornecedor>();
    public DbSet<LiquidacaoObrigacaoFornecedor> LiquidacoesObrigacaoFornecedor => Set<LiquidacaoObrigacaoFornecedor>();
    public DbSet<MovimentacaoFinanceira> MovimentacoesFinanceiras => Set<MovimentacaoFinanceira>();

    public DbSet<FechamentoPessoa> FechamentosPessoa => Set<FechamentoPessoa>();
    public DbSet<FechamentoPessoaItem> FechamentoPessoaItens => Set<FechamentoPessoaItem>();
    public DbSet<FechamentoPessoaMovimento> FechamentoPessoaMovimentos => Set<FechamentoPessoaMovimento>();
    public DbSet<AlertaOperacional> AlertasOperacionais => Set<AlertaOperacional>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyNamingConventions(modelBuilder);
        ApplyCommonConventions(modelBuilder);

        ConfigureAcesso(modelBuilder);
        ConfigureLojas(modelBuilder);
        ConfigurePessoas(modelBuilder);
        ConfigureCatalogos(modelBuilder);
        ConfigureRegrasComerciais(modelBuilder);
        ConfigureEstoque(modelBuilder);
        ConfigureVendas(modelBuilder);
        ConfigureCredito(modelBuilder);
        ConfigureFinanceiro(modelBuilder);
        ConfigureFechamento(modelBuilder);
        ConfigureAlertas(modelBuilder);
    }

    private static void ApplyNamingConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null)
            {
                continue;
            }

            var entityBuilder = modelBuilder.Entity(clrType);
            entityBuilder.ToTable(ToSnakeCase(clrType.Name));

            foreach (var property in entityType.GetProperties())
            {
                entityBuilder.Property(property.Name).HasColumnName(ToSnakeCase(property.Name));
            }
        }
    }

    private static void ApplyCommonConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null)
            {
                continue;
            }

            var entityBuilder = modelBuilder.Entity(clrType);

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    var precision = property.Name.Contains("Percentual", StringComparison.OrdinalIgnoreCase) ? 5 : 18;
                    entityBuilder.Property(property.Name).HasPrecision(precision, 2);
                }

                if (property.Name == nameof(AuditEntityBase.RowVersion))
                {
                    entityBuilder.Property(property.Name).IsConcurrencyToken();
                }
            }
        }
    }

    private static void ConfigureAcesso(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.PessoaId).IsUnique();

            entity.HasOne<Pessoa>()
                .WithMany()
                .HasForeignKey(x => x.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsuarioSessao>(entity =>
        {
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CargoPermissao>(entity =>
        {
            entity.HasIndex(x => new { x.CargoId, x.PermissaoId }).IsUnique();

            entity.HasOne<Cargo>()
                .WithMany()
                .HasForeignKey(x => x.CargoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Permissao>()
                .WithMany()
                .HasForeignKey(x => x.PermissaoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsuarioLoja>(entity =>
        {
            entity.HasIndex(x => new { x.UsuarioId, x.LojaId }).IsUnique();

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsuarioLojaCargo>(entity =>
        {
            entity.HasIndex(x => new { x.UsuarioLojaId, x.CargoId }).IsUnique();

            entity.HasOne<UsuarioLoja>()
                .WithMany()
                .HasForeignKey(x => x.UsuarioLojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Cargo>()
                .WithMany()
                .HasForeignKey(x => x.CargoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsuarioAcessoEvento>(entity =>
        {
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditoriaEvento>(entity =>
        {
            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureLojas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loja>(entity =>
        {
            entity.HasIndex(x => x.Documento).IsUnique();

            entity.HasOne<ConjuntoCatalogo>()
                .WithMany()
                .HasForeignKey(x => x.ConjuntoCatalogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LojaConfiguracao>(entity =>
        {
            entity.HasIndex(x => x.LojaId).IsUnique();

            entity.HasOne<Loja>()
                .WithOne()
                .HasForeignKey<LojaConfiguracao>(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePessoas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasIndex(x => x.Documento);
        });

        modelBuilder.Entity<PessoaLoja>(entity =>
        {
            entity.HasIndex(x => new { x.PessoaId, x.LojaId }).IsUnique();

            entity.HasOne<Pessoa>()
                .WithMany()
                .HasForeignKey(x => x.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PessoaContaBancaria>(entity =>
        {
            entity.HasOne<Pessoa>()
                .WithMany()
                .HasForeignKey(x => x.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCatalogos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProdutoNome>(entity =>
        {
            entity.HasOne<ConjuntoCatalogo>()
                .WithMany()
                .HasForeignKey(x => x.ConjuntoCatalogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasOne<ConjuntoCatalogo>()
                .WithMany()
                .HasForeignKey(x => x.ConjuntoCatalogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tamanho>(entity =>
        {
            entity.HasOne<ConjuntoCatalogo>()
                .WithMany()
                .HasForeignKey(x => x.ConjuntoCatalogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cor>(entity =>
        {
            entity.HasOne<ConjuntoCatalogo>()
                .WithMany()
                .HasForeignKey(x => x.ConjuntoCatalogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasOne<ConjuntoCatalogo>()
                .WithMany()
                .HasForeignKey(x => x.ConjuntoCatalogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Colecao>(entity =>
        {
            entity.HasOne<ConjuntoCatalogo>()
                .WithMany()
                .HasForeignKey(x => x.ConjuntoCatalogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRegrasComerciais(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LojaRegraComercial>(entity =>
        {
            entity.HasIndex(x => x.LojaId).IsUnique();

            entity.HasOne<Loja>()
                .WithOne()
                .HasForeignKey<LojaRegraComercial>(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FornecedorRegraComercial>(entity =>
        {
            entity.HasIndex(x => x.PessoaLojaId).IsUnique();

            entity.HasOne<PessoaLoja>()
                .WithOne()
                .HasForeignKey<FornecedorRegraComercial>(x => x.PessoaLojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MeioPagamento>(entity =>
        {
            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEstoque(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Peca>(entity =>
        {
            entity.HasIndex(x => new { x.LojaId, x.CodigoInterno }).IsUnique();
            entity.HasIndex(x => x.CodigoBarras);
            entity.HasIndex(x => new { x.LojaId, x.StatusPeca, x.DataEntrada });

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Pessoa>()
                .WithMany()
                .HasForeignKey(x => x.FornecedorPessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ProdutoNome>()
                .WithMany()
                .HasForeignKey(x => x.ProdutoNomeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Marca>()
                .WithMany()
                .HasForeignKey(x => x.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Tamanho>()
                .WithMany()
                .HasForeignKey(x => x.TamanhoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Cor>()
                .WithMany()
                .HasForeignKey(x => x.CorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Categoria>()
                .WithMany()
                .HasForeignKey(x => x.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Colecao>()
                .WithMany()
                .HasForeignKey(x => x.ColecaoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.ResponsavelCadastroUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PecaCondicaoComercial>(entity =>
        {
            entity.HasIndex(x => x.PecaId).IsUnique();

            entity.HasOne<Peca>()
                .WithOne()
                .HasForeignKey<PecaCondicaoComercial>(x => x.PecaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PecaImagem>(entity =>
        {
            entity.HasOne<Peca>()
                .WithMany()
                .HasForeignKey(x => x.PecaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PecaHistoricoPreco>(entity =>
        {
            entity.HasOne<Peca>()
                .WithMany()
                .HasForeignKey(x => x.PecaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.AlteradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.HasIndex(x => new { x.PecaId, x.MovimentadoEm });

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Peca>()
                .WithMany()
                .HasForeignKey(x => x.PecaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.MovimentadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureVendas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venda>(entity =>
        {
            entity.HasIndex(x => new { x.LojaId, x.NumeroVenda }).IsUnique();
            entity.HasIndex(x => new { x.LojaId, x.DataHoraVenda });

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.VendedorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Pessoa>()
                .WithMany()
                .HasForeignKey(x => x.CompradorPessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.CanceladaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendaItem>(entity =>
        {
            entity.HasIndex(x => x.PecaId);

            entity.HasOne<Venda>()
                .WithMany()
                .HasForeignKey(x => x.VendaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Peca>()
                .WithMany()
                .HasForeignKey(x => x.PecaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendaPagamento>(entity =>
        {
            entity.HasOne<Venda>()
                .WithMany()
                .HasForeignKey(x => x.VendaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MeioPagamento>()
                .WithMany()
                .HasForeignKey(x => x.MeioPagamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ContaCreditoLoja>()
                .WithMany()
                .HasForeignKey(x => x.ContaCreditoLojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCredito(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContaCreditoLoja>(entity =>
        {
            entity.HasIndex(x => new { x.LojaId, x.PessoaId }).IsUnique();

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Pessoa>()
                .WithMany()
                .HasForeignKey(x => x.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimentacaoCreditoLoja>(entity =>
        {
            entity.HasOne<ContaCreditoLoja>()
                .WithMany()
                .HasForeignKey(x => x.ContaCreditoLojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.MovimentadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFinanceiro(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ObrigacaoFornecedor>(entity =>
        {
            entity.HasIndex(x => new { x.LojaId, x.PessoaId, x.StatusObrigacao });

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Pessoa>()
                .WithMany()
                .HasForeignKey(x => x.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<VendaItem>()
                .WithMany()
                .HasForeignKey(x => x.VendaItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Peca>()
                .WithMany()
                .HasForeignKey(x => x.PecaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LiquidacaoObrigacaoFornecedor>(entity =>
        {
            entity.HasOne<ObrigacaoFornecedor>()
                .WithMany()
                .HasForeignKey(x => x.ObrigacaoFornecedorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MeioPagamento>()
                .WithMany()
                .HasForeignKey(x => x.MeioPagamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ContaCreditoLoja>()
                .WithMany()
                .HasForeignKey(x => x.ContaCreditoLojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.LiquidadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimentacaoFinanceira>(entity =>
        {
            entity.HasIndex(x => new { x.LojaId, x.MovimentadoEm });

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MeioPagamento>()
                .WithMany()
                .HasForeignKey(x => x.MeioPagamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<VendaPagamento>()
                .WithMany()
                .HasForeignKey(x => x.VendaPagamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<LiquidacaoObrigacaoFornecedor>()
                .WithMany()
                .HasForeignKey(x => x.LiquidacaoObrigacaoFornecedorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.MovimentadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFechamento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FechamentoPessoa>(entity =>
        {
            entity.HasIndex(x => new { x.LojaId, x.PessoaId, x.PeriodoInicio, x.PeriodoFim });

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Pessoa>()
                .WithMany()
                .HasForeignKey(x => x.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.GeradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(x => x.ConferidoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FechamentoPessoaItem>(entity =>
        {
            entity.HasOne<FechamentoPessoa>()
                .WithMany()
                .HasForeignKey(x => x.FechamentoPessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Peca>()
                .WithMany()
                .HasForeignKey(x => x.PecaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FechamentoPessoaMovimento>(entity =>
        {
            entity.HasOne<FechamentoPessoa>()
                .WithMany()
                .HasForeignKey(x => x.FechamentoPessoaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAlertas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertaOperacional>(entity =>
        {
            entity.HasIndex(x => new { x.LojaId, x.StatusAlerta, x.Severidade });

            entity.HasOne<Loja>()
                .WithMany()
                .HasForeignKey(x => x.LojaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var currentChar = value[i];
            var previousChar = i > 0 ? value[i - 1] : '\0';
            var nextChar = i < value.Length - 1 ? value[i + 1] : '\0';

            if (char.IsUpper(currentChar))
            {
                var shouldAddUnderscore =
                    i > 0 &&
                    (char.IsLower(previousChar) ||
                     char.IsDigit(previousChar) ||
                     (char.IsUpper(previousChar) && char.IsLower(nextChar)));

                if (shouldAddUnderscore)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(currentChar));
            }
            else
            {
                builder.Append(currentChar);
            }
        }

        return builder.ToString();
    }
}
