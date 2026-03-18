using Renova.Services.Features.Pieces.Contracts;

namespace Renova.Services.Features.Pieces.Abstractions;

// Define os casos de uso do modulo 06 para API e consumidores internos.
public interface IPieceService
{
    /// <summary>
    /// Carrega os cadastros e opcoes usados no modulo da loja ativa.
    /// </summary>
    Task<PieceWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista as pecas da loja ativa com filtros rapidos.
    /// </summary>
    Task<IReadOnlyList<PieceSummaryResponse>> ListarAsync(PieceListQueryRequest query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega o detalhe completo de uma peca da loja ativa.
    /// </summary>
    Task<PieceDetailResponse> ObterDetalheAsync(Guid pecaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma peca, congela a regra comercial e gera a entrada inicial de estoque.
    /// </summary>
    Task<PieceDetailResponse> CriarAsync(CreatePieceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os dados cadastrais da peca e o snapshot comercial vigente.
    /// </summary>
    Task<PieceDetailResponse> AtualizarAsync(Guid pecaId, UpdatePieceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra uma imagem ja armazenada para a peca informada.
    /// </summary>
    Task<PieceImageResponse> AdicionarImagemAsync(Guid pecaId, RegisterPieceImageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza metadados de uma imagem vinculada a peca.
    /// </summary>
    Task<PieceImageResponse> AtualizarImagemAsync(Guid pecaId, Guid imagemId, UpdatePieceImageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove o vinculo de uma imagem da peca e retorna os dados removidos.
    /// </summary>
    Task<PieceImageResponse> RemoverImagemAsync(Guid pecaId, Guid imagemId, CancellationToken cancellationToken = default);
}
