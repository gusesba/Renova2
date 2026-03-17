namespace Renova.Services.Features.Access;

// Centraliza o catalogo de permissoes e o unico cargo base automatico do sistema.
public static class AccessPermissionCodes
{
    public const string UsuariosVisualizar = "usuarios.visualizar";
    public const string UsuariosGerenciar = "usuarios.gerenciar";
    public const string CargosGerenciar = "cargos.gerenciar";
    public const string LojasGerenciar = "lojas.gerenciar";
    public const string PessoasVisualizar = "pessoas.visualizar";
    public const string PessoasGerenciar = "pessoas.gerenciar";
    public const string CatalogoGerenciar = "catalogo.gerenciar";
    public const string RegrasGerenciar = "regras.gerenciar";
    public const string PecasVisualizar = "pecas.visualizar";
    public const string PecasCadastrar = "pecas.cadastrar";
    public const string PecasAjustar = "pecas.ajustar";
    public const string VendasRegistrar = "vendas.registrar";
    public const string VendasCancelar = "vendas.cancelar";
    public const string CreditoVisualizar = "credito.visualizar";
    public const string CreditoGerenciar = "credito.gerenciar";
    public const string FinanceiroVisualizar = "financeiro.visualizar";
    public const string FinanceiroConciliar = "financeiro.conciliar";
    public const string FechamentoGerar = "fechamento.gerar";
    public const string FechamentoConferir = "fechamento.conferir";
    public const string RelatoriosExportar = "relatorios.exportar";
    public const string AlertasVisualizar = "alertas.visualizar";
    public const string PortalConsultar = "portal.consultar";
    public const string MobileConsultar = "mobile.consultar";

    public static readonly IReadOnlyList<PermissionDefinition> Catalog =
    [
        new(UsuariosVisualizar, "Visualizar usuarios", "Consulta usuarios e vinculos da loja.", "acesso"),
        new(UsuariosGerenciar, "Gerenciar usuarios", "Cria usuarios, altera status e mantem vinculos.", "acesso"),
        new(CargosGerenciar, "Gerenciar cargos", "Mantem cargos e matriz de permissoes por loja.", "acesso"),
        new(LojasGerenciar, "Gerenciar lojas", "Mantem cadastro e configuracao operacional da loja.", "lojas"),
        new(PessoasVisualizar, "Visualizar pessoas", "Consulta clientes e fornecedores.", "pessoas"),
        new(PessoasGerenciar, "Gerenciar pessoas", "Mantem cadastros de clientes e fornecedores.", "pessoas"),
        new(CatalogoGerenciar, "Gerenciar catalogo", "Mantem tabelas base compartilhadas.", "catalogo"),
        new(RegrasGerenciar, "Gerenciar regras comerciais", "Mantem regras comerciais e meios de pagamento.", "regras"),
        new(PecasVisualizar, "Visualizar pecas", "Consulta pecas, estoque e imagens.", "estoque"),
        new(PecasCadastrar, "Cadastrar pecas", "Cadastra e altera pecas do estoque.", "estoque"),
        new(PecasAjustar, "Ajustar estoque", "Realiza ajustes manuais de estoque.", "estoque"),
        new(VendasRegistrar, "Registrar vendas", "Abre e conclui vendas.", "vendas"),
        new(VendasCancelar, "Cancelar vendas", "Cancela vendas e executa estornos.", "vendas"),
        new(CreditoVisualizar, "Visualizar credito", "Consulta saldo e extrato de credito.", "credito"),
        new(CreditoGerenciar, "Gerenciar credito", "Lanca credito manual e ajustes.", "credito"),
        new(FinanceiroVisualizar, "Visualizar financeiro", "Consulta movimentos financeiros.", "financeiro"),
        new(FinanceiroConciliar, "Conciliar financeiro", "Executa conciliacao e lancamentos financeiros.", "financeiro"),
        new(FechamentoGerar, "Gerar fechamento", "Gera fechamento financeiro por pessoa.", "fechamento"),
        new(FechamentoConferir, "Conferir fechamento", "Confere e liquida fechamentos.", "fechamento"),
        new(RelatoriosExportar, "Exportar relatorios", "Exporta consultas em PDF e Excel.", "relatorios"),
        new(AlertasVisualizar, "Visualizar alertas", "Consulta alertas operacionais.", "alertas"),
        new(PortalConsultar, "Consultar portal", "Permite acesso ao portal do cliente/fornecedor.", "portal"),
        new(MobileConsultar, "Consultar mobile", "Permite acesso a consultas no mobile.", "mobile"),
    ];

    public static readonly IReadOnlyList<RoleTemplateDefinition> BaseRoleTemplates =
    [
        new(
            "dono_loja",
            "Dono da Loja",
            "Perfil base com acesso administrativo completo da loja.",
            Catalog.Select(x => x.Codigo).ToArray()),
    ];
}

// Representa uma permissao cadastravel no catalogo base.
public sealed record PermissionDefinition(string Codigo, string Nome, string Descricao, string Modulo);

// Representa a definicao de um cargo padrao criado automaticamente para a loja.
public sealed record RoleTemplateDefinition(
    string CodigoInterno,
    string Nome,
    string Descricao,
    IReadOnlyCollection<string> PermissionCodes);
