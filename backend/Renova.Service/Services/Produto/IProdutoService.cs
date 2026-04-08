using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Produto;
using Renova.Service.Parameters.Produto;
using Renova.Service.Queries.Produto;

namespace Renova.Service.Services.Produto
{
    public interface IProdutoService
    {
        Task<ProdutoDto> CreateAsync(CriarProdutoCommand request, CriarProdutoParametros parametros, CancellationToken cancellationToken = default);
        Task<PaginacaoDto<ProdutoBuscaDto>> GetAllAsync(ObterProdutosQuery request, ObterProdutosParametros parametros, CancellationToken cancellationToken = default);
        Task<PaginacaoDto<ProdutoAuxiliarDto>> GetCorAsync(ObterProdutoAuxiliarQuery request, ObterProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<ProdutoAuxiliarDto> CreateCorAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<PaginacaoDto<ProdutoAuxiliarDto>> GetMarcaAsync(ObterProdutoAuxiliarQuery request, ObterProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<ProdutoAuxiliarDto> CreateMarcaAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<PaginacaoDto<ProdutoAuxiliarDto>> GetProdutoAuxiliarAsync(ObterProdutoAuxiliarQuery request, ObterProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<ProdutoAuxiliarDto> CreateProdutoAuxiliarAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<PaginacaoDto<ProdutoAuxiliarDto>> GetTamanhoAsync(ObterProdutoAuxiliarQuery request, ObterProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
        Task<ProdutoAuxiliarDto> CreateTamanhoAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default);
    }
}
