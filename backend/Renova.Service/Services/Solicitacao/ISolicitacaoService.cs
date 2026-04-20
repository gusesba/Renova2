using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Solicitacao;
using Renova.Service.Parameters.Solicitacao;
using Renova.Service.Queries.Solicitacao;

namespace Renova.Service.Services.Solicitacao
{
    public interface ISolicitacaoService
    {
        Task<SolicitacaoDto> CreateAsync(CriarSolicitacaoCommand request, CriarSolicitacaoParametros parametros, CancellationToken cancellationToken = default);
        Task DeleteAsync(ExcluirSolicitacaoParametros parametros, CancellationToken cancellationToken = default);
        Task<PaginacaoDto<SolicitacaoBuscaDto>> GetAllAsync(ObterSolicitacoesQuery request, ObterSolicitacoesParametros parametros, CancellationToken cancellationToken = default);
    }
}
