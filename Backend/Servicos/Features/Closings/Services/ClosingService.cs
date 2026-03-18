using System.Text;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Closings.Abstractions;
using Renova.Services.Features.Closings.Contracts;
using Renova.Services.Features.Credits;
using Renova.Services.Features.People;
using Renova.Services.Features.Pieces;
using Renova.Services.Features.Sales;
using Renova.Services.Features.SupplierPayments;

namespace Renova.Services.Features.Closings.Services;

// Implementa o modulo 13 com geracao, conferencia, liquidacao e exportacao do fechamento.
public sealed class ClosingService : IClosingService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public ClosingService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega filtros e pessoas elegiveis para geracao do fechamento.
    /// </summary>
    public async Task<ClosingWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureClosingViewContextAsync(cancellationToken);
        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var people = await (
                from relation in _dbContext.PessoaLojas.AsNoTracking()
                join person in _dbContext.Pessoas.AsNoTracking() on relation.PessoaId equals person.Id
                where relation.LojaId == context.LojaId
                where relation.StatusRelacao == PeopleStatusValues.StatusRelacao.Ativo
                where relation.EhCliente || relation.EhFornecedor
                orderby person.Nome
                select new ClosingPersonOptionResponse(
                    person.Id,
                    person.Nome,
                    person.Documento,
                    relation.EhCliente,
                    relation.EhFornecedor,
                    relation.AceitaCreditoLoja))
            .ToListAsync(cancellationToken);

        return new ClosingWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            people,
            ClosingValues.BuildStatusOptions()
                .Select(x => new ClosingOptionResponse(x.Codigo, x.Nome))
                .ToArray(),
            ClosingValues.BuildMovementTypeOptions()
                .Select(x => new ClosingOptionResponse(x.Codigo, x.Nome))
                .ToArray());
    }

    /// <summary>
    /// Lista os fechamentos gravados com filtros simples por pessoa, status e periodo.
    /// </summary>
    public async Task<IReadOnlyList<ClosingSummaryResponse>> ListarAsync(
        ClosingListQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureClosingViewContextAsync(cancellationToken);
        return await LoadClosingSummariesAsync(context.LojaId, query, cancellationToken);
    }

    /// <summary>
    /// Carrega o fechamento gravado com seus snapshots de itens e movimentos.
    /// </summary>
    public async Task<ClosingDetailResponse> ObterDetalheAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureClosingViewContextAsync(cancellationToken);
        return await LoadClosingDetailAsync(context.LojaId, fechamentoId, cancellationToken)
            ?? throw new InvalidOperationException("Fechamento nao encontrado na loja ativa.");
    }

    /// <summary>
    /// Gera ou regenera o snapshot do fechamento para a pessoa e periodo informados.
    /// </summary>
    public async Task<ClosingDetailResponse> GerarAsync(
        GenerateClosingRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureClosingGenerateContextAsync(cancellationToken);
        var periodStart = ToUtcStart(request.PeriodoInicio);
        var periodEnd = ToUtcEnd(request.PeriodoFim);
        if (periodEnd < periodStart)
        {
            throw new InvalidOperationException("O periodo final precisa ser maior ou igual ao periodo inicial.");
        }

        var person = await LoadStorePersonAsync(context.LojaId, request.PessoaId, cancellationToken);
        var generatedData = await BuildGeneratedClosingAsync(
            context.LojaId,
            request.PessoaId,
            periodStart,
            periodEnd,
            person,
            cancellationToken);

        var closing = await _dbContext.FechamentosPessoa
            .FirstOrDefaultAsync(
                x => x.LojaId == context.LojaId &&
                     x.PessoaId == request.PessoaId &&
                     x.PeriodoInicio == periodStart &&
                     x.PeriodoFim == periodEnd,
                cancellationToken);

        var previousSnapshot = closing is null ? null : SnapshotClosing(closing);
        if (closing is not null && ClosingValues.NormalizeStatus(closing.StatusFechamento) == ClosingValues.Statuses.Liquidado)
        {
            throw new InvalidOperationException("Nao e possivel regerar um fechamento ja liquidado.");
        }

        if (closing is null)
        {
            closing = new FechamentoPessoa
            {
                Id = Guid.NewGuid(),
                LojaId = context.LojaId,
                PessoaId = request.PessoaId,
                CriadoPorUsuarioId = context.UsuarioId,
            };

            _dbContext.FechamentosPessoa.Add(closing);
        }
        else
        {
            var existingItems = _dbContext.FechamentoPessoaItens.Where(x => x.FechamentoPessoaId == closing.Id);
            var existingMovements = _dbContext.FechamentoPessoaMovimentos.Where(x => x.FechamentoPessoaId == closing.Id);
            _dbContext.FechamentoPessoaItens.RemoveRange(existingItems);
            _dbContext.FechamentoPessoaMovimentos.RemoveRange(existingMovements);
        }

        closing.PeriodoInicio = periodStart;
        closing.PeriodoFim = periodEnd;
        closing.StatusFechamento = ClosingValues.Statuses.Aberto;
        closing.ValorVendido = generatedData.ValorVendido;
        closing.ValorAReceber = generatedData.ValorAReceber;
        closing.ValorPago = generatedData.ValorPago;
        closing.ValorCompradoNaLoja = generatedData.ValorCompradoNaLoja;
        closing.SaldoFinal = generatedData.SaldoFinal;
        closing.ResumoTexto = generatedData.ResumoTexto;
        closing.GeradoEm = DateTimeOffset.UtcNow;
        closing.GeradoPorUsuarioId = context.UsuarioId;
        closing.ConferidoEm = null;
        closing.ConferidoPorUsuarioId = null;
        closing.AtualizadoEm = DateTimeOffset.UtcNow;
        closing.AtualizadoPorUsuarioId = context.UsuarioId;
        closing.PdfUrl = BuildExportUrl(closing.Id, ClosingValues.ExportTypes.Pdf);
        closing.ExcelUrl = BuildExportUrl(closing.Id, ClosingValues.ExportTypes.Excel);

        var itemEntities = generatedData.Items.Select(item => new FechamentoPessoaItem
        {
            Id = Guid.NewGuid(),
            FechamentoPessoaId = closing.Id,
            PecaId = item.PecaId,
            StatusPecaSnapshot = item.StatusPecaSnapshot,
            ValorVendaSnapshot = item.ValorVendaSnapshot,
            ValorRepasseSnapshot = item.ValorRepasseSnapshot,
            DataEvento = item.DataEvento,
            CriadoPorUsuarioId = context.UsuarioId,
        }).ToArray();

        var movementEntities = generatedData.Movements.Select(movement => new FechamentoPessoaMovimento
        {
            Id = Guid.NewGuid(),
            FechamentoPessoaId = closing.Id,
            TipoMovimento = movement.TipoMovimento,
            OrigemTipo = movement.OrigemTipo,
            OrigemId = movement.OrigemId,
            DataMovimento = movement.DataMovimento,
            Descricao = movement.Descricao,
            Valor = movement.Valor,
            CriadoPorUsuarioId = context.UsuarioId,
        }).ToArray();

        _dbContext.FechamentoPessoaItens.AddRange(itemEntities);
        _dbContext.FechamentoPessoaMovimentos.AddRange(movementEntities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "fechamento_pessoa",
            closing.Id,
            previousSnapshot is null ? "criado" : "regenerado",
            previousSnapshot,
            SnapshotClosing(closing),
            cancellationToken);

        var detail = await LoadClosingDetailAsync(context.LojaId, closing.Id, cancellationToken)
            ?? throw new InvalidOperationException("Fechamento gerado, mas nao foi possivel carregar o detalhe.");

        return detail;
    }

    /// <summary>
    /// Marca o fechamento como conferido sem alterar os snapshots gravados.
    /// </summary>
    public async Task<ClosingDetailResponse> ConferirAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureClosingReviewContextAsync(cancellationToken);
        var closing = await _dbContext.FechamentosPessoa
            .FirstOrDefaultAsync(x => x.Id == fechamentoId && x.LojaId == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Fechamento nao encontrado na loja ativa.");

        if (ClosingValues.NormalizeStatus(closing.StatusFechamento) == ClosingValues.Statuses.Liquidado)
        {
            throw new InvalidOperationException("Fechamentos liquidados nao podem voltar para conferencia.");
        }

        var before = SnapshotClosing(closing);
        closing.StatusFechamento = ClosingValues.Statuses.Conferido;
        closing.ConferidoEm = DateTimeOffset.UtcNow;
        closing.ConferidoPorUsuarioId = context.UsuarioId;
        closing.AtualizadoEm = DateTimeOffset.UtcNow;
        closing.AtualizadoPorUsuarioId = context.UsuarioId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "fechamento_pessoa",
            closing.Id,
            "conferido",
            before,
            SnapshotClosing(closing),
            cancellationToken);

        return await ObterDetalheAsync(fechamentoId, cancellationToken);
    }

    /// <summary>
    /// Marca o fechamento como liquidado quando nao ha saldo pendente.
    /// </summary>
    public async Task<ClosingDetailResponse> LiquidarAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureClosingReviewContextAsync(cancellationToken);
        var closing = await _dbContext.FechamentosPessoa
            .FirstOrDefaultAsync(x => x.Id == fechamentoId && x.LojaId == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Fechamento nao encontrado na loja ativa.");

        var normalizedStatus = ClosingValues.NormalizeStatus(closing.StatusFechamento);
        if (normalizedStatus == ClosingValues.Statuses.Liquidado)
        {
            return await ObterDetalheAsync(fechamentoId, cancellationToken);
        }

        if (Math.Abs(closing.SaldoFinal) > 0.009m || Math.Abs(closing.ValorAReceber) > 0.009m)
        {
            throw new InvalidOperationException(
                "O fechamento so pode ser liquidado quando nao houver saldo final nem valor a receber.");
        }

        var before = SnapshotClosing(closing);
        closing.StatusFechamento = ClosingValues.Statuses.Liquidado;
        closing.ConferidoEm ??= DateTimeOffset.UtcNow;
        closing.ConferidoPorUsuarioId ??= context.UsuarioId;
        closing.AtualizadoEm = DateTimeOffset.UtcNow;
        closing.AtualizadoPorUsuarioId = context.UsuarioId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "fechamento_pessoa",
            closing.Id,
            "liquidado",
            before,
            SnapshotClosing(closing),
            cancellationToken);

        return await ObterDetalheAsync(fechamentoId, cancellationToken);
    }

    /// <summary>
    /// Gera o exportavel imprimivel para navegadores ou conversao posterior em PDF.
    /// </summary>
    public async Task<ClosingExportResponse> ExportarPdfAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default)
    {
        var detail = await ObterDetalheAsync(fechamentoId, cancellationToken);
        var html = BuildPrintableHtml(detail);
        return new ClosingExportResponse(
            BuildFileName(detail.Fechamento, "html"),
            "text/html; charset=utf-8",
            Encoding.UTF8.GetBytes(html));
    }

    /// <summary>
    /// Gera um arquivo CSV compativel com abertura no Excel.
    /// </summary>
    public async Task<ClosingExportResponse> ExportarExcelAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default)
    {
        var detail = await ObterDetalheAsync(fechamentoId, cancellationToken);
        var csv = BuildCsv(detail);
        return new ClosingExportResponse(
            BuildFileName(detail.Fechamento, "csv"),
            "text/csv; charset=utf-8",
            Encoding.UTF8.GetBytes(csv));
    }

    // Remaining helpers below.

    private async Task<IReadOnlyList<ClosingSummaryResponse>> LoadClosingSummariesAsync(
        Guid lojaId,
        ClosingListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var closingQuery =
            from closing in _dbContext.FechamentosPessoa.AsNoTracking()
            join person in _dbContext.Pessoas.AsNoTracking() on closing.PessoaId equals person.Id
            join relation in _dbContext.PessoaLojas.AsNoTracking()
                on new { closing.PessoaId, closing.LojaId } equals new { relation.PessoaId, relation.LojaId }
            join generatedBy in _dbContext.Usuarios.AsNoTracking() on closing.GeradoPorUsuarioId equals generatedBy.Id
            join checkedBy in _dbContext.Usuarios.AsNoTracking() on closing.ConferidoPorUsuarioId equals checkedBy.Id into checkedByGroup
            from checkedBy in checkedByGroup.DefaultIfEmpty()
            where closing.LojaId == lojaId
            select new
            {
                Closing = closing,
                PersonName = person.Nome,
                PersonDocument = person.Documento,
                relation.EhCliente,
                relation.EhFornecedor,
                GeneratedByName = generatedBy.Nome,
                ReviewedByName = checkedBy != null ? checkedBy.Nome : null,
            };

        if (query.PessoaId.HasValue)
        {
            closingQuery = closingQuery.Where(x => x.Closing.PessoaId == query.PessoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.StatusFechamento))
        {
            var status = ClosingValues.NormalizeStatus(query.StatusFechamento);
            closingQuery = closingQuery.Where(x => x.Closing.StatusFechamento == status);
        }

        if (query.DataInicial.HasValue)
        {
            var start = ToUtcStart(query.DataInicial.Value);
            closingQuery = closingQuery.Where(x => x.Closing.PeriodoFim >= start);
        }

        if (query.DataFinal.HasValue)
        {
            var end = ToUtcEnd(query.DataFinal.Value);
            closingQuery = closingQuery.Where(x => x.Closing.PeriodoInicio <= end);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            closingQuery = closingQuery.Where(x =>
                x.PersonName.ToLower().Contains(term) ||
                x.PersonDocument.ToLower().Contains(term) ||
                x.GeneratedByName.ToLower().Contains(term) ||
                (x.ReviewedByName ?? string.Empty).ToLower().Contains(term));
        }

        var closings = await closingQuery
            .OrderByDescending(x => x.Closing.PeriodoFim)
            .ThenByDescending(x => x.Closing.GeradoEm)
            .ToListAsync(cancellationToken);

        if (closings.Count == 0)
        {
            return [];
        }

        var closingIds = closings.Select(x => x.Closing.Id).ToArray();
        var personIds = closings.Select(x => x.Closing.PessoaId).Distinct().ToArray();
        var itemGroups = await _dbContext.FechamentoPessoaItens
            .AsNoTracking()
            .Where(x => closingIds.Contains(x.FechamentoPessoaId))
            .GroupBy(x => x.FechamentoPessoaId)
            .Select(group => new
            {
                FechamentoId = group.Key,
                QuantidadePecasAtuais = group.Count(item =>
                    item.StatusPecaSnapshot == PieceValues.PieceStatuses.Disponivel ||
                    item.StatusPecaSnapshot == PieceValues.PieceStatuses.Reservada ||
                    item.StatusPecaSnapshot == PieceValues.PieceStatuses.Inativa),
                QuantidadePecasVendidas = group.Count(item => item.StatusPecaSnapshot == PieceValues.PieceStatuses.Vendida),
            })
            .ToDictionaryAsync(x => x.FechamentoId, cancellationToken);

        var creditBalances = await _dbContext.ContasCreditoLoja
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId && personIds.Contains(x.PessoaId))
            .GroupBy(x => x.PessoaId)
            .Select(group => new
            {
                PessoaId = group.Key,
                SaldoAtual = group.Sum(x => x.SaldoAtual),
            })
            .ToDictionaryAsync(x => x.PessoaId, cancellationToken);

        return closings.Select(item =>
        {
            var counters = itemGroups.GetValueOrDefault(item.Closing.Id);
            var creditBalance = creditBalances.GetValueOrDefault(item.Closing.PessoaId)?.SaldoAtual ?? 0m;

            return new ClosingSummaryResponse(
                item.Closing.Id,
                item.Closing.PessoaId,
                item.PersonName,
                item.PersonDocument,
                item.EhCliente,
                item.EhFornecedor,
                item.Closing.PeriodoInicio,
                item.Closing.PeriodoFim,
                item.Closing.StatusFechamento,
                item.Closing.ValorVendido,
                item.Closing.ValorAReceber,
                item.Closing.ValorPago,
                item.Closing.ValorCompradoNaLoja,
                RoundMoney(creditBalance),
                item.Closing.SaldoFinal,
                counters?.QuantidadePecasAtuais ?? 0,
                counters?.QuantidadePecasVendidas ?? 0,
                item.Closing.GeradoEm,
                item.Closing.GeradoPorUsuarioId,
                item.GeneratedByName,
                item.Closing.ConferidoEm,
                item.Closing.ConferidoPorUsuarioId,
                item.ReviewedByName,
                item.Closing.ResumoTexto,
                item.Closing.PdfUrl,
                item.Closing.ExcelUrl);
        }).ToArray();
    }

    private async Task<ClosingDetailResponse?> LoadClosingDetailAsync(
        Guid lojaId,
        Guid fechamentoId,
        CancellationToken cancellationToken)
    {
        var summary = (await LoadClosingSummariesAsync(
            lojaId,
            new ClosingListQueryRequest(null, null, null, null, null),
            cancellationToken))
            .FirstOrDefault(x => x.Id == fechamentoId);

        if (summary is null)
        {
            return null;
        }

        var items = await (
                from item in _dbContext.FechamentoPessoaItens.AsNoTracking()
                join piece in _dbContext.Pecas.AsNoTracking() on item.PecaId equals piece.Id
                join product in _dbContext.ProdutoNomes.AsNoTracking() on piece.ProdutoNomeId equals product.Id
                where item.FechamentoPessoaId == fechamentoId
                orderby item.DataEvento descending, piece.CodigoInterno
                select new ClosingItemResponse(
                    item.Id,
                    item.PecaId,
                    item.StatusPecaSnapshot == PieceValues.PieceStatuses.Vendida
                        ? ClosingValues.ItemGroups.Vendida
                        : ClosingValues.ItemGroups.Atual,
                    piece.CodigoInterno,
                    product.Nome,
                    item.StatusPecaSnapshot,
                    item.ValorVendaSnapshot,
                    item.ValorRepasseSnapshot,
                    item.DataEvento))
            .ToListAsync(cancellationToken);

        var movements = await _dbContext.FechamentoPessoaMovimentos
            .AsNoTracking()
            .Where(x => x.FechamentoPessoaId == fechamentoId)
            .OrderByDescending(x => x.DataMovimento)
            .ThenByDescending(x => x.CriadoEm)
            .Select(x => new ClosingMovementResponse(
                x.Id,
                x.TipoMovimento,
                x.OrigemTipo,
                x.OrigemId,
                x.DataMovimento,
                x.Descricao,
                x.Valor))
            .ToListAsync(cancellationToken);

        return new ClosingDetailResponse(summary, items, movements, summary.ResumoTexto);
    }

    private async Task<GeneratedClosingData> BuildGeneratedClosingAsync(
        Guid lojaId,
        Guid pessoaId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        StorePersonContext person,
        CancellationToken cancellationToken)
    {
        var soldPieces = await (
                from saleItem in _dbContext.VendaItens.AsNoTracking()
                join sale in _dbContext.Vendas.AsNoTracking() on saleItem.VendaId equals sale.Id
                join piece in _dbContext.Pecas.AsNoTracking() on saleItem.PecaId equals piece.Id
                where sale.LojaId == lojaId
                where sale.StatusVenda == SaleValues.SaleStatuses.Concluida
                where saleItem.FornecedorPessoaIdSnapshot == pessoaId
                where sale.DataHoraVenda >= periodStart && sale.DataHoraVenda <= periodEnd
                select new SoldPieceRow(
                    saleItem.PecaId,
                    saleItem.Id,
                    piece.CodigoInterno,
                    sale.NumeroVenda,
                    sale.DataHoraVenda,
                    RoundMoney(saleItem.PrecoFinalUnitario * saleItem.Quantidade),
                    RoundMoney(saleItem.ValorRepassePrevisto)))
            .ToListAsync(cancellationToken);

        var currentPieces = await (
                from piece in _dbContext.Pecas.AsNoTracking()
                where piece.LojaId == lojaId
                where piece.FornecedorPessoaId == pessoaId
                where piece.DataEntrada <= periodEnd
                where piece.StatusPeca == PieceValues.PieceStatuses.Disponivel ||
                      piece.StatusPeca == PieceValues.PieceStatuses.Reservada ||
                      piece.StatusPeca == PieceValues.PieceStatuses.Inativa
                select new CurrentPieceRow(
                    piece.Id,
                    piece.CodigoInterno,
                    piece.StatusPeca,
                    piece.DataEntrada))
            .ToListAsync(cancellationToken);

        var openObligationsTotal = await _dbContext.ObrigacoesFornecedor
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => x.PessoaId == pessoaId)
            .Where(x => x.DataGeracao <= periodEnd)
            .Where(x => x.StatusObrigacao != SupplierPaymentValues.ObligationStatuses.Cancelada)
            .SumAsync(x => (decimal?)x.ValorEmAberto, cancellationToken) ?? 0m;

        var supplierPayments = await (
                from liquidation in _dbContext.LiquidacoesObrigacaoFornecedor.AsNoTracking()
                join obligation in _dbContext.ObrigacoesFornecedor.AsNoTracking() on liquidation.ObrigacaoFornecedorId equals obligation.Id
                where obligation.LojaId == lojaId
                where obligation.PessoaId == pessoaId
                where liquidation.LiquidadoEm >= periodStart && liquidation.LiquidadoEm <= periodEnd
                select new SupplierPaymentRow(
                    liquidation.Id,
                    liquidation.TipoLiquidacao,
                    liquidation.LiquidadoEm,
                    RoundMoney(liquidation.Valor),
                    liquidation.Observacoes))
            .ToListAsync(cancellationToken);

        var customerPurchases = await _dbContext.Vendas
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId)
            .Where(x => x.StatusVenda == SaleValues.SaleStatuses.Concluida)
            .Where(x => x.CompradorPessoaId == pessoaId)
            .Where(x => x.DataHoraVenda >= periodStart && x.DataHoraVenda <= periodEnd)
            .Select(x => new CustomerPurchaseRow(
                x.Id,
                x.NumeroVenda,
                x.DataHoraVenda,
                RoundMoney(x.TotalLiquido),
                x.Observacoes))
            .ToListAsync(cancellationToken);

        var creditAccount = await _dbContext.ContasCreditoLoja
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LojaId == lojaId && x.PessoaId == pessoaId, cancellationToken);

        var creditMovements = creditAccount is null
            ? []
            : await _dbContext.MovimentacoesCreditoLoja
                .AsNoTracking()
                .Where(x => x.ContaCreditoLojaId == creditAccount.Id)
                .Where(x => x.MovimentadoEm >= periodStart && x.MovimentadoEm <= periodEnd)
                .OrderByDescending(x => x.MovimentadoEm)
                .Select(x => new CreditMovementRow(
                    x.Id,
                    x.TipoMovimentacao,
                    x.OrigemTipo,
                    x.OrigemId,
                    x.MovimentadoEm,
                    RoundMoney(
                        CreditValues.ResolveDirection(x.TipoMovimentacao) == "entrada"
                            ? x.Valor
                            : -x.Valor),
                    x.Observacoes))
                .ToListAsync(cancellationToken);

        var itemDrafts = new List<ClosingItemDraft>();
        itemDrafts.AddRange(currentPieces.Select(piece => new ClosingItemDraft(
            piece.PecaId,
            piece.StatusPeca,
            null,
            null,
            piece.DataEntrada)));
        itemDrafts.AddRange(soldPieces.Select(item => new ClosingItemDraft(
            item.PecaId,
            PieceValues.PieceStatuses.Vendida,
            item.ValorVenda,
            item.ValorRepasse,
            item.DataVenda)));

        var movementDrafts = new List<ClosingMovementDraft>();
        movementDrafts.AddRange(soldPieces.Select(item => new ClosingMovementDraft(
            ClosingValues.MovementTypes.Venda,
            "venda_item",
            item.VendaItemId,
            item.DataVenda,
            $"Venda da peca {item.CodigoInternoPeca} na venda {item.NumeroVenda}.",
            item.ValorRepasse)));
        movementDrafts.AddRange(supplierPayments.Select(item => new ClosingMovementDraft(
            ClosingValues.MovementTypes.Pagamento,
            "liquidacao_obrigacao_fornecedor",
            item.LiquidacaoId,
            item.LiquidadoEm,
            $"Liquidacao {item.TipoLiquidacao} do fornecedor.",
            -item.Valor)));
        movementDrafts.AddRange(creditMovements.Select(item => new ClosingMovementDraft(
            ClosingValues.MovementTypes.Credito,
            item.OrigemTipo,
            item.OrigemId,
            item.MovimentadoEm,
            string.IsNullOrWhiteSpace(item.Observacoes)
                ? $"Movimento de credito {item.TipoMovimentacao}."
                : item.Observacoes,
            item.Valor)));
        movementDrafts.AddRange(customerPurchases.Select(item => new ClosingMovementDraft(
            ClosingValues.MovementTypes.CompraLoja,
            "venda",
            item.VendaId,
            item.DataVenda,
            $"Compra do cliente na venda {item.NumeroVenda}.",
            -item.TotalLiquido)));

        var valorVendido = RoundMoney(soldPieces.Sum(x => x.ValorRepasse));
        var valorPago = RoundMoney(supplierPayments.Sum(x => x.Valor));
        var valorCompradoNaLoja = RoundMoney(customerPurchases.Sum(x => x.TotalLiquido));
        var saldoCreditoAtual = RoundMoney(creditAccount?.SaldoAtual ?? 0m);
        var valorAReceber = RoundMoney(openObligationsTotal);
        var saldoFinal = RoundMoney(valorAReceber + saldoCreditoAtual);
        var resumoTexto = BuildSummaryText(
            person,
            periodStart,
            periodEnd,
            currentPieces.Count,
            soldPieces.Count,
            valorVendido,
            valorAReceber,
            valorPago,
            valorCompradoNaLoja,
            saldoCreditoAtual,
            saldoFinal);

        return new GeneratedClosingData(
            itemDrafts,
            movementDrafts.OrderByDescending(x => x.DataMovimento).ToArray(),
            valorVendido,
            valorAReceber,
            valorPago,
            valorCompradoNaLoja,
            saldoFinal,
            resumoTexto);
    }

    private static string BuildSummaryText(
        StorePersonContext person,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        int quantityCurrentPieces,
        int quantitySoldPieces,
        decimal valorVendido,
        decimal valorAReceber,
        decimal valorPago,
        decimal valorCompradoNaLoja,
        decimal saldoCreditoAtual,
        decimal saldoFinal)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Fechamento: {person.Nome}");
        builder.AppendLine($"Documento: {person.Documento}");
        builder.AppendLine($"Periodo: {periodStart:dd/MM/yyyy} a {periodEnd:dd/MM/yyyy}");
        builder.AppendLine($"Pecas atuais: {quantityCurrentPieces}");
        builder.AppendLine($"Pecas vendidas: {quantitySoldPieces}");
        builder.AppendLine($"Valor vendido: {valorVendido:N2}");
        builder.AppendLine($"Valor a receber: {valorAReceber:N2}");
        builder.AppendLine($"Valor pago: {valorPago:N2}");
        builder.AppendLine($"Compras na loja: {valorCompradoNaLoja:N2}");
        builder.AppendLine($"Saldo de credito: {saldoCreditoAtual:N2}");
        builder.AppendLine($"Saldo final: {saldoFinal:N2}");
        return builder.ToString().Trim();
    }

    private static string BuildPrintableHtml(ClosingDetailResponse detail)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\" />");
        builder.AppendLine("<title>Fechamento</title>");
        builder.AppendLine("<style>body{font-family:Arial,sans-serif;padding:24px;color:#1f2937}table{width:100%;border-collapse:collapse;margin-top:16px}th,td{border:1px solid #d1d5db;padding:8px;text-align:left}h1,h2{margin:0 0 12px}pre{white-space:pre-wrap;background:#f8fafc;padding:12px;border-radius:8px}</style>");
        builder.AppendLine("</head><body>");
        builder.AppendLine($"<h1>Fechamento de {EscapeHtml(detail.Fechamento.PessoaNome)}</h1>");
        builder.AppendLine($"<p>Periodo: {detail.Fechamento.PeriodoInicio:dd/MM/yyyy} a {detail.Fechamento.PeriodoFim:dd/MM/yyyy}</p>");
        builder.AppendLine("<h2>Resumo</h2>");
        builder.AppendLine($"<pre>{EscapeHtml(detail.ResumoWhatsapp)}</pre>");
        builder.AppendLine("<h2>Itens</h2><table><thead><tr><th>Grupo</th><th>Peca</th><th>Produto</th><th>Status</th><th>Venda</th><th>Repasse</th><th>Data</th></tr></thead><tbody>");
        foreach (var item in detail.Itens)
        {
            builder.AppendLine(
                $"<tr><td>{EscapeHtml(item.GrupoItem)}</td><td>{EscapeHtml(item.CodigoInternoPeca)}</td><td>{EscapeHtml(item.ProdutoNomePeca)}</td><td>{EscapeHtml(item.StatusPecaSnapshot)}</td><td>{item.ValorVendaSnapshot?.ToString("N2") ?? "-"}</td><td>{item.ValorRepasseSnapshot?.ToString("N2") ?? "-"}</td><td>{item.DataEvento:dd/MM/yyyy}</td></tr>");
        }
        builder.AppendLine("</tbody></table>");
        builder.AppendLine("<h2>Movimentos</h2><table><thead><tr><th>Tipo</th><th>Descricao</th><th>Data</th><th>Valor</th></tr></thead><tbody>");
        foreach (var movement in detail.Movimentos)
        {
            builder.AppendLine(
                $"<tr><td>{EscapeHtml(movement.TipoMovimento)}</td><td>{EscapeHtml(movement.Descricao)}</td><td>{movement.DataMovimento:dd/MM/yyyy}</td><td>{movement.Valor:N2}</td></tr>");
        }
        builder.AppendLine("</tbody></table></body></html>");
        return builder.ToString();
    }

    private static string BuildCsv(ClosingDetailResponse detail)
    {
        var builder = new StringBuilder();
        builder.AppendLine("secao;descricao;valor1;valor2;valor3;data");
        builder.AppendLine($"resumo;Pessoa;{EscapeCsv(detail.Fechamento.PessoaNome)};;;");
        builder.AppendLine($"resumo;Periodo;{detail.Fechamento.PeriodoInicio:dd/MM/yyyy};{detail.Fechamento.PeriodoFim:dd/MM/yyyy};;");
        builder.AppendLine($"resumo;Valor vendido;{detail.Fechamento.ValorVendido:N2};;;");
        builder.AppendLine($"resumo;Valor a receber;{detail.Fechamento.ValorAReceber:N2};;;");
        builder.AppendLine($"resumo;Valor pago;{detail.Fechamento.ValorPago:N2};;;");
        builder.AppendLine($"resumo;Valor comprado na loja;{detail.Fechamento.ValorCompradoNaLoja:N2};;;");
        builder.AppendLine($"resumo;Saldo final;{detail.Fechamento.SaldoFinal:N2};;;");

        foreach (var item in detail.Itens)
        {
            builder.AppendLine(
                $"item;{EscapeCsv(item.CodigoInternoPeca)};{EscapeCsv(item.GrupoItem)};{EscapeCsv(item.StatusPecaSnapshot)};{item.ValorRepasseSnapshot?.ToString("N2") ?? ""};{item.DataEvento:dd/MM/yyyy}");
        }

        foreach (var movement in detail.Movimentos)
        {
            builder.AppendLine(
                $"movimento;{EscapeCsv(movement.TipoMovimento)};{EscapeCsv(movement.Descricao)};{movement.Valor:N2};;{movement.DataMovimento:dd/MM/yyyy}");
        }

        return builder.ToString();
    }

    private static string BuildFileName(ClosingSummaryResponse summary, string extension)
    {
        var slug = summary.PessoaNome.Trim().ToLowerInvariant().Replace(' ', '-');
        return $"fechamento-{slug}-{summary.PeriodoInicio:yyyyMMdd}-{summary.PeriodoFim:yyyyMMdd}.{extension}";
    }

    private static string BuildExportUrl(Guid fechamentoId, string exportType)
    {
        return $"/api/v1/closings/{fechamentoId}/export/{exportType}";
    }

    private static string EscapeHtml(string value)
    {
        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
    }

    private static string EscapeCsv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    /// <summary>
    /// Carrega a pessoa vinculada a loja ativa e valida que ela participa do fechamento.
    /// </summary>
    private async Task<StorePersonContext> LoadStorePersonAsync(
        Guid lojaId,
        Guid pessoaId,
        CancellationToken cancellationToken)
    {
        var person = await (
                from relation in _dbContext.PessoaLojas.AsNoTracking()
                join item in _dbContext.Pessoas.AsNoTracking() on relation.PessoaId equals item.Id
                where relation.LojaId == lojaId
                where relation.PessoaId == pessoaId
                where relation.StatusRelacao == PeopleStatusValues.StatusRelacao.Ativo
                select new StorePersonContext(
                    item.Id,
                    item.Nome,
                    item.Documento,
                    relation.EhCliente,
                    relation.EhFornecedor,
                    relation.AceitaCreditoLoja))
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            throw new InvalidOperationException("Pessoa nao encontrada na loja ativa.");
        }

        if (!person.EhCliente && !person.EhFornecedor)
        {
            throw new InvalidOperationException("A pessoa precisa ser cliente, fornecedor ou ambos para gerar fechamento.");
        }

        return person;
    }

    /// <summary>
    /// Exige loja ativa, vinculo valido e permissao de consulta do modulo.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureClosingViewContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

        await EnsureStoreMembershipAsync(usuarioId, lojaId, cancellationToken);

        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.FechamentoGerar, AccessPermissionCodes.FechamentoConferir],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui acesso ao modulo de fechamento na loja ativa.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Exige permissao especifica para gerar ou regerar fechamentos.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureClosingGenerateContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureClosingViewContextAsync(cancellationToken);
        var hasPermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.FechamentoGerar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para gerar fechamentos.");
        }

        return context;
    }

    /// <summary>
    /// Exige permissao especifica para conferir e liquidar fechamentos.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureClosingReviewContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureClosingViewContextAsync(cancellationToken);
        var hasPermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.FechamentoConferir],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para conferir fechamentos.");
        }

        return context;
    }

    /// <summary>
    /// Garante que o usuario continua vinculado a loja ativa.
    /// </summary>
    private async Task EnsureStoreMembershipAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
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
    }

    /// <summary>
    /// Verifica se o usuario possui alguma permissao na matriz de cargos da loja.
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
    /// Converte a data inicial do filtro para o inicio do dia em UTC.
    /// </summary>
    private static DateTimeOffset ToUtcStart(DateOnly value)
    {
        return new DateTimeOffset(value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    /// <summary>
    /// Converte a data final do filtro para o fim do dia em UTC.
    /// </summary>
    private static DateTimeOffset ToUtcEnd(DateOnly value)
    {
        return new DateTimeOffset(value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddTicks(-1);
    }

    /// <summary>
    /// Arredonda valores monetarios para duas casas.
    /// </summary>
    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Gera um snapshot sintetico do fechamento para auditoria.
    /// </summary>
    private static object SnapshotClosing(FechamentoPessoa closing)
    {
        return new
        {
            closing.Id,
            closing.LojaId,
            closing.PessoaId,
            closing.PeriodoInicio,
            closing.PeriodoFim,
            closing.StatusFechamento,
            closing.ValorVendido,
            closing.ValorAReceber,
            closing.ValorPago,
            closing.ValorCompradoNaLoja,
            closing.SaldoFinal,
            closing.ResumoTexto,
            closing.PdfUrl,
            closing.ExcelUrl,
            closing.GeradoEm,
            closing.GeradoPorUsuarioId,
            closing.ConferidoEm,
            closing.ConferidoPorUsuarioId,
        };
    }

    // Reune a pessoa, papel na loja e dados basicos consumidos pelo modulo.
    private sealed record StorePersonContext(
        Guid PessoaId,
        string Nome,
        string Documento,
        bool EhCliente,
        bool EhFornecedor,
        bool AceitaCreditoLoja);

    // Agrupa o fechamento calculado antes da gravacao das tabelas snapshot.
    private sealed record GeneratedClosingData(
        IReadOnlyList<ClosingItemDraft> Items,
        IReadOnlyList<ClosingMovementDraft> Movements,
        decimal ValorVendido,
        decimal ValorAReceber,
        decimal ValorPago,
        decimal ValorCompradoNaLoja,
        decimal SaldoFinal,
        string ResumoTexto);

    // Mantem a estrutura intermediaria dos itens gravados no fechamento.
    private sealed record ClosingItemDraft(
        Guid PecaId,
        string StatusPecaSnapshot,
        decimal? ValorVendaSnapshot,
        decimal? ValorRepasseSnapshot,
        DateTimeOffset DataEvento);

    // Mantem a estrutura intermediaria dos movimentos gravados no fechamento.
    private sealed record ClosingMovementDraft(
        string TipoMovimento,
        string OrigemTipo,
        Guid? OrigemId,
        DateTimeOffset DataMovimento,
        string Descricao,
        decimal Valor);

    // Projeta uma venda de peca do fornecedor dentro do periodo.
    private sealed record SoldPieceRow(
        Guid PecaId,
        Guid VendaItemId,
        string CodigoInternoPeca,
        string NumeroVenda,
        DateTimeOffset DataVenda,
        decimal ValorVenda,
        decimal ValorRepasse);

    // Projeta uma peca ainda atual do fornecedor ate o fim do periodo.
    private sealed record CurrentPieceRow(
        Guid PecaId,
        string CodigoInternoPeca,
        string StatusPeca,
        DateTimeOffset DataEntrada);

    // Projeta uma liquidacao de obrigacao do fornecedor.
    private sealed record SupplierPaymentRow(
        Guid LiquidacaoId,
        string TipoLiquidacao,
        DateTimeOffset LiquidadoEm,
        decimal Valor,
        string Observacoes);

    // Projeta uma compra do cliente na loja durante o periodo.
    private sealed record CustomerPurchaseRow(
        Guid VendaId,
        string NumeroVenda,
        DateTimeOffset DataVenda,
        decimal TotalLiquido,
        string? Observacoes);

    // Projeta uma movimentacao da conta de credito da pessoa.
    private sealed record CreditMovementRow(
        Guid Id,
        string TipoMovimentacao,
        string OrigemTipo,
        Guid? OrigemId,
        DateTimeOffset MovimentadoEm,
        decimal Valor,
        string? Observacoes);
}
