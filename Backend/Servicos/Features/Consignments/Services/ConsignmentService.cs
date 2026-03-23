using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.CommercialRules;
using Renova.Services.Features.Consignments.Abstractions;
using Renova.Services.Features.Consignments.Contracts;
using Renova.Services.Features.Pieces;

namespace Renova.Services.Features.Consignments.Services;

// Implementa o modulo 07 com ciclo de vida operacional da consignacao.
public sealed class ConsignmentService : IConsignmentService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public ConsignmentService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega resumo, opcoes de filtro e acoes do modulo para a loja ativa.
    /// </summary>
    public async Task<ConsignmentWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureConsignmentContextAsync(cancellationToken);

        var projections = await LoadConsignmentProjectionsAsync(context.LojaId, null, true, cancellationToken);
        await SyncConsignmentAlertsAsync(context.LojaId, context.UsuarioId, projections, cancellationToken);

        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var fornecedores = projections
            .Where(x => x.Peca.FornecedorPessoaId.HasValue && !string.IsNullOrWhiteSpace(x.FornecedorNome))
            .Select(x => new ConsignmentSupplierOptionResponse(
                x.Peca.FornecedorPessoaId!.Value,
                x.FornecedorNome!,
                x.FornecedorDocumento ?? string.Empty))
            .DistinctBy(x => x.PessoaId)
            .OrderBy(x => x.Nome)
            .ToArray();

        var summary = await BuildSummaryAsync(projections, cancellationToken);

        return new ConsignmentWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            summary,
            fornecedores,
            BuildStatusOptions(),
            BuildActionOptions());
    }

    /// <summary>
    /// Lista as pecas consignadas da loja ativa com filtros e indicadores do ciclo.
    /// </summary>
    public async Task<IReadOnlyList<ConsignmentPieceSummaryResponse>> ListarAsync(
        ConsignmentListQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureConsignmentContextAsync(cancellationToken);

        var projections = await LoadConsignmentProjectionsAsync(context.LojaId, query, true, cancellationToken);
        await SyncConsignmentAlertsAsync(context.LojaId, context.UsuarioId, projections, cancellationToken);

        var basePriceMap = await LoadBasePriceMapAsync(projections.Select(x => x.Peca.Id).ToArray(), cancellationToken);
        var alertPieceIds = await LoadOpenAlertPieceIdsAsync(context.LojaId, projections.Select(x => x.Peca.Id).ToArray(), cancellationToken);

        return projections
            .Select(x => MapSummary(x, basePriceMap, alertPieceIds.Contains(x.Peca.Id)))
            .Where(x => FilterByLifecycleStatus(x, query.StatusConsignacao))
            .Where(x => !query.SomenteProximasDoFim || x.ProximaDoFim || x.Vencida)
            .Where(x => !query.SomenteDescontoPendente || x.DescontoPendente)
            .OrderBy(x => x.DiasRestantes ?? int.MaxValue)
            .ThenBy(x => x.CodigoInterno)
            .ToArray();
    }

    /// <summary>
    /// Carrega o detalhe operacional de uma peca consignada da loja ativa.
    /// </summary>
    public async Task<ConsignmentDetailResponse> ObterDetalheAsync(Guid pecaId, CancellationToken cancellationToken = default)
    {
        var context = await EnsureConsignmentContextAsync(cancellationToken);
        var projection = await LoadConsignmentProjectionByIdAsync(context.LojaId, pecaId, true, cancellationToken)
            ?? throw new InvalidOperationException("Peca consignada nao encontrada na loja ativa.");

        await SyncConsignmentAlertsAsync(context.LojaId, context.UsuarioId, [projection], cancellationToken);

        var basePriceMap = await LoadBasePriceMapAsync([projection.Peca.Id], cancellationToken);
        var alertPieceIds = await LoadOpenAlertPieceIdsAsync(context.LojaId, [projection.Peca.Id], cancellationToken);
        var history = await _dbContext.PecaHistoricosPreco
            .AsNoTracking()
            .Where(x => x.PecaId == pecaId)
            .OrderByDescending(x => x.AlteradoEm)
            .ThenByDescending(x => x.CriadoEm)
            .Select(x => new ConsignmentPriceHistoryResponse(
                x.Id,
                x.PrecoAnterior,
                x.PrecoNovo,
                x.Motivo,
                x.AlteradoEm,
                x.AlteradoPorUsuarioId))
            .ToListAsync(cancellationToken);

        return new ConsignmentDetailResponse(
            MapSummary(projection, basePriceMap, alertPieceIds.Contains(projection.Peca.Id)),
            CommercialRulePolicySerializer.Deserialize(projection.Condicao.PoliticaDescontoJson),
            history);
    }

    /// <summary>
    /// Encerra a consignacao com devolucao, doacao, perda ou descarte.
    /// </summary>
    public async Task<CloseConsignmentResponse> EncerrarAsync(
        Guid pecaId,
        CloseConsignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureConsignmentManageContextAsync(cancellationToken);
        var action = ConsignmentValues.NormalizeCloseAction(request.Acao);
        var reason = NormalizeRequiredText(request.Motivo, "Informe o motivo do encerramento da consignacao.");

        var projection = await LoadConsignmentProjectionByIdAsync(context.LojaId, pecaId, true, cancellationToken)
            ?? throw new InvalidOperationException("Peca consignada nao encontrada na loja ativa.");

        if (!ConsignmentValues.IsActivePieceStatus(projection.Peca.StatusPeca))
        {
            throw new InvalidOperationException("Somente pecas consignadas ativas podem ser encerradas neste modulo.");
        }

        if (projection.Peca.QuantidadeAtual <= 0)
        {
            throw new InvalidOperationException("A peca nao possui saldo disponivel para encerramento.");
        }

        var status = MapPieceStatusFromCloseAction(action);
        var movementType = MapMovementTypeFromCloseAction(action);
        var quantity = projection.Peca.QuantidadeAtual;
        var before = SnapshotPiece(projection.Peca);

        projection.Peca.StatusPeca = status;
        projection.Peca.QuantidadeAtual = 0;
        TouchEntity(projection.Peca, context.UsuarioId);

        var movement = new MovimentacaoEstoque
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            PecaId = projection.Peca.Id,
            TipoMovimentacao = movementType,
            Quantidade = quantity,
            SaldoAnterior = quantity,
            SaldoPosterior = 0,
            OrigemTipo = PieceValues.StockOrigins.Consignacao,
            OrigemId = projection.Peca.Id,
            Motivo = reason,
            MovimentadoEm = DateTimeOffset.UtcNow,
            MovimentadoPorUsuarioId = context.UsuarioId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.MovimentacoesEstoque.Add(movement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await ResolveConsignmentAlertAsync(context.UsuarioId, context.LojaId, projection.Peca.Id, cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca",
            projection.Peca.Id,
            $"consignacao_{action}",
            before,
            SnapshotPiece(projection.Peca),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "movimentacao_estoque",
            movement.Id,
            "criada",
            null,
            SnapshotStockMovement(movement),
            cancellationToken);

        return new CloseConsignmentResponse(
            projection.Peca.Id,
            projection.Peca.CodigoInterno,
            projection.Peca.StatusPeca,
            movement.TipoMovimentacao,
            quantity,
            movement.MovimentadoEm,
            BuildCloseReceipt(projection, action, reason, quantity, movement.MovimentadoEm));
    }

    /// <summary>
    /// Carrega as pecas consignadas da loja ativa conforme os filtros de busca.
    /// </summary>
    private async Task<List<ConsignmentProjection>> LoadConsignmentProjectionsAsync(
        Guid lojaId,
        ConsignmentListQueryRequest? query,
        bool tracking,
        CancellationToken cancellationToken)
    {
        IQueryable<Peca> pieces = tracking ? _dbContext.Pecas : _dbContext.Pecas.AsNoTracking();
        IQueryable<PecaCondicaoComercial> conditions = tracking ? _dbContext.PecaCondicoesComerciais : _dbContext.PecaCondicoesComerciais.AsNoTracking();

        var composedQuery =
            from peca in pieces
            join condicao in conditions on peca.Id equals condicao.PecaId
            join produto in _dbContext.ProdutoNomes.AsNoTracking() on peca.ProdutoNomeId equals produto.Id
            join marca in _dbContext.Marcas.AsNoTracking() on peca.MarcaId equals marca.Id
            join tamanho in _dbContext.Tamanhos.AsNoTracking() on peca.TamanhoId equals tamanho.Id
            join cor in _dbContext.Cores.AsNoTracking() on peca.CorId equals cor.Id
            join fornecedor in _dbContext.Pessoas.AsNoTracking() on peca.FornecedorPessoaId equals fornecedor.Id into supplierGroup
            from fornecedor in supplierGroup.DefaultIfEmpty()
            where peca.LojaId == lojaId
            where peca.TipoPeca == PieceValues.PieceTypes.Consignada
            select new
            {
                Peca = peca,
                Condicao = condicao,
                ProdutoNome = produto.Nome,
                Marca = marca.Nome,
                Tamanho = tamanho.Nome,
                Cor = cor.Nome,
                FornecedorNome = fornecedor != null ? fornecedor.Nome : null,
                FornecedorDocumento = fornecedor != null ? fornecedor.Documento : null,
            };

        if (query?.FornecedorPessoaId is not null)
        {
            composedQuery = composedQuery.Where(x => x.Peca.FornecedorPessoaId == query.FornecedorPessoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query?.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            composedQuery = composedQuery.Where(x =>
                x.Peca.CodigoInterno.ToLower().Contains(term) ||
                x.Peca.CodigoBarras.ToLower().Contains(term) ||
                x.ProdutoNome.ToLower().Contains(term) ||
                x.Marca.ToLower().Contains(term) ||
                x.Cor.ToLower().Contains(term) ||
                x.Tamanho.ToLower().Contains(term) ||
                (x.FornecedorNome ?? string.Empty).ToLower().Contains(term));
        }

        var items = await composedQuery
            .OrderBy(x => x.Peca.CodigoInterno)
            .ToListAsync(cancellationToken);

        return items
            .Select(x => new ConsignmentProjection(
                x.Peca,
                x.Condicao,
                x.ProdutoNome,
                x.Marca,
                x.Tamanho,
                x.Cor,
                x.FornecedorNome,
                x.FornecedorDocumento))
            .ToList();
    }

    /// <summary>
    /// Carrega uma unica peca consignada da loja ativa.
    /// </summary>
    private async Task<ConsignmentProjection?> LoadConsignmentProjectionByIdAsync(
        Guid lojaId,
        Guid pecaId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var projections = await LoadConsignmentProjectionsAsync(
            lojaId,
            new ConsignmentListQueryRequest(null, null, null, false, false),
            tracking,
            cancellationToken);

        return projections.FirstOrDefault(x => x.Peca.Id == pecaId);
    }

    /// <summary>
    /// Sincroniza alertas de consignacao proxima ou vencida para as pecas avaliadas.
    /// </summary>
    private async Task SyncConsignmentAlertsAsync(
        Guid lojaId,
        Guid usuarioId,
        IReadOnlyList<ConsignmentProjection> projections,
        CancellationToken cancellationToken)
    {
        if (projections.Count == 0)
        {
            return;
        }

        var pieceIds = projections.Select(x => x.Peca.Id).ToArray();
        var basePriceMap = await LoadBasePriceMapAsync(pieceIds, cancellationToken);
        var existingAlerts = await _dbContext.AlertasOperacionais
            .Where(x => x.LojaId == lojaId)
            .Where(x => x.TipoAlerta == ConsignmentValues.AlertTypes.ConsignacaoProxima)
            .Where(x => x.ReferenciaTipo == "peca")
            .Where(x => x.ReferenciaId.HasValue && pieceIds.Contains(x.ReferenciaId.Value))
            .ToListAsync(cancellationToken);

        foreach (var projection in projections)
        {
            var summary = MapSummary(projection, basePriceMap, false);
            var existing = existingAlerts.FirstOrDefault(x => x.ReferenciaId == projection.Peca.Id);
            var shouldAlert = ConsignmentValues.IsActivePieceStatus(projection.Peca.StatusPeca) &&
                              summary.DiasRestantes.HasValue &&
                              summary.DiasRestantes.Value <= ConsignmentValues.NearExpirationAlertThresholdDays;

            if (shouldAlert)
            {
                var title = summary.Vencida
                    ? "Consignacao vencida"
                    : "Consignacao proxima do fim";

                var description = summary.Vencida
                    ? $"A peca {summary.CodigoInterno} excedeu o prazo de consignacao."
                    : $"A peca {summary.CodigoInterno} encerra a consignacao em {summary.DiasRestantes} dia(s).";

                if (existing is null)
                {
                    existing = new AlertaOperacional
                    {
                        Id = Guid.NewGuid(),
                        LojaId = lojaId,
                        TipoAlerta = ConsignmentValues.AlertTypes.ConsignacaoProxima,
                        ReferenciaTipo = "peca",
                        ReferenciaId = projection.Peca.Id,
                        CriadoPorUsuarioId = usuarioId,
                    };

                    _dbContext.AlertasOperacionais.Add(existing);
                }

                existing.Severidade = summary.Vencida
                    ? ConsignmentValues.AlertSeverities.Alta
                    : ConsignmentValues.AlertSeverities.Media;
                existing.Titulo = title;
                existing.Descricao = description;
                existing.StatusAlerta = ConsignmentValues.AlertStatuses.Aberto;
                existing.GeradoEm = DateTimeOffset.UtcNow;
                existing.ResolvidoEm = null;
                TouchEntity(existing, usuarioId);
                continue;
            }

            if (existing is not null && existing.StatusAlerta != ConsignmentValues.AlertStatuses.Resolvido)
            {
                existing.StatusAlerta = ConsignmentValues.AlertStatuses.Resolvido;
                existing.ResolvidoEm = DateTimeOffset.UtcNow;
                TouchEntity(existing, usuarioId);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Resolve o alerta aberto da peca apos o encerramento da consignacao.
    /// </summary>
    private async Task ResolveConsignmentAlertAsync(
        Guid usuarioId,
        Guid lojaId,
        Guid pecaId,
        CancellationToken cancellationToken)
    {
        var alert = await _dbContext.AlertasOperacionais
            .FirstOrDefaultAsync(
                x => x.LojaId == lojaId &&
                     x.TipoAlerta == ConsignmentValues.AlertTypes.ConsignacaoProxima &&
                     x.ReferenciaTipo == "peca" &&
                     x.ReferenciaId == pecaId &&
                     x.StatusAlerta == ConsignmentValues.AlertStatuses.Aberto,
                cancellationToken);

        if (alert is null)
        {
            return;
        }

        alert.StatusAlerta = ConsignmentValues.AlertStatuses.Resolvido;
        alert.ResolvidoEm = DateTimeOffset.UtcNow;
        TouchEntity(alert, usuarioId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Monta o resumo consolidado do modulo.
    /// </summary>
    private async Task<ConsignmentSummaryResponse> BuildSummaryAsync(
        IReadOnlyList<ConsignmentProjection> projections,
        CancellationToken cancellationToken)
    {
        var basePriceMap = await LoadBasePriceMapAsync(projections.Select(x => x.Peca.Id).ToArray(), cancellationToken);
        var summaries = projections.Select(x => MapSummary(x, basePriceMap, false)).ToArray();

        return new ConsignmentSummaryResponse(
            summaries.Count(x => x.StatusConsignacao != ConsignmentValues.LifecycleStatuses.Encerrada),
            summaries.Count(x => x.ProximaDoFim),
            summaries.Count(x => x.Vencida),
            summaries.Count(x => x.DescontoPendente));
    }

    /// <summary>
    /// Carrega o preco base original das pecas para evitar desconto cumulativo indevido.
    /// </summary>
    private async Task<IReadOnlyDictionary<Guid, decimal>> LoadBasePriceMapAsync(
        IReadOnlyCollection<Guid> pieceIds,
        CancellationToken cancellationToken)
    {
        if (pieceIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var currentPrices = await _dbContext.Pecas
            .AsNoTracking()
            .Where(x => pieceIds.Contains(x.Id))
            .Select(x => new { x.Id, x.PrecoVendaAtual })
            .ToListAsync(cancellationToken);

        var historyItems = await _dbContext.PecaHistoricosPreco
            .AsNoTracking()
            .Where(x => pieceIds.Contains(x.PecaId))
            .OrderBy(x => x.AlteradoEm)
            .ThenBy(x => x.CriadoEm)
            .ToListAsync(cancellationToken);

        var result = currentPrices.ToDictionary(x => x.Id, x => x.PrecoVendaAtual);
        foreach (var group in historyItems.GroupBy(x => x.PecaId))
        {
            result[group.Key] = group.First().PrecoAnterior;
        }

        return result;
    }

    /// <summary>
    /// Carrega os ids de pecas com alerta operacional aberto.
    /// </summary>
    private async Task<HashSet<Guid>> LoadOpenAlertPieceIdsAsync(
        Guid lojaId,
        IReadOnlyCollection<Guid> pieceIds,
        CancellationToken cancellationToken)
    {
        if (pieceIds.Count == 0)
        {
            return [];
        }

        var ids = await _dbContext.AlertasOperacionais
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => x.TipoAlerta == ConsignmentValues.AlertTypes.ConsignacaoProxima)
            .Where(x => x.StatusAlerta == ConsignmentValues.AlertStatuses.Aberto)
            .Where(x => x.ReferenciaTipo == "peca")
            .Where(x => x.ReferenciaId.HasValue && pieceIds.Contains(x.ReferenciaId.Value))
            .Select(x => x.ReferenciaId!.Value)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    /// <summary>
    /// Garante usuario autenticado, loja ativa e permissao de consulta do modulo.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureConsignmentContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

        var hasMembership = await _dbContext.UsuarioLojas.AnyAsync(
            x => x.UsuarioId == usuarioId &&
                 x.LojaId == lojaId &&
                 x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                 (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
            cancellationToken);

        if (!hasMembership)
        {
            throw new InvalidOperationException("Voce nao possui acesso a loja ativa informada.");
        }

        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.PecasVisualizar, AccessPermissionCodes.PecasCadastrar, AccessPermissionCodes.PecasAjustar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao tem acesso ao ciclo de vida da consignacao na loja ativa.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Garante permissao de ajuste para operacoes mutaveis da consignacao.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureConsignmentManageContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureConsignmentContextAsync(cancellationToken);
        var hasManagePermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.PecasAjustar],
            cancellationToken);

        if (!hasManagePermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para alterar o ciclo de vida da consignacao.");
        }

        return context;
    }

    /// <summary>
    /// Verifica se o usuario possui ao menos uma das permissoes solicitadas.
    /// </summary>
    private async Task<bool> HasPermissionAsync(
        Guid usuarioId,
        Guid lojaId,
        IReadOnlyCollection<string> permissionCodes,
        CancellationToken cancellationToken)
    {
        return await (
                from usuarioLoja in _dbContext.UsuarioLojas
                join usuarioLojaCargo in _dbContext.UsuarioLojaCargos on usuarioLoja.Id equals usuarioLojaCargo.UsuarioLojaId
                join cargo in _dbContext.Cargos on usuarioLojaCargo.CargoId equals cargo.Id
                join cargoPermissao in _dbContext.CargoPermissoes on cargo.Id equals cargoPermissao.CargoId
                join permissao in _dbContext.Permissoes on cargoPermissao.PermissaoId equals permissao.Id
                where usuarioLoja.UsuarioId == usuarioId
                where usuarioLoja.LojaId == lojaId
                where usuarioLoja.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo
                where usuarioLoja.DataFim == null || usuarioLoja.DataFim >= DateTimeOffset.UtcNow
                where cargo.Ativo && permissao.Ativo
                where permissionCodes.Contains(permissao.Codigo)
                select permissao.Id)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Mapeia a projection interna para o resumo consumido pela API.
    /// </summary>
    private static ConsignmentPieceSummaryResponse MapSummary(
        ConsignmentProjection projection,
        IReadOnlyDictionary<Guid, decimal> basePriceMap,
        bool alertOpen)
    {
        var lifecycle = BuildLifecycleSnapshot(projection, basePriceMap);

        return new ConsignmentPieceSummaryResponse(
            projection.Peca.Id,
            projection.Peca.CodigoInterno,
            projection.ProdutoNome,
            projection.Marca,
            projection.Tamanho,
            projection.Cor,
            projection.Peca.FornecedorPessoaId,
            projection.FornecedorNome,
            projection.Peca.StatusPeca,
            lifecycle.StatusConsignacao,
            lifecycle.PrecoBase,
            lifecycle.PrecoEfetivoVenda,
            lifecycle.PercentualDescontoAplicado,
            lifecycle.PercentualDescontoEsperado,
            lifecycle.DescontoAutomaticoAtivo,
            projection.Peca.DataEntrada,
            lifecycle.DataInicioConsignacao,
            lifecycle.DataFimConsignacao,
            lifecycle.DiasEmLoja,
            lifecycle.DiasRestantes,
            lifecycle.ProximaDoFim,
            lifecycle.Vencida,
            projection.Condicao.DestinoPadraoFimConsignacao,
            alertOpen);
    }

    /// <summary>
    /// Resume o estado calculado da consignacao para a peca carregada.
    /// </summary>
    private static LifecycleSnapshot BuildLifecycleSnapshot(
        ConsignmentProjection projection,
        IReadOnlyDictionary<Guid, decimal> basePriceMap)
    {
        var basePrice = basePriceMap.TryGetValue(projection.Peca.Id, out var loadedBasePrice)
            ? loadedBasePrice
            : projection.Peca.PrecoVendaAtual;
        var snapshot = ConsignmentLifecycleCalculator.Calculate(projection.Peca, projection.Condicao, basePrice);

        return new LifecycleSnapshot(
            snapshot.PrecoBase,
            snapshot.PrecoEfetivoVenda,
            snapshot.PercentualDescontoAplicado,
            snapshot.PercentualDescontoEsperado,
            snapshot.DescontoAutomaticoAtivo,
            snapshot.DataInicioConsignacao,
            snapshot.DataFimConsignacao,
            snapshot.DiasEmLoja,
            snapshot.DiasRestantes,
            snapshot.ProximaDoFim,
            snapshot.Vencida,
            snapshot.StatusConsignacao);
    }

    /// <summary>
    /// Define se o resumo calculado atende ao filtro de status operacional.
    /// </summary>
    private static bool FilterByLifecycleStatus(ConsignmentPieceSummaryResponse summary, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        return summary.StatusConsignacao == status.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Traduz a acao final para o status final da peca.
    /// </summary>
    private static string MapPieceStatusFromCloseAction(string action)
    {
        return action switch
        {
            ConsignmentValues.CloseActions.Devolver => PieceValues.PieceStatuses.Devolvida,
            ConsignmentValues.CloseActions.Doar => PieceValues.PieceStatuses.Doada,
            ConsignmentValues.CloseActions.Perder => PieceValues.PieceStatuses.Perdida,
            ConsignmentValues.CloseActions.Descartar => PieceValues.PieceStatuses.Descartada,
            _ => throw new InvalidOperationException("Acao de encerramento invalida."),
        };
    }

    /// <summary>
    /// Traduz a acao final para o movimento de estoque correspondente.
    /// </summary>
    private static string MapMovementTypeFromCloseAction(string action)
    {
        return action switch
        {
            ConsignmentValues.CloseActions.Devolver => PieceValues.StockMovementTypes.Devolucao,
            ConsignmentValues.CloseActions.Doar => PieceValues.StockMovementTypes.Doacao,
            ConsignmentValues.CloseActions.Perder => PieceValues.StockMovementTypes.Perda,
            ConsignmentValues.CloseActions.Descartar => PieceValues.StockMovementTypes.Descarte,
            _ => throw new InvalidOperationException("Acao de encerramento invalida."),
        };
    }

    /// <summary>
    /// Monta o comprovante textual padrao do encerramento da consignacao.
    /// </summary>
    private static string BuildCloseReceipt(
        ConsignmentProjection projection,
        string action,
        string reason,
        int quantity,
        DateTimeOffset movedAt)
    {
        var fornecedor = string.IsNullOrWhiteSpace(projection.FornecedorNome)
            ? "Nao informado"
            : projection.FornecedorNome;

        return
$@"Comprovante de encerramento da consignacao
Peca: {projection.Peca.CodigoInterno}
Produto: {projection.ProdutoNome} / {projection.Marca} / {projection.Tamanho} / {projection.Cor}
Fornecedor: {fornecedor}
Acao: {action}
Quantidade baixada: {quantity}
Data: {movedAt:dd/MM/yyyy HH:mm}
Motivo: {reason}";
    }

    /// <summary>
    /// Expone os status operacionais usados no filtro da tela.
    /// </summary>
    private static IReadOnlyList<ConsignmentStatusOptionResponse> BuildStatusOptions()
    {
        return
        [
            new(ConsignmentValues.LifecycleStatuses.Ativa, "Ativa"),
            new(ConsignmentValues.LifecycleStatuses.Proxima, "Proxima do fim"),
            new(ConsignmentValues.LifecycleStatuses.Vencida, "Vencida"),
            new(ConsignmentValues.LifecycleStatuses.Encerrada, "Encerrada"),
        ];
    }

    /// <summary>
    /// Expone as acoes finais permitidas para encerramento da consignacao.
    /// </summary>
    private static IReadOnlyList<ConsignmentActionOptionResponse> BuildActionOptions()
    {
        return
        [
            new(ConsignmentValues.CloseActions.Devolver, "Devolver ao fornecedor"),
            new(ConsignmentValues.CloseActions.Doar, "Doar peca"),
            new(ConsignmentValues.CloseActions.Perder, "Registrar perda"),
            new(ConsignmentValues.CloseActions.Descartar, "Descartar peca"),
        ];
    }

    /// <summary>
    /// Atualiza metadados comuns das entidades auditaveis.
    /// </summary>
    private static void TouchEntity(AuditEntityBase entity, Guid usuarioId)
    {
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Normaliza um texto obrigatorio.
    /// </summary>
    private static string NormalizeRequiredText(string value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    /// <summary>
    /// Resume a peca para trilha de auditoria.
    /// </summary>
    private static object SnapshotPiece(Peca entity)
    {
        return new
        {
            entity.LojaId,
            entity.FornecedorPessoaId,
            entity.TipoPeca,
            entity.CodigoInterno,
            entity.CodigoBarras,
            entity.PrecoVendaAtual,
            entity.QuantidadeAtual,
            entity.StatusPeca,
            entity.LocalizacaoFisica,
        };
    }

    /// <summary>
    /// Resume a movimentacao de estoque gerada no encerramento.
    /// </summary>
    private static object SnapshotStockMovement(MovimentacaoEstoque entity)
    {
        return new
        {
            entity.LojaId,
            entity.PecaId,
            entity.TipoMovimentacao,
            entity.Quantidade,
            entity.SaldoAnterior,
            entity.SaldoPosterior,
            entity.OrigemTipo,
            entity.OrigemId,
            entity.Motivo,
        };
    }

    /// <summary>
    /// Projection interna usada para compor resumo e detalhe.
    /// </summary>
    private sealed record ConsignmentProjection(
        Peca Peca,
        PecaCondicaoComercial Condicao,
        string ProdutoNome,
        string Marca,
        string Tamanho,
        string Cor,
        string? FornecedorNome,
        string? FornecedorDocumento);

    /// <summary>
    /// Snapshot calculado do prazo e da politica de desconto da consignacao.
    /// </summary>
    private sealed record LifecycleSnapshot(
        decimal PrecoBase,
        decimal PrecoEfetivoVenda,
        decimal PercentualDescontoAplicado,
        decimal PercentualDescontoEsperado,
        bool DescontoAutomaticoAtivo,
        DateTimeOffset DataInicioConsignacao,
        DateTimeOffset DataFimConsignacao,
        int DiasEmLoja,
        int? DiasRestantes,
        bool ProximaDoFim,
        bool Vencida,
        string StatusConsignacao);
}
