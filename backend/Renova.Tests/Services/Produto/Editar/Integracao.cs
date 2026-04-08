namespace Renova.Tests.Services.Produto.Editar
{
    public class Integracao
    {
        [Fact(Skip = "Esqueleto pendente: implementar endpoint PUT /api/produto/{id} no backend.")]
        public Task PutProdutoDeveRetornarOkQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar endpoint PUT /api/produto/{id} no backend.")]
        public Task PutProdutoDeveRetornarOkQuandoAtualizarSituacaoEConsignado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar endpoint PUT /api/produto/{id} no backend.")]
        public Task PutProdutoDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar endpoint PUT /api/produto/{id} no backend.")]
        public Task PutProdutoDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar endpoint PUT /api/produto/{id} no backend.")]
        public Task PutProdutoDeveRetornarBadRequestQuandoFornecedorNaoPertencerALojaInformada()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar endpoint PUT /api/produto/{id} no backend.")]
        public Task PutProdutoDeveRetornarBadRequestQuandoTabelaAuxiliarNaoPertencerALojaInformada()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar endpoint PUT /api/produto/{id} no backend.")]
        public Task PutProdutoDeveRetornarNotFoundQuandoProdutoNaoForEncontrado()
        {
            return Task.CompletedTask;
        }
    }
}
