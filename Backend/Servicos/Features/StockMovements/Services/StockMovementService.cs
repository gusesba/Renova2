using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Pieces;
using Renova.Services.Features.StockMovements.Abstractions;
using Renova.Services.Features.StockMovements.Contracts;

namespace Renova.Services.Features.StockMovements.Services;

// Implementa o modulo 08 com consulta de movimentacoes, busca operacional e ajuste manual.
public sealed class StockMovementService : IStockMovementService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public StockMovementService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega o resumo operacional e os filtros auxiliares do modulo.
    /// </summary>
    public async Task<StockMovementWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureStockContextAsync(cancellationToken);

        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var resumo = new StockMovementSummaryResponse(
            await _dbContext.MovimentacoesEstoque.CountAsync(x => x.LojaId == context.LojaId, cancellationToken),
            await _dbContext.MovimentacoesEstoque.CountAsync(
                x => x.LojaId == context.LojaId &&
                     x.TipoMovimentacao == PieceValues.StockMovementTypes.Ajuste,
                cancellationToken),
            await _dbContext.Pecas.CountAsync(x => x.LojaId == context.LojaId && x.QuantidadeAtual > 0, cancellationToken),
            await _dbContext.Pecas.CountAsync(x => x.LojaId == context.LojaId && x.QuantidadeAtual <= 0, cancellationToken));

        var fornecedoresRaw = await (
                from peca in _dbContext.Pecas.AsNoTracking()
                join pessoa in _dbContext.Pessoas.AsNoTracking() on peca.FornecedorPessoaId equals pessoa.Id
                where peca.LojaId == context.LojaId
                orderby pessoa.Nome
                select new
                {
                    pessoa.Id,
                    pessoa.Nome,
                    pessoa.Documento,
                })
            .ToListAsync(cancellationToken);

        var fornecedores = fornecedoresRaw
            .DistinctBy(x => x.Id)
            .Select(x => new StockSupplierOptionResponse(
                x.Id,
                x.Nome,
                x.Documento))
            .ToArray();

        return new StockMovementWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            resumo,
            fornecedores,
            BuildPieceStatusOptions(),
            BuildMovementTypeOptions());
    }

    /// <summary>
    /// Lista as movimentacoes de estoque aplicando filtros por peca, periodo e contexto.
    /// </summary>
    public async Task<IReadOnlyList<StockMovementItemResponse>> ListarAsync(
        StockMovementListQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureStockContextAsync(cancellationToken);

        var movementQuery =
            from movimento in _dbContext.MovimentacoesEstoque.AsNoTracking()
            join peca in _dbContext.Pecas.AsNoTracking() on movimento.PecaId equals peca.Id
            join produto in _dbContext.ProdutoNomes.AsNoTracking() on peca.ProdutoNomeId equals produto.Id
            join marca in _dbContext.Marcas.AsNoTracking() on peca.MarcaId equals marca.Id
            join tamanho in _dbContext.Tamanhos.AsNoTracking() on peca.TamanhoId equals tamanho.Id
            join cor in _dbContext.Cores.AsNoTracking() on peca.CorId equals cor.Id
            join fornecedor in _dbContext.Pessoas.AsNoTracking() on peca.FornecedorPessoaId equals fornecedor.Id into supplierGroup
            from fornecedor in supplierGroup.DefaultIfEmpty()
            where movimento.LojaId == context.LojaId
            select new
            {
                Movimento = movimento,
                Peca = peca,
                ProdutoNome = produto.Nome,
                Marca = marca.Nome,
                Tamanho = tamanho.Nome,
                Cor = cor.Nome,
                FornecedorNome = fornecedor != null ? fornecedor.Nome : null,
            };

        if (query.PecaId.HasValue)
        {
            movementQuery = movementQuery.Where(x => x.Peca.Id == query.PecaId.Value);
        }

        if (query.FornecedorPessoaId.HasValue)
        {
            movementQuery = movementQuery.Where(x => x.Peca.FornecedorPessoaId == query.FornecedorPessoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.StatusPeca))
        {
            var normalizedStatus = PieceValues.NormalizePieceStatus(query.StatusPeca);
            movementQuery = movementQuery.Where(x => x.Peca.StatusPeca == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(query.TipoMovimentacao))
        {
            var normalizedMovementType = query.TipoMovimentacao.Trim().ToLowerInvariant();
            movementQuery = movementQuery.Where(x => x.Movimento.TipoMovimentacao == normalizedMovementType);
        }

        if (query.DataInicial.HasValue)
        {
            movementQuery = movementQuery.Where(x => x.Movimento.MovimentadoEm >= query.DataInicial.Value);
        }

        if (query.DataFinal.HasValue)
        {
            movementQuery = movementQuery.Where(x => x.Movimento.MovimentadoEm <= query.DataFinal.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            movementQuery = movementQuery.Where(x =>
                x.Peca.CodigoInterno.ToLower().Contains(term) ||
                x.Peca.CodigoBarras.ToLower().Contains(term) ||
                x.ProdutoNome.ToLower().Contains(term) ||
                x.Marca.ToLower().Contains(term) ||
                x.Tamanho.ToLower().Contains(term) ||
                x.Cor.ToLower().Contains(term) ||
                (x.FornecedorNome ?? string.Empty).ToLower().Contains(term) ||
                x.Movimento.TipoMovimentacao.ToLower().Contains(term) ||
                x.Movimento.Motivo.ToLower().Contains(term));
        }

        var items = await movementQuery
            .OrderByDescending(x => x.Movimento.MovimentadoEm)
            .ThenByDescending(x => x.Movimento.CriadoEm)
            .ToListAsync(cancellationToken);

        return items
            .Select(item => new StockMovementItemResponse(
                item.Movimento.Id,
                item.Peca.Id,
                item.Peca.CodigoInterno,
                item.Peca.CodigoBarras,
                item.ProdutoNome,
                item.Marca,
                item.Tamanho,
                item.Cor,
                item.Peca.FornecedorPessoaId,
                item.FornecedorNome,
                item.Peca.StatusPeca,
                item.Movimento.TipoMovimentacao,
                item.Movimento.Quantidade,
                item.Movimento.SaldoAnterior,
                item.Movimento.SaldoPosterior,
                item.Movimento.OrigemTipo,
                item.Movimento.OrigemId,
                item.Movimento.Motivo,
                item.Movimento.MovimentadoEm,
                item.Movimento.MovimentadoPorUsuarioId,
                item.Peca.QuantidadeAtual,
                CalculateDaysInStore(item.Peca.DataEntrada)))
            .ToArray();
    }

    /// <summary>
    /// Busca pecas da loja ativa para uso operacional e ajustes rapidos.
    /// </summary>
    public async Task<IReadOnlyList<StockPieceLookupResponse>> BuscarPecasAsync(
        StockPieceSearchQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureStockContextAsync(cancellationToken);

        var pieceQuery =
            from peca in _dbContext.Pecas.AsNoTracking()
            join produto in _dbContext.ProdutoNomes.AsNoTracking() on peca.ProdutoNomeId equals produto.Id
            join marca in _dbContext.Marcas.AsNoTracking() on peca.MarcaId equals marca.Id
            join tamanho in _dbContext.Tamanhos.AsNoTracking() on peca.TamanhoId equals tamanho.Id
            join cor in _dbContext.Cores.AsNoTracking() on peca.CorId equals cor.Id
            join fornecedor in _dbContext.Pessoas.AsNoTracking() on peca.FornecedorPessoaId equals fornecedor.Id into supplierGroup
            from fornecedor in supplierGroup.DefaultIfEmpty()
            where peca.LojaId == context.LojaId
            select new
            {
                Peca = peca,
                ProdutoNome = produto.Nome,
                Marca = marca.Nome,
                Tamanho = tamanho.Nome,
                Cor = cor.Nome,
                FornecedorNome = fornecedor != null ? fornecedor.Nome : null,
            };

        if (!string.IsNullOrWhiteSpace(query.CodigoBarras))
        {
            var barcode = query.CodigoBarras.Trim();
            pieceQuery = pieceQuery.Where(x => x.Peca.CodigoBarras == barcode);
        }

        if (query.FornecedorPessoaId.HasValue)
        {
            pieceQuery = pieceQuery.Where(x => x.Peca.FornecedorPessoaId == query.FornecedorPessoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.StatusPeca))
        {
            var normalizedStatus = PieceValues.NormalizePieceStatus(query.StatusPeca);
            pieceQuery = pieceQuery.Where(x => x.Peca.StatusPeca == normalizedStatus);
        }

        if (query.TempoMinimoLojaDias.HasValue && query.TempoMinimoLojaDias.Value > 0)
        {
            var startDateLimit = DateTimeOffset.UtcNow.AddDays(-query.TempoMinimoLojaDias.Value);
            pieceQuery = pieceQuery.Where(x => x.Peca.DataEntrada <= startDateLimit);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            pieceQuery = pieceQuery.Where(x =>
                x.Peca.CodigoInterno.ToLower().Contains(term) ||
                x.Peca.CodigoBarras.ToLower().Contains(term) ||
                x.ProdutoNome.ToLower().Contains(term) ||
                x.Marca.ToLower().Contains(term) ||
                x.Tamanho.ToLower().Contains(term) ||
                x.Cor.ToLower().Contains(term) ||
                (x.FornecedorNome ?? string.Empty).ToLower().Contains(term) ||
                x.Peca.LocalizacaoFisica.ToLower().Contains(term));
        }

        var latestMovements = await _dbContext.MovimentacoesEstoque
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .GroupBy(x => x.PecaId)
            .Select(group => new
            {
                PecaId = group.Key,
                UltimaMovimentacaoEm = group.Max(x => x.MovimentadoEm),
            })
            .ToListAsync(cancellationToken);

        var latestMovementMap = latestMovements.ToDictionary(x => x.PecaId, x => x.UltimaMovimentacaoEm);

        var items = await pieceQuery
            .OrderByDescending(x => x.Peca.DataEntrada)
            .ThenBy(x => x.Peca.CodigoInterno)
            .ToListAsync(cancellationToken);

        return items
            .Select(item => new StockPieceLookupResponse(
                item.Peca.Id,
                item.Peca.CodigoInterno,
                item.Peca.CodigoBarras,
                item.Peca.TipoPeca,
                item.Peca.StatusPeca,
                item.ProdutoNome,
                item.Marca,
                item.Tamanho,
                item.Cor,
                item.Peca.FornecedorPessoaId,
                item.FornecedorNome,
                item.Peca.DataEntrada,
                CalculateDaysInStore(item.Peca.DataEntrada),
                item.Peca.QuantidadeAtual,
                item.Peca.LocalizacaoFisica,
                CanBeSold(item.Peca),
                latestMovementMap.GetValueOrDefault(item.Peca.Id)))
            .ToArray();
    }

    /// <summary>
    /// Registra um ajuste manual atualizando saldo e opcionalmente o status da peca.
    /// </summary>
    public async Task<AdjustStockResponse> AjustarAsync(
        AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureStockManageContextAsync(cancellationToken);

        if (request.QuantidadeNova < 0)
        {
            throw new InvalidOperationException("Informe uma quantidade nova igual ou maior que zero.");
        }

        var motivo = NormalizeRequiredText(request.Motivo, "Informe o motivo do ajuste manual.");

        var peca = await _dbContext.Pecas
            .FirstOrDefaultAsync(x => x.Id == request.PecaId && x.LojaId == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Peca nao encontrada na loja ativa.");

        var before = SnapshotPiece(peca);
        var quantidadeAnterior = peca.QuantidadeAtual;
        var statusAnterior = peca.StatusPeca;
        var statusNovo = ResolveAdjustedStatus(peca.StatusPeca, request.StatusPeca, request.QuantidadeNova);

        if (quantidadeAnterior == request.QuantidadeNova && statusAnterior == statusNovo)
        {
            throw new InvalidOperationException("Informe uma alteracao real de saldo ou status para registrar o ajuste.");
        }

        peca.QuantidadeAtual = request.QuantidadeNova;
        peca.StatusPeca = statusNovo;
        TouchEntity(peca, context.UsuarioId);

        var movement = new MovimentacaoEstoque
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            PecaId = peca.Id,
            TipoMovimentacao = PieceValues.StockMovementTypes.Ajuste,
            Quantidade = Math.Abs(request.QuantidadeNova - quantidadeAnterior),
            SaldoAnterior = quantidadeAnterior,
            SaldoPosterior = request.QuantidadeNova,
            OrigemTipo = PieceValues.StockOrigins.AjusteManual,
            OrigemId = peca.Id,
            Motivo = motivo,
            MovimentadoEm = DateTimeOffset.UtcNow,
            MovimentadoPorUsuarioId = context.UsuarioId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.MovimentacoesEstoque.Add(movement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "peca",
            peca.Id,
            "ajuste_manual_estoque",
            before,
            SnapshotPiece(peca),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "movimentacao_estoque",
            movement.Id,
            "criada",
            null,
            SnapshotStockMovement(movement),
            cancellationToken);

        return new AdjustStockResponse(
            movement.Id,
            peca.Id,
            peca.CodigoInterno,
            quantidadeAnterior,
            peca.QuantidadeAtual,
            statusAnterior,
            peca.StatusPeca,
            movement.MovimentadoEm,
            movement.Motivo);
    }

    /// <summary>
    /// Garante usuario autenticado, loja ativa e permissao de consulta no modulo.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureStockContextAsync(CancellationToken cancellationToken)
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
            throw new InvalidOperationException("Voce nao tem acesso ao modulo de movimentacoes de estoque na loja ativa.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Garante permissao especifica de ajuste para operacoes mutaveis.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureStockManageContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureStockContextAsync(cancellationToken);
        var hasPermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.PecasAjustar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para registrar ajustes manuais de estoque.");
        }

        return context;
    }

    /// <summary>
    /// Verifica se o usuario possui ao menos uma permissao dentro da loja ativa.
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
    /// Define o status final do ajuste, preservando o atual quando nao houver valor informado.
    /// </summary>
    private static string ResolveAdjustedStatus(string currentStatus, string? requestedStatus, int quantidadeNova)
    {
        if (!string.IsNullOrWhiteSpace(requestedStatus))
        {
            return PieceValues.NormalizePieceStatus(requestedStatus);
        }

        if (quantidadeNova == 0 && currentStatus == PieceValues.PieceStatuses.Disponivel)
        {
            return PieceValues.PieceStatuses.Inativa;
        }

        return PieceValues.NormalizePieceStatus(currentStatus);
    }

    /// <summary>
    /// Define se a peca atual pode ser usada numa venda.
    /// </summary>
    private static bool CanBeSold(Peca peca)
    {
        return peca.QuantidadeAtual > 0 &&
               PieceValues.NormalizePieceStatus(peca.StatusPeca) is
                   PieceValues.PieceStatuses.Disponivel or PieceValues.PieceStatuses.Reservada;
    }

    /// <summary>
    /// Calcula os dias corridos desde a entrada da peca na loja.
    /// </summary>
    private static int CalculateDaysInStore(DateTimeOffset dataEntrada)
    {
        return Math.Max(0, (int)Math.Floor((DateTimeOffset.UtcNow - dataEntrada).TotalDays));
    }

    /// <summary>
    /// Normaliza um texto obrigatorio informado pela tela.
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
    /// Atualiza os metadados comuns das entidades auditaveis.
    /// </summary>
    private static void TouchEntity(AuditEntityBase entity, Guid usuarioId)
    {
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Expone os status operacionais permitidos para filtro e ajuste.
    /// </summary>
    private static IReadOnlyList<StockOptionResponse> BuildPieceStatusOptions()
    {
        return
        [
            new(PieceValues.PieceStatuses.Disponivel, "Disponivel"),
            new(PieceValues.PieceStatuses.Reservada, "Reservada"),
            new(PieceValues.PieceStatuses.Vendida, "Vendida"),
            new(PieceValues.PieceStatuses.Devolvida, "Devolvida"),
            new(PieceValues.PieceStatuses.Doada, "Doada"),
            new(PieceValues.PieceStatuses.Perdida, "Perdida"),
            new(PieceValues.PieceStatuses.Descartada, "Descartada"),
            new(PieceValues.PieceStatuses.Inativa, "Inativa"),
        ];
    }

    /// <summary>
    /// Expone os tipos de movimentacao usados no historico operacional.
    /// </summary>
    private static IReadOnlyList<StockOptionResponse> BuildMovementTypeOptions()
    {
        return
        [
            new(PieceValues.StockMovementTypes.Entrada, "Entrada"),
            new(PieceValues.StockMovementTypes.Ajuste, "Ajuste manual"),
            new(PieceValues.StockMovementTypes.Venda, "Venda"),
            new(PieceValues.StockMovementTypes.CancelamentoVenda, "Cancelamento de venda"),
            new(PieceValues.StockMovementTypes.Devolucao, "Devolucao"),
            new(PieceValues.StockMovementTypes.Doacao, "Doacao"),
            new(PieceValues.StockMovementTypes.Perda, "Perda"),
            new(PieceValues.StockMovementTypes.Descarte, "Descarte"),
        ];
    }

    /// <summary>
    /// Resume a peca para a trilha de auditoria.
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
            entity.StatusPeca,
            entity.QuantidadeAtual,
            entity.LocalizacaoFisica,
        };
    }

    /// <summary>
    /// Resume a movimentacao de estoque para auditoria.
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
}
