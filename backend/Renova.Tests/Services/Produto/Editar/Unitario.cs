namespace Renova.Tests.Services.Produto.Editar
{
    public class Unitario
    {
        [Fact(Skip = "Esqueleto pendente: implementar EditarProdutoCommand/EditAsync no backend.")]
        public Task EditAsyncDeveEditarProdutoDaLojaDoUsuarioAutenticado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar EditarProdutoCommand/EditAsync no backend.")]
        public Task EditAsyncDeveAtualizarSituacaoDoProdutoQuandoPayloadForValido()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar EditarProdutoCommand/EditAsync no backend.")]
        public Task EditAsyncDeveAtualizarFlagConsignadoQuandoInformada()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar EditarProdutoCommand/EditAsync no backend.")]
        public Task EditAsyncDeveImpedirEdicaoQuandoFornecedorNaoPertencerALojaInformada()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar EditarProdutoCommand/EditAsync no backend.")]
        public Task EditAsyncDeveImpedirEdicaoQuandoTabelaAuxiliarNaoPertencerALojaInformada()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar EditarProdutoCommand/EditAsync no backend.")]
        public Task EditAsyncDeveImpedirEdicaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Esqueleto pendente: implementar EditarProdutoCommand/EditAsync no backend.")]
        public Task EditAsyncDeveFalharQuandoProdutoNaoForEncontrado()
        {
            return Task.CompletedTask;
        }
    }
}
