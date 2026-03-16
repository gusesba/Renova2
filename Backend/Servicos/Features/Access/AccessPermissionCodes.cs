namespace Renova.Services.Features.Access;

// Representa o catalogo central de permissoes e papeis base do sistema.
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
        new(UsuariosVisualizar, "Visualizar usuários", "Consulta usuários e vínculos da loja.", "acesso"),
        new(UsuariosGerenciar, "Gerenciar usuários", "Cria usuários, altera status e mantém vínculos.", "acesso"),
        new(CargosGerenciar, "Gerenciar cargos", "Mantém cargos e matriz de permissões por loja.", "acesso"),
        new(LojasGerenciar, "Gerenciar lojas", "Mantém cadastro e configuração operacional da loja.", "lojas"),
        new(PessoasVisualizar, "Visualizar pessoas", "Consulta clientes e fornecedores.", "pessoas"),
        new(PessoasGerenciar, "Gerenciar pessoas", "Mantém cadastros de clientes e fornecedores.", "pessoas"),
        new(CatalogoGerenciar, "Gerenciar catálogo", "Mantém tabelas base compartilhadas.", "catalogo"),
        new(RegrasGerenciar, "Gerenciar regras comerciais", "Mantém regras comerciais e meios de pagamento.", "regras"),
        new(PecasVisualizar, "Visualizar peças", "Consulta peças, estoque e imagens.", "estoque"),
        new(PecasCadastrar, "Cadastrar peças", "Cadastra e altera peças do estoque.", "estoque"),
        new(PecasAjustar, "Ajustar estoque", "Realiza ajustes manuais de estoque.", "estoque"),
        new(VendasRegistrar, "Registrar vendas", "Abre e conclui vendas.", "vendas"),
        new(VendasCancelar, "Cancelar vendas", "Cancela vendas e executa estornos.", "vendas"),
        new(CreditoVisualizar, "Visualizar crédito", "Consulta saldo e extrato de crédito.", "credito"),
        new(CreditoGerenciar, "Gerenciar crédito", "Lança crédito manual e ajustes.", "credito"),
        new(FinanceiroVisualizar, "Visualizar financeiro", "Consulta movimentos financeiros.", "financeiro"),
        new(FinanceiroConciliar, "Conciliar financeiro", "Executa conciliação e lançamentos financeiros.", "financeiro"),
        new(FechamentoGerar, "Gerar fechamento", "Gera fechamento financeiro por pessoa.", "fechamento"),
        new(FechamentoConferir, "Conferir fechamento", "Confere e liquida fechamentos.", "fechamento"),
        new(RelatoriosExportar, "Exportar relatórios", "Exporta consultas em PDF e Excel.", "relatorios"),
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
        new(
            "gerente",
            "Gerente",
            "Perfil base para gestão operacional com pouca restrição.",
            [
                UsuariosVisualizar,
                PessoasVisualizar,
                PessoasGerenciar,
                CatalogoGerenciar,
                RegrasGerenciar,
                PecasVisualizar,
                PecasCadastrar,
                PecasAjustar,
                VendasRegistrar,
                VendasCancelar,
                CreditoVisualizar,
                CreditoGerenciar,
                FinanceiroVisualizar,
                FinanceiroConciliar,
                FechamentoGerar,
                FechamentoConferir,
                RelatoriosExportar,
                AlertasVisualizar,
            ]),
        new(
            "funcionario",
            "Funcionário",
            "Perfil base para operação diária da loja.",
            [
                UsuariosVisualizar,
                PessoasVisualizar,
                PecasVisualizar,
                PecasCadastrar,
                VendasRegistrar,
                CreditoVisualizar,
                AlertasVisualizar,
            ]),
        new(
            "cliente",
            "Cliente",
            "Perfil base para portal e consultas próprias.",
            [
                PortalConsultar,
                MobileConsultar,
            ]),
    ];
}

// Representa uma permissao cadastravel no catalogo base.
public sealed record PermissionDefinition(string Codigo, string Nome, string Descricao, string Modulo);

// Representa a definicao de um cargo padrao para bootstrap de lojas.
public sealed record RoleTemplateDefinition(
    string CodigoInterno,
    string Nome,
    string Descricao,
    IReadOnlyCollection<string> PermissionCodes);
