using Microsoft.EntityFrameworkCore;
using Renova.Persistence;
using Renova.Services.Features.Pieces;
using Renova.Services.Features.StockMovements.Abstractions;
using Renova.Services.Features.StockMovements.Contracts;

namespace Renova.Services.Features.StockMovements.Services;

// Centraliza a regra que impede vendas sem saldo ou com peca indisponivel.
public sealed class StockAvailabilityService : IStockAvailabilityService
{
    private readonly RenovaDbContext _dbContext;

    /// <summary>
    /// Inicializa o service com acesso ao contexto de persistencia.
    /// </summary>
    public StockAvailabilityService(RenovaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Garante a disponibilidade de uma unica peca para venda.
    /// </summary>
    public Task EnsureSaleAvailabilityAsync(
        Guid lojaId,
        StockSaleAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        return EnsureSaleAvailabilityAsync(lojaId, [request], cancellationToken);
    }

    /// <summary>
    /// Garante a disponibilidade de todas as pecas informadas para venda.
    /// </summary>
    public async Task EnsureSaleAvailabilityAsync(
        Guid lojaId,
        IReadOnlyCollection<StockSaleAvailabilityRequest> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0)
        {
            return;
        }

        foreach (var request in requests)
        {
            if (request.QuantidadeSolicitada <= 0)
            {
                throw new InvalidOperationException("Informe uma quantidade solicitada maior que zero.");
            }
        }

        var pieceIds = requests.Select(x => x.PecaId).Distinct().ToArray();
        var pieces = await _dbContext.Pecas
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => pieceIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.CodigoInterno,
                x.StatusPeca,
                x.QuantidadeAtual,
            })
            .ToListAsync(cancellationToken);

        foreach (var request in requests)
        {
            var piece = pieces.FirstOrDefault(x => x.Id == request.PecaId)
                ?? throw new InvalidOperationException("Peca nao encontrada na loja ativa.");

            if (piece.QuantidadeAtual < request.QuantidadeSolicitada)
            {
                throw new InvalidOperationException(
                    $"A peca {piece.CodigoInterno} nao possui saldo suficiente para a venda.");
            }

            if (!CanBeSold(piece.StatusPeca))
            {
                throw new InvalidOperationException(
                    $"A peca {piece.CodigoInterno} nao esta disponivel para venda.");
            }
        }
    }

    /// <summary>
    /// Define se o status atual ainda permite saida por venda.
    /// </summary>
    private static bool CanBeSold(string statusPeca)
    {
        var normalizedStatus = PieceValues.NormalizePieceStatus(statusPeca);

        return normalizedStatus is PieceValues.PieceStatuses.Disponivel
            or PieceValues.PieceStatuses.Reservada;
    }
}
