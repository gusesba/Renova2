using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Produto;
using Renova.Service.Parameters.Produto;

namespace Renova.Service.Services.Produto
{
    public interface IProdutoService
    {
        Task<ProdutoDto> CreateAsync(CriarProdutoCommand request, CriarProdutoParametros parametros, CancellationToken cancellationToken = default);
        Task<ProdutoAuxiliarDto> CreateCorAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<ProdutoAuxiliarDto> CreateMarcaAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<ProdutoAuxiliarDto> CreateProdutoAuxiliarAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<ProdutoAuxiliarDto> CreateTamanhoAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
    }
}
