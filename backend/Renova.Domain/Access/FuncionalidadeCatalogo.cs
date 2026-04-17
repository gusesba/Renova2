namespace Renova.Domain.Access
{
    public sealed record FuncionalidadeCatalogItem(int Id, string Chave, string Grupo, string Descricao);

    public static class FuncionalidadeCatalogo
    {
        public const string ClientesVisualizar = "clientes.visualizar";
        public const string ClientesVisualizarDetalhe = "clientes.visualizar_detalhe";
        public const string ClientesAdicionar = "clientes.adicionar";
        public const string ClientesEditar = "clientes.editar";
        public const string ClientesExcluir = "clientes.excluir";
        public const string ClientesExportarFechamento = "clientes.exportar_fechamento";

        public const string ProdutosVisualizar = "produtos.visualizar";
        public const string ProdutosVisualizarItem = "produtos.visualizar_item";
        public const string ProdutosAdicionar = "produtos.adicionar";
        public const string ProdutosEditar = "produtos.editar";
        public const string ProdutosExcluir = "produtos.excluir";
        public const string ProdutosEmprestadosVisualizar = "produtos.emprestados.visualizar";
        public const string ProdutosAuxiliaresVisualizar = "produtos.auxiliares.visualizar";
        public const string ProdutosAuxiliaresAdicionarReferencia = "produtos.auxiliares.adicionar_referencia";
        public const string ProdutosAuxiliaresAdicionarMarca = "produtos.auxiliares.adicionar_marca";
        public const string ProdutosAuxiliaresAdicionarTamanho = "produtos.auxiliares.adicionar_tamanho";
        public const string ProdutosAuxiliaresAdicionarCor = "produtos.auxiliares.adicionar_cor";

        public const string SolicitacoesVisualizar = "solicitacoes.visualizar";
        public const string SolicitacoesAdicionar = "solicitacoes.adicionar";

        public const string MovimentacoesVisualizar = "movimentacoes.visualizar";
        public const string MovimentacoesAdicionar = "movimentacoes.adicionar";
        public const string MovimentacoesDestinacaoVisualizar = "movimentacoes.destinacao.visualizar";
        public const string MovimentacoesDestinacaoExecutar = "movimentacoes.destinacao.executar";

        public const string PagamentosVisualizar = "pagamentos.visualizar";
        public const string PagamentosManuaisAdicionar = "pagamentos.manuais.adicionar";
        public const string PagamentosCreditoVisualizar = "pagamentos.credito.visualizar";
        public const string PagamentosCreditoAdicionar = "pagamentos.credito.adicionar";
        public const string PagamentosCreditoResgatar = "pagamentos.credito.resgatar";
        public const string PagamentosPendenciasVisualizar = "pagamentos.pendencias.visualizar";
        public const string PagamentosPendenciasAtualizar = "pagamentos.pendencias.atualizar";
        public const string PagamentosFechamentoVisualizar = "pagamentos.fechamento.visualizar";

        public const string GastosLojaVisualizar = "gastos_loja.visualizar";
        public const string GastosLojaAdicionar = "gastos_loja.adicionar";

        public const string LojasVisualizar = "lojas.visualizar";
        public const string LojasAdicionar = "lojas.adicionar";
        public const string LojasEditar = "lojas.editar";
        public const string LojasExcluir = "lojas.excluir";

        public const string ConfigLojaVisualizar = "config_loja.visualizar";
        public const string ConfigLojaEditar = "config_loja.editar";

        public const string FuncionariosVisualizar = "funcionarios.visualizar";
        public const string FuncionariosAdicionar = "funcionarios.adicionar";
        public const string FuncionariosEditar = "funcionarios.editar";
        public const string FuncionariosRemover = "funcionarios.remover";

        public const string CargosVisualizar = "cargos.visualizar";
        public const string CargosAdicionar = "cargos.adicionar";
        public const string CargosEditar = "cargos.editar";
        public const string CargosExcluir = "cargos.excluir";

        public static IReadOnlyList<FuncionalidadeCatalogItem> Itens { get; } =
        [
            new(1, ClientesVisualizar, "Clientes", "Listar os clientes da loja."),
            new(2, ClientesVisualizarDetalhe, "Clientes", "Visualizar o detalhe e historico de um cliente."),
            new(3, ClientesAdicionar, "Clientes", "Cadastrar novos clientes."),
            new(4, ClientesEditar, "Clientes", "Editar clientes existentes."),
            new(5, ClientesExcluir, "Clientes", "Excluir clientes sem relacionamentos ativos."),
            new(6, ClientesExportarFechamento, "Clientes", "Exportar o fechamento de clientes."),
            new(7, ProdutosVisualizar, "Produtos", "Listar os produtos da loja."),
            new(8, ProdutosVisualizarItem, "Produtos", "Visualizar os dados de um produto."),
            new(9, ProdutosAdicionar, "Produtos", "Cadastrar novos produtos no estoque."),
            new(10, ProdutosEditar, "Produtos", "Editar produtos existentes."),
            new(11, ProdutosExcluir, "Produtos", "Excluir produtos sem movimentacoes."),
            new(12, ProdutosEmprestadosVisualizar, "Produtos", "Consultar produtos emprestados por cliente."),
            new(13, ProdutosAuxiliaresVisualizar, "Produtos", "Consultar referencias, marcas, tamanhos e cores."),
            new(14, ProdutosAuxiliaresAdicionarReferencia, "Produtos", "Cadastrar novas referencias de produto."),
            new(15, ProdutosAuxiliaresAdicionarMarca, "Produtos", "Cadastrar novas marcas."),
            new(16, ProdutosAuxiliaresAdicionarTamanho, "Produtos", "Cadastrar novos tamanhos."),
            new(17, ProdutosAuxiliaresAdicionarCor, "Produtos", "Cadastrar novas cores."),
            new(18, SolicitacoesVisualizar, "Solicitacoes", "Listar solicitacoes da loja."),
            new(19, SolicitacoesAdicionar, "Solicitacoes", "Cadastrar novas solicitacoes."),
            new(20, MovimentacoesVisualizar, "Movimentacoes", "Listar movimentacoes da loja."),
            new(21, MovimentacoesAdicionar, "Movimentacoes", "Registrar novas movimentacoes."),
            new(22, MovimentacoesDestinacaoVisualizar, "Movimentacoes", "Consultar sugestoes para doacao e devolucao."),
            new(23, MovimentacoesDestinacaoExecutar, "Movimentacoes", "Executar destinacoes de doacao e devolucao."),
            new(24, PagamentosVisualizar, "Pagamentos", "Listar pagamentos da loja."),
            new(25, PagamentosManuaisAdicionar, "Pagamentos", "Lancar pagamentos manuais."),
            new(26, PagamentosCreditoVisualizar, "Pagamentos", "Listar pagamentos de credito."),
            new(27, PagamentosCreditoAdicionar, "Pagamentos", "Adicionar credito para cliente."),
            new(28, PagamentosCreditoResgatar, "Pagamentos", "Resgatar credito de cliente."),
            new(29, PagamentosPendenciasVisualizar, "Pagamentos", "Visualizar pendencias de credito."),
            new(30, PagamentosPendenciasAtualizar, "Pagamentos", "Atualizar pendencias de credito."),
            new(31, PagamentosFechamentoVisualizar, "Pagamentos", "Visualizar o fechamento da loja."),
            new(32, GastosLojaVisualizar, "GastosLoja", "Listar gastos da loja."),
            new(33, GastosLojaAdicionar, "GastosLoja", "Cadastrar gastos e recebimentos da loja."),
            new(34, LojasVisualizar, "Lojas", "Visualizar as lojas acessiveis."),
            new(35, LojasAdicionar, "Lojas", "Cadastrar novas lojas."),
            new(36, LojasEditar, "Lojas", "Editar lojas existentes."),
            new(37, LojasExcluir, "Lojas", "Excluir lojas sem registros ativos."),
            new(38, ConfigLojaVisualizar, "ConfiguracaoLoja", "Visualizar configuracoes da loja."),
            new(39, ConfigLojaEditar, "ConfiguracaoLoja", "Editar configuracoes da loja."),
            new(40, FuncionariosVisualizar, "Funcionarios", "Listar funcionarios da loja."),
            new(41, FuncionariosAdicionar, "Funcionarios", "Vincular funcionarios a loja."),
            new(42, FuncionariosEditar, "Funcionarios", "Alterar o cargo de funcionarios."),
            new(43, FuncionariosRemover, "Funcionarios", "Remover funcionarios da loja."),
            new(44, CargosVisualizar, "Cargos", "Listar cargos e funcionalidades da loja."),
            new(45, CargosAdicionar, "Cargos", "Cadastrar novos cargos."),
            new(46, CargosEditar, "Cargos", "Editar cargos existentes."),
            new(47, CargosExcluir, "Cargos", "Excluir cargos sem funcionarios vinculados.")
        ];

        public static IReadOnlyList<string> TodasAsChaves { get; } = [.. Itens.Select(item => item.Chave)];

        public static FuncionalidadeCatalogItem ObterPorChave(string chave)
        {
            return Itens.SingleOrDefault(item => item.Chave == chave)
                ?? throw new KeyNotFoundException($"Funcionalidade {chave} nao foi encontrada no catalogo.");
        }
    }
}
