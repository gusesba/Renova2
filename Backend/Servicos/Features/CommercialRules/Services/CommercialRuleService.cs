using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.CommercialRules.Abstractions;
using Renova.Services.Features.CommercialRules.Contracts;

namespace Renova.Services.Features.CommercialRules.Services;

// Implementa o modulo 05 com regras comerciais e meios de pagamento por loja.
public sealed class CommercialRuleService : ICommercialRuleService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public CommercialRuleService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega regra da loja, regras por fornecedor e meios de pagamento da loja ativa.
    /// </summary>
    public async Task<CommercialRulesWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureCommercialContextAsync(cancellationToken);

        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var regraLojaEntity = await _dbContext.LojaRegrasComerciais
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .FirstOrDefaultAsync(cancellationToken);

        var regrasFornecedor = await (
                from regra in _dbContext.FornecedorRegrasComerciais
                join pessoaLoja in _dbContext.PessoaLojas on regra.PessoaLojaId equals pessoaLoja.Id
                join pessoa in _dbContext.Pessoas on pessoaLoja.PessoaId equals pessoa.Id
                where pessoaLoja.LojaId == context.LojaId
                orderby pessoa.Nome
                select new
                {
                    Regra = regra,
                    PessoaLoja = pessoaLoja,
                    Pessoa = pessoa,
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var fornecedores = await (
                from pessoaLoja in _dbContext.PessoaLojas
                join pessoa in _dbContext.Pessoas on pessoaLoja.PessoaId equals pessoa.Id
                where pessoaLoja.LojaId == context.LojaId
                where pessoaLoja.EhFornecedor
                orderby pessoa.Nome
                select new SupplierRuleOptionResponse(
                    pessoaLoja.Id,
                    pessoa.Id,
                    pessoa.Nome,
                    pessoa.Documento,
                    pessoaLoja.StatusRelacao))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var meiosPagamentoEntities = await _dbContext.MeiosPagamento
            .AsNoTracking()
            .Where(x => x.LojaId == context.LojaId)
            .OrderByDescending(x => x.Ativo)
            .ThenBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        return new CommercialRulesWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            regraLojaEntity is null ? null : MapStoreRule(regraLojaEntity),
            regrasFornecedor
                .Select(item => MapSupplierRule(item.Regra, item.PessoaLoja, item.Pessoa))
                .ToArray(),
            fornecedores,
            meiosPagamentoEntities.Select(MapPaymentMethod).ToArray(),
            BuildPaymentMethodTypes());
    }

    /// <summary>
    /// Cria ou atualiza a regra padrao da loja ativa.
    /// </summary>
    public async Task<StoreCommercialRuleResponse> SalvarRegraLojaAsync(
        UpsertStoreCommercialRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCommercialContextAsync(cancellationToken);
        ValidateRulePayload(
            request.PercentualRepasseDinheiro,
            request.PercentualRepasseCredito,
            request.TempoMaximoExposicaoDias,
            request.PoliticaDesconto);

        var entity = await _dbContext.LojaRegrasComerciais
            .FirstOrDefaultAsync(x => x.LojaId == context.LojaId, cancellationToken);

        var before = entity is null ? null : SnapshotStoreRule(entity);
        var action = entity is null ? "criada" : "atualizada";

        if (entity is null)
        {
            entity = new LojaRegraComercial
            {
                Id = Guid.NewGuid(),
                LojaId = context.LojaId,
                CriadoPorUsuarioId = context.UsuarioId,
            };

            _dbContext.LojaRegrasComerciais.Add(entity);
        }

        ApplyRuleData(
            entity,
            request.PercentualRepasseDinheiro,
            request.PercentualRepasseCredito,
            request.PermitePagamentoMisto,
            request.TempoMaximoExposicaoDias,
            request.Ativo,
            request.PoliticaDesconto,
            context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "loja_regra_comercial",
            entity.Id,
            action,
            before,
            SnapshotStoreRule(entity),
            cancellationToken);

        return MapStoreRule(entity);
    }

    /// <summary>
    /// Cria uma sobrescrita comercial para fornecedor da loja ativa.
    /// </summary>
    public async Task<SupplierCommercialRuleResponse> CriarRegraFornecedorAsync(
        CreateSupplierCommercialRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCommercialContextAsync(cancellationToken);
        ValidateRulePayload(
            request.PercentualRepasseDinheiro,
            request.PercentualRepasseCredito,
            request.TempoMaximoExposicaoDias,
            request.PoliticaDesconto);

        var supplierRelation = await EnsureSupplierRelationAsync(request.PessoaLojaId, context.LojaId, cancellationToken);

        var alreadyExists = await _dbContext.FornecedorRegrasComerciais.AnyAsync(
            x => x.PessoaLojaId == request.PessoaLojaId,
            cancellationToken);

        if (alreadyExists)
        {
            throw new InvalidOperationException("Ja existe regra comercial cadastrada para este fornecedor.");
        }

        var entity = new FornecedorRegraComercial
        {
            Id = Guid.NewGuid(),
            PessoaLojaId = request.PessoaLojaId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        ApplyRuleData(
            entity,
            request.PercentualRepasseDinheiro,
            request.PercentualRepasseCredito,
            request.PermitePagamentoMisto,
            request.TempoMaximoExposicaoDias,
            request.Ativo,
            request.PoliticaDesconto,
            context.UsuarioId);

        _dbContext.FornecedorRegrasComerciais.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "fornecedor_regra_comercial",
            entity.Id,
            "criada",
            null,
            SnapshotSupplierRule(entity),
            cancellationToken);

        var pessoa = await _dbContext.Pessoas
            .AsNoTracking()
            .FirstAsync(x => x.Id == supplierRelation.PessoaId, cancellationToken);

        return MapSupplierRule(entity, supplierRelation, pessoa);
    }

    /// <summary>
    /// Atualiza uma regra comercial de fornecedor pertencente a loja ativa.
    /// </summary>
    public async Task<SupplierCommercialRuleResponse> AtualizarRegraFornecedorAsync(
        Guid regraFornecedorId,
        UpdateSupplierCommercialRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCommercialContextAsync(cancellationToken);
        ValidateRulePayload(
            request.PercentualRepasseDinheiro,
            request.PercentualRepasseCredito,
            request.TempoMaximoExposicaoDias,
            request.PoliticaDesconto);

        var entity = await (
                from regra in _dbContext.FornecedorRegrasComerciais
                join pessoaLoja in _dbContext.PessoaLojas on regra.PessoaLojaId equals pessoaLoja.Id
                where regra.Id == regraFornecedorId
                where pessoaLoja.LojaId == context.LojaId
                select regra)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Regra comercial do fornecedor nao encontrada.");

        var supplierRelation = await EnsureSupplierRelationAsync(entity.PessoaLojaId, context.LojaId, cancellationToken);
        var before = SnapshotSupplierRule(entity);

        ApplyRuleData(
            entity,
            request.PercentualRepasseDinheiro,
            request.PercentualRepasseCredito,
            request.PermitePagamentoMisto,
            request.TempoMaximoExposicaoDias,
            request.Ativo,
            request.PoliticaDesconto,
            context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "fornecedor_regra_comercial",
            entity.Id,
            "atualizada",
            before,
            SnapshotSupplierRule(entity),
            cancellationToken);

        var pessoa = await _dbContext.Pessoas
            .AsNoTracking()
            .FirstAsync(x => x.Id == supplierRelation.PessoaId, cancellationToken);

        return MapSupplierRule(entity, supplierRelation, pessoa);
    }

    /// <summary>
    /// Cria um meio de pagamento configurado para a loja ativa.
    /// </summary>
    public async Task<PaymentMethodResponse> CriarMeioPagamentoAsync(
        CreatePaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCommercialContextAsync(cancellationToken);
        ValidatePaymentMethodPayload(
            request.Nome,
            request.TipoMeioPagamento,
            request.TaxaPercentual,
            request.PrazoRecebimentoDias);

        var normalizedName = NormalizeRequiredText(request.Nome, "Informe o nome do meio de pagamento.");
        var normalizedType = CommercialRuleValues.NormalizePaymentMethodType(request.TipoMeioPagamento);

        await EnsureUniquePaymentMethodNameAsync(context.LojaId, normalizedName, null, cancellationToken);

        var entity = new MeioPagamento
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        ApplyPaymentMethodData(
            entity,
            normalizedName,
            normalizedType,
            request.TaxaPercentual,
            request.PrazoRecebimentoDias,
            request.Ativo,
            context.UsuarioId);

        _dbContext.MeiosPagamento.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "meio_pagamento",
            entity.Id,
            "criado",
            null,
            SnapshotPaymentMethod(entity),
            cancellationToken);

        return MapPaymentMethod(entity);
    }

    /// <summary>
    /// Atualiza um meio de pagamento existente da loja ativa.
    /// </summary>
    public async Task<PaymentMethodResponse> AtualizarMeioPagamentoAsync(
        Guid meioPagamentoId,
        UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCommercialContextAsync(cancellationToken);
        ValidatePaymentMethodPayload(
            request.Nome,
            request.TipoMeioPagamento,
            request.TaxaPercentual,
            request.PrazoRecebimentoDias);

        var entity = await _dbContext.MeiosPagamento
            .FirstOrDefaultAsync(x => x.Id == meioPagamentoId && x.LojaId == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Meio de pagamento nao encontrado na loja ativa.");

        var normalizedName = NormalizeRequiredText(request.Nome, "Informe o nome do meio de pagamento.");
        var normalizedType = CommercialRuleValues.NormalizePaymentMethodType(request.TipoMeioPagamento);
        await EnsureUniquePaymentMethodNameAsync(context.LojaId, normalizedName, entity.Id, cancellationToken);

        var before = SnapshotPaymentMethod(entity);

        ApplyPaymentMethodData(
            entity,
            normalizedName,
            normalizedType,
            request.TaxaPercentual,
            request.PrazoRecebimentoDias,
            request.Ativo,
            context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "meio_pagamento",
            entity.Id,
            "atualizado",
            before,
            SnapshotPaymentMethod(entity),
            cancellationToken);

        return MapPaymentMethod(entity);
    }

    /// <summary>
    /// Garante usuario autenticado, loja ativa e permissao de gerenciamento comercial.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureCommercialContextAsync(CancellationToken cancellationToken)
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
            [AccessPermissionCodes.RegrasGerenciar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao tem acesso para gerenciar regras comerciais na loja ativa.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Exige que a relacao pessoa x loja exista e seja de fornecedor.
    /// </summary>
    private async Task<PessoaLoja> EnsureSupplierRelationAsync(Guid pessoaLojaId, Guid lojaId, CancellationToken cancellationToken)
    {
        var relation = await _dbContext.PessoaLojas
            .FirstOrDefaultAsync(x => x.Id == pessoaLojaId && x.LojaId == lojaId, cancellationToken)
            ?? throw new InvalidOperationException("Fornecedor nao encontrado na loja ativa.");

        if (!relation.EhFornecedor)
        {
            throw new InvalidOperationException("A pessoa informada nao esta vinculada como fornecedor na loja ativa.");
        }

        return relation;
    }

    /// <summary>
    /// Verifica se o usuario possui a permissao comercial exigida pela feature.
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
    /// Valida os campos de uma regra comercial.
    /// </summary>
    private static void ValidateRulePayload(
        decimal percentualRepasseDinheiro,
        decimal percentualRepasseCredito,
        int tempoMaximoExposicaoDias,
        IReadOnlyList<CommercialDiscountBandRequest> politicaDesconto)
    {
        ValidatePercentage(percentualRepasseDinheiro, "Informe um percentual de repasse em dinheiro entre 0 e 100.");
        ValidatePercentage(percentualRepasseCredito, "Informe um percentual de repasse em credito entre 0 e 100.");

        if (tempoMaximoExposicaoDias <= 0)
        {
            throw new InvalidOperationException("Informe um prazo maximo de exposicao maior que zero.");
        }

        ValidateDiscountBands(politicaDesconto);
    }

    /// <summary>
    /// Valida os campos principais de um meio de pagamento.
    /// </summary>
    private static void ValidatePaymentMethodPayload(
        string nome,
        string tipoMeioPagamento,
        decimal taxaPercentual,
        int prazoRecebimentoDias)
    {
        _ = NormalizeRequiredText(nome, "Informe o nome do meio de pagamento.");
        _ = CommercialRuleValues.NormalizePaymentMethodType(tipoMeioPagamento);
        ValidatePercentage(taxaPercentual, "Informe uma taxa percentual entre 0 e 100.");

        if (prazoRecebimentoDias < 0)
        {
            throw new InvalidOperationException("Informe um prazo de recebimento igual ou maior que zero.");
        }
    }

    /// <summary>
    /// Garante unicidade do nome do meio de pagamento dentro da loja.
    /// </summary>
    private async Task EnsureUniquePaymentMethodNameAsync(
        Guid lojaId,
        string nome,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        var normalized = nome.ToLowerInvariant();
        var exists = await _dbContext.MeiosPagamento.AnyAsync(
            x => x.LojaId == lojaId &&
                 x.Id != currentId &&
                 x.Nome.ToLower() == normalized,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ja existe um meio de pagamento com este nome na loja ativa.");
        }
    }

    /// <summary>
    /// Aplica os dados comuns de uma regra comercial de loja.
    /// </summary>
    private static void ApplyRuleData(
        LojaRegraComercial entity,
        decimal percentualRepasseDinheiro,
        decimal percentualRepasseCredito,
        bool permitePagamentoMisto,
        int tempoMaximoExposicaoDias,
        bool ativo,
        IReadOnlyList<CommercialDiscountBandRequest> politicaDesconto,
        Guid usuarioId)
    {
        entity.PercentualRepasseDinheiro = percentualRepasseDinheiro;
        entity.PercentualRepasseCredito = percentualRepasseCredito;
        entity.PermitePagamentoMisto = permitePagamentoMisto;
        entity.TempoMaximoExposicaoDias = tempoMaximoExposicaoDias;
        entity.PoliticaDescontoJson = SerializeDiscountBands(politicaDesconto);
        entity.Ativo = ativo;
        entity.InativadoEm = ativo ? null : DateTimeOffset.UtcNow;
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Aplica os dados comuns de uma regra comercial de fornecedor.
    /// </summary>
    private static void ApplyRuleData(
        FornecedorRegraComercial entity,
        decimal percentualRepasseDinheiro,
        decimal percentualRepasseCredito,
        bool permitePagamentoMisto,
        int tempoMaximoExposicaoDias,
        bool ativo,
        IReadOnlyList<CommercialDiscountBandRequest> politicaDesconto,
        Guid usuarioId)
    {
        entity.PercentualRepasseDinheiro = percentualRepasseDinheiro;
        entity.PercentualRepasseCredito = percentualRepasseCredito;
        entity.PermitePagamentoMisto = permitePagamentoMisto;
        entity.TempoMaximoExposicaoDias = tempoMaximoExposicaoDias;
        entity.PoliticaDescontoJson = SerializeDiscountBands(politicaDesconto);
        entity.Ativo = ativo;
        entity.InativadoEm = ativo ? null : DateTimeOffset.UtcNow;
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Aplica os campos de um meio de pagamento.
    /// </summary>
    private static void ApplyPaymentMethodData(
        MeioPagamento entity,
        string nome,
        string tipoMeioPagamento,
        decimal taxaPercentual,
        int prazoRecebimentoDias,
        bool ativo,
        Guid usuarioId)
    {
        entity.Nome = nome;
        entity.TipoMeioPagamento = tipoMeioPagamento;
        entity.TaxaPercentual = taxaPercentual;
        entity.PrazoRecebimentoDias = prazoRecebimentoDias;
        entity.Ativo = ativo;
        entity.InativadoEm = ativo ? null : DateTimeOffset.UtcNow;
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Serializa a politica de desconto para a entidade persistida.
    /// </summary>
    private static string SerializeDiscountBands(IReadOnlyList<CommercialDiscountBandRequest> bands)
    {
        var normalizedBands = bands
            .OrderBy(x => x.DiasMinimos)
            .Select(x => new CommercialDiscountBandResponse(x.DiasMinimos, x.PercentualDesconto))
            .ToArray();

        return CommercialRulePolicySerializer.Serialize(normalizedBands);
    }

    /// <summary>
    /// Valida as faixas de desconto configuradas para a regra comercial.
    /// </summary>
    private static void ValidateDiscountBands(IReadOnlyList<CommercialDiscountBandRequest> bands)
    {
        var days = new HashSet<int>();
        foreach (var band in bands)
        {
            if (band.DiasMinimos <= 0)
            {
                throw new InvalidOperationException("Cada faixa de desconto deve informar dias minimos maiores que zero.");
            }

            if (!days.Add(band.DiasMinimos))
            {
                throw new InvalidOperationException("Nao repita a mesma faixa de dias na politica de desconto.");
            }

            ValidatePercentage(
                band.PercentualDesconto,
                "Cada percentual de desconto deve ficar entre 0 e 100.");
        }
    }

    /// <summary>
    /// Valida percentuais comerciais e financeiros.
    /// </summary>
    private static void ValidatePercentage(decimal value, string errorMessage)
    {
        if (value < 0 || value > 100)
        {
            throw new InvalidOperationException(errorMessage);
        }
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
    /// Mapeia a regra comercial da loja para o contrato da API.
    /// </summary>
    private static StoreCommercialRuleResponse MapStoreRule(LojaRegraComercial entity)
    {
        return new StoreCommercialRuleResponse(
            entity.Id,
            entity.LojaId,
            entity.PercentualRepasseDinheiro,
            entity.PercentualRepasseCredito,
            entity.PermitePagamentoMisto,
            entity.TempoMaximoExposicaoDias,
            CommercialRulePolicySerializer.Deserialize(entity.PoliticaDescontoJson),
            entity.Ativo);
    }

    /// <summary>
    /// Mapeia a regra de fornecedor para o contrato de leitura da API.
    /// </summary>
    private static SupplierCommercialRuleResponse MapSupplierRule(
        FornecedorRegraComercial entity,
        PessoaLoja pessoaLoja,
        Pessoa pessoa)
    {
        return new SupplierCommercialRuleResponse(
            entity.Id,
            pessoaLoja.Id,
            pessoa.Id,
            pessoa.Nome,
            pessoa.Documento,
            entity.PercentualRepasseDinheiro,
            entity.PercentualRepasseCredito,
            entity.PermitePagamentoMisto,
            entity.TempoMaximoExposicaoDias,
            CommercialRulePolicySerializer.Deserialize(entity.PoliticaDescontoJson),
            entity.Ativo);
    }

    /// <summary>
    /// Mapeia o meio de pagamento para o contrato consumido pelo frontend.
    /// </summary>
    private static PaymentMethodResponse MapPaymentMethod(MeioPagamento entity)
    {
        return new PaymentMethodResponse(
            entity.Id,
            entity.LojaId,
            entity.Nome,
            entity.TipoMeioPagamento,
            entity.TaxaPercentual,
            entity.PrazoRecebimentoDias,
            entity.Ativo);
    }

    /// <summary>
    /// Expone os tipos validos de meio de pagamento para o frontend.
    /// </summary>
    private static IReadOnlyList<PaymentMethodTypeOptionResponse> BuildPaymentMethodTypes()
    {
        return
        [
            new(CommercialRuleValues.PaymentMethodTypes.Dinheiro, "Dinheiro"),
            new(CommercialRuleValues.PaymentMethodTypes.Pix, "PIX"),
            new(CommercialRuleValues.PaymentMethodTypes.CartaoCredito, "Cartao de credito"),
            new(CommercialRuleValues.PaymentMethodTypes.CartaoDebito, "Cartao de debito"),
            new(CommercialRuleValues.PaymentMethodTypes.Outro, "Outro"),
        ];
    }

    /// <summary>
    /// Resume a regra da loja para auditoria.
    /// </summary>
    private static object SnapshotStoreRule(LojaRegraComercial entity)
    {
        return new
        {
            entity.LojaId,
            entity.PercentualRepasseDinheiro,
            entity.PercentualRepasseCredito,
            entity.PermitePagamentoMisto,
            entity.TempoMaximoExposicaoDias,
            PoliticaDesconto = CommercialRulePolicySerializer.Deserialize(entity.PoliticaDescontoJson),
            entity.Ativo,
        };
    }

    /// <summary>
    /// Resume a regra do fornecedor para auditoria.
    /// </summary>
    private static object SnapshotSupplierRule(FornecedorRegraComercial entity)
    {
        return new
        {
            entity.PessoaLojaId,
            entity.PercentualRepasseDinheiro,
            entity.PercentualRepasseCredito,
            entity.PermitePagamentoMisto,
            entity.TempoMaximoExposicaoDias,
            PoliticaDesconto = CommercialRulePolicySerializer.Deserialize(entity.PoliticaDescontoJson),
            entity.Ativo,
        };
    }

    /// <summary>
    /// Resume o meio de pagamento para auditoria.
    /// </summary>
    private static object SnapshotPaymentMethod(MeioPagamento entity)
    {
        return new
        {
            entity.LojaId,
            entity.Nome,
            entity.TipoMeioPagamento,
            entity.TaxaPercentual,
            entity.PrazoRecebimentoDias,
            entity.Ativo,
        };
    }
}
