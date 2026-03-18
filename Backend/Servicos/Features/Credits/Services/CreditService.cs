using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Credits.Abstractions;
using Renova.Services.Features.Credits.Contracts;

namespace Renova.Services.Features.Credits.Services;

// Implementa o modulo 10 com conta por pessoa, extrato e lancamentos de credito.
public sealed class CreditService : ICreditService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public CreditService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Carrega a visao geral do modulo para a loja ativa.
    /// </summary>
    public async Task<CreditsWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureCreditViewContextAsync(cancellationToken);
        var loja = await _dbContext.Lojas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja ativa nao encontrada.");

        var people = await LoadStorePeopleAsync(context.LojaId, cancellationToken);
        var summaries = await BuildAccountSummariesAsync(context.LojaId, people, cancellationToken);

        return new CreditsWorkspaceResponse(
            loja.Id,
            loja.NomeFantasia,
            summaries,
            people
                .OrderBy(x => x.Pessoa.Nome)
                .Select(x => new CreditPersonOptionResponse(
                    x.Pessoa.Id,
                    x.Pessoa.Nome,
                    x.Pessoa.Documento,
                    x.Pessoa.TipoPessoa,
                    x.Relacao.EhCliente,
                    x.Relacao.EhFornecedor,
                    x.Relacao.AceitaCreditoLoja,
                    x.Relacao.StatusRelacao,
                    summaries.Any(summary => summary.PessoaId == x.Pessoa.Id)))
                .ToArray(),
            CreditValues.BuildAccountStatusOptions()
                .Select(x => new CreditOptionResponse(x.Codigo, x.Nome))
                .ToArray(),
            CreditValues.BuildMovementTypeOptions()
                .Select(x => new CreditOptionResponse(x.Codigo, x.Nome))
                .ToArray());
    }

    /// <summary>
    /// Carrega o saldo e o extrato completo da pessoa informada.
    /// </summary>
    public async Task<CreditAccountDetailResponse> ObterDetalhePorPessoaAsync(
        Guid pessoaId,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCreditViewContextAsync(cancellationToken);
        return await LoadAccountDetailAsync(context.LojaId, pessoaId, cancellationToken)
            ?? throw new InvalidOperationException("Conta de credito nao encontrada para a pessoa na loja ativa.");
    }

    /// <summary>
    /// Cria a conta unica da pessoa na loja ativa quando ela ainda nao existe.
    /// </summary>
    public async Task<CreditAccountDetailResponse> GarantirContaAsync(
        EnsureCreditAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCreditManageContextAsync(cancellationToken);
        var personContext = await LoadStorePersonAsync(context.LojaId, request.PessoaId, cancellationToken);

        var existingAccount = await _dbContext.ContasCreditoLoja
            .FirstOrDefaultAsync(
                x => x.LojaId == context.LojaId && x.PessoaId == request.PessoaId,
                cancellationToken);

        if (existingAccount is not null)
        {
            return await ObterDetalhePorPessoaAsync(request.PessoaId, cancellationToken);
        }

        var account = new ContaCreditoLoja
        {
            Id = Guid.NewGuid(),
            LojaId = context.LojaId,
            PessoaId = request.PessoaId,
            SaldoAtual = 0m,
            SaldoComprometido = 0m,
            StatusConta = CreditValues.AccountStatuses.Ativa,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.ContasCreditoLoja.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "conta_credito_loja",
            account.Id,
            "criada",
            null,
            SnapshotAccount(account, personContext.Pessoa, personContext.Relacao),
            cancellationToken);

        return await ObterDetalhePorPessoaAsync(request.PessoaId, cancellationToken);
    }

    /// <summary>
    /// Lanca credito manual e atualiza o saldo da conta.
    /// </summary>
    public async Task<CreditAccountDetailResponse> RegistrarCreditoManualAsync(
        ManualCreditRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCreditManageContextAsync(cancellationToken);
        var justification = NormalizeRequiredText(
            request.Justificativa,
            "Informe a justificativa do credito manual.");
        var amount = RoundMoney(request.Valor);
        if (amount <= 0m)
        {
            throw new InvalidOperationException("Informe um valor de credito maior que zero.");
        }

        await EnsureStorePersonAsync(context.LojaId, request.PessoaId, cancellationToken);
        var account = await EnsureTrackedAccountAsync(context.LojaId, request.PessoaId, context.UsuarioId, cancellationToken);
        EnsureAccountIsActive(account.StatusConta);

        var before = SnapshotAccount(account);
        var previousBalance = account.SaldoAtual;
        account.SaldoAtual = RoundMoney(account.SaldoAtual + amount);
        TouchEntity(account, context.UsuarioId);

        var movement = new MovimentacaoCreditoLoja
        {
            Id = Guid.NewGuid(),
            ContaCreditoLojaId = account.Id,
            TipoMovimentacao = CreditValues.MovementTypes.CreditoManual,
            OrigemTipo = CreditValues.Origins.AjusteManual,
            OrigemId = null,
            Valor = amount,
            SaldoAnterior = previousBalance,
            SaldoPosterior = account.SaldoAtual,
            Observacoes = justification,
            MovimentadoEm = DateTimeOffset.UtcNow,
            MovimentadoPorUsuarioId = context.UsuarioId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.MovimentacoesCreditoLoja.Add(movement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "conta_credito_loja",
            account.Id,
            "credito_manual",
            before,
            SnapshotAccount(account),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "movimentacao_credito_loja",
            movement.Id,
            "criada",
            null,
            SnapshotMovement(movement),
            cancellationToken);

        return await ObterDetalhePorPessoaAsync(request.PessoaId, cancellationToken);
    }

    /// <summary>
    /// Registra credito de repasse para uso posterior pelo fluxo de pagamento ao fornecedor.
    /// </summary>
    public async Task<CreditAccountDetailResponse> RegistrarCreditoRepasseAsync(
        SupplierPassThroughCreditRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCreditManageContextAsync(cancellationToken);
        var amount = RoundMoney(request.Valor);
        if (amount <= 0m)
        {
            throw new InvalidOperationException("Informe um valor de repasse maior que zero.");
        }

        var reference = NormalizeRequiredText(
            request.Referencia,
            "Informe a referencia do repasse em credito.");
        var notes = NormalizeOptionalText(request.Observacoes);
        var personContext = await LoadStorePersonAsync(context.LojaId, request.PessoaId, cancellationToken);
        if (!personContext.Relacao.EhFornecedor)
        {
            throw new InvalidOperationException("O repasse em credito exige um fornecedor vinculado a loja ativa.");
        }

        var account = await EnsureTrackedAccountAsync(context.LojaId, request.PessoaId, context.UsuarioId, cancellationToken);
        EnsureAccountIsActive(account.StatusConta);

        var before = SnapshotAccount(account);
        var previousBalance = account.SaldoAtual;
        account.SaldoAtual = RoundMoney(account.SaldoAtual + amount);
        TouchEntity(account, context.UsuarioId);

        var description = notes is null ? reference : $"{reference} - {notes}";
        var movement = new MovimentacaoCreditoLoja
        {
            Id = Guid.NewGuid(),
            ContaCreditoLojaId = account.Id,
            TipoMovimentacao = CreditValues.MovementTypes.CreditoRepasse,
            OrigemTipo = CreditValues.Origins.RepasseFornecedor,
            OrigemId = request.ObrigacaoFornecedorId,
            Valor = amount,
            SaldoAnterior = previousBalance,
            SaldoPosterior = account.SaldoAtual,
            Observacoes = description,
            MovimentadoEm = DateTimeOffset.UtcNow,
            MovimentadoPorUsuarioId = context.UsuarioId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        _dbContext.MovimentacoesCreditoLoja.Add(movement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "conta_credito_loja",
            account.Id,
            "credito_repasse",
            before,
            SnapshotAccount(account),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "movimentacao_credito_loja",
            movement.Id,
            "criada",
            null,
            SnapshotMovement(movement),
            cancellationToken);

        return await ObterDetalhePorPessoaAsync(request.PessoaId, cancellationToken);
    }

    /// <summary>
    /// Atualiza o status operacional da conta para bloquear ou reativar uso.
    /// </summary>
    public async Task<CreditAccountDetailResponse> AtualizarStatusContaAsync(
        Guid contaId,
        UpdateCreditAccountStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = await EnsureCreditManageContextAsync(cancellationToken);
        var account = await _dbContext.ContasCreditoLoja
            .FirstOrDefaultAsync(x => x.Id == contaId && x.LojaId == context.LojaId, cancellationToken)
            ?? throw new InvalidOperationException("Conta de credito nao encontrada na loja ativa.");

        var nextStatus = CreditValues.NormalizeAccountStatus(request.StatusConta);
        var before = SnapshotAccount(account);

        account.StatusConta = nextStatus;
        TouchEntity(account, context.UsuarioId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "conta_credito_loja",
            account.Id,
            "status_atualizado",
            before,
            SnapshotAccount(account),
            cancellationToken);

        return await ObterDetalhePorPessoaAsync(account.PessoaId, cancellationToken);
    }

    /// <summary>
    /// Consulta o saldo da pessoa vinculada ao usuario autenticado.
    /// </summary>
    public async Task<CreditAccountDetailResponse> ObterMinhaContaAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureSelfCreditViewContextAsync(cancellationToken);
        var pessoaId = await _dbContext.Usuarios
            .AsNoTracking()
            .Where(x => x.Id == context.UsuarioId)
            .Select(x => x.PessoaId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!pessoaId.HasValue)
        {
            throw new InvalidOperationException("O usuario autenticado nao possui uma pessoa vinculada.");
        }

        return await LoadAccountDetailAsync(context.LojaId, pessoaId.Value, cancellationToken)
            ?? throw new InvalidOperationException("Conta de credito nao encontrada para o usuario autenticado na loja ativa.");
    }

    /// <summary>
    /// Carrega as pessoas da loja com as relacoes operacionais do modulo.
    /// </summary>
    private async Task<IReadOnlyList<StorePersonProjection>> LoadStorePeopleAsync(
        Guid lojaId,
        CancellationToken cancellationToken)
    {
        return await (
                from relacao in _dbContext.PessoaLojas.AsNoTracking()
                join pessoa in _dbContext.Pessoas.AsNoTracking() on relacao.PessoaId equals pessoa.Id
                where relacao.LojaId == lojaId
                orderby pessoa.Nome
                select new StorePersonProjection(pessoa, relacao))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Carrega e valida uma pessoa especifica no contexto da loja ativa.
    /// </summary>
    private async Task<StorePersonProjection> LoadStorePersonAsync(
        Guid lojaId,
        Guid pessoaId,
        CancellationToken cancellationToken)
    {
        var projection = await (
                from relacao in _dbContext.PessoaLojas.AsNoTracking()
                join pessoa in _dbContext.Pessoas.AsNoTracking() on relacao.PessoaId equals pessoa.Id
                where relacao.LojaId == lojaId && relacao.PessoaId == pessoaId
                select new StorePersonProjection(pessoa, relacao))
            .FirstOrDefaultAsync(cancellationToken);

        return projection ?? throw new InvalidOperationException("Pessoa nao encontrada na loja ativa.");
    }

    /// <summary>
    /// Garante apenas a existencia do vinculo da pessoa com a loja.
    /// </summary>
    private async Task EnsureStorePersonAsync(Guid lojaId, Guid pessoaId, CancellationToken cancellationToken)
    {
        _ = await LoadStorePersonAsync(lojaId, pessoaId, cancellationToken);
    }

    /// <summary>
    /// Monta os resumos das contas da loja com ultimo movimento conhecido.
    /// </summary>
    private async Task<IReadOnlyList<CreditAccountSummaryResponse>> BuildAccountSummariesAsync(
        Guid lojaId,
        IReadOnlyList<StorePersonProjection> people,
        CancellationToken cancellationToken)
    {
        var personIds = people.Select(x => x.Pessoa.Id).ToArray();
        if (personIds.Length == 0)
        {
            return [];
        }

        var accounts = await _dbContext.ContasCreditoLoja
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId && personIds.Contains(x.PessoaId))
            .ToListAsync(cancellationToken);

        if (accounts.Count == 0)
        {
            return [];
        }

        var lastMovementMap = await (
                from movement in _dbContext.MovimentacoesCreditoLoja.AsNoTracking()
                join account in _dbContext.ContasCreditoLoja.AsNoTracking() on movement.ContaCreditoLojaId equals account.Id
                where account.LojaId == lojaId
                group movement by account.PessoaId into groupedMovements
                select new
                {
                    PessoaId = groupedMovements.Key,
                    UltimaMovimentacaoEm = groupedMovements.Max(x => (DateTimeOffset?)x.MovimentadoEm),
                })
            .ToDictionaryAsync(x => x.PessoaId, x => x.UltimaMovimentacaoEm, cancellationToken);

        var personMap = people.ToDictionary(x => x.Pessoa.Id, x => x);

        return accounts
            .OrderByDescending(x => x.SaldoAtual)
            .ThenBy(x => personMap.GetValueOrDefault(x.PessoaId)?.Pessoa.Nome)
            .Select(account =>
            {
                var person = personMap[account.PessoaId];
                return MapSummary(account, person.Pessoa, person.Relacao, lastMovementMap.GetValueOrDefault(account.PessoaId));
            })
            .ToArray();
    }

    /// <summary>
    /// Carrega detalhe e extrato de uma conta existente pela pessoa dona da conta.
    /// </summary>
    private async Task<CreditAccountDetailResponse?> LoadAccountDetailAsync(
        Guid lojaId,
        Guid pessoaId,
        CancellationToken cancellationToken)
    {
        var accountProjection = await (
                from account in _dbContext.ContasCreditoLoja.AsNoTracking()
                join relacao in _dbContext.PessoaLojas.AsNoTracking()
                    on new { account.PessoaId, account.LojaId } equals new { relacao.PessoaId, relacao.LojaId }
                join pessoa in _dbContext.Pessoas.AsNoTracking() on account.PessoaId equals pessoa.Id
                where account.LojaId == lojaId && account.PessoaId == pessoaId
                select new StoreAccountProjection(account, pessoa, relacao))
            .FirstOrDefaultAsync(cancellationToken);

        if (accountProjection is null)
        {
            return null;
        }

        var movements = await (
                from movement in _dbContext.MovimentacoesCreditoLoja.AsNoTracking()
                join usuario in _dbContext.Usuarios.AsNoTracking() on movement.MovimentadoPorUsuarioId equals usuario.Id
                where movement.ContaCreditoLojaId == accountProjection.Account.Id
                orderby movement.MovimentadoEm descending
                select new CreditMovementResponse(
                    movement.Id,
                    movement.TipoMovimentacao,
                    movement.OrigemTipo,
                    movement.OrigemId,
                    movement.Valor,
                    movement.SaldoAnterior,
                    movement.SaldoPosterior,
                    CreditValues.ResolveDirection(movement.TipoMovimentacao),
                    movement.Observacoes,
                    movement.MovimentadoEm,
                    movement.MovimentadoPorUsuarioId,
                    usuario.Nome))
            .Take(100)
            .ToListAsync(cancellationToken);

        var lastMovementAt = movements.FirstOrDefault()?.MovimentadoEm;

        return new CreditAccountDetailResponse(
            MapSummary(
                accountProjection.Account,
                accountProjection.Pessoa,
                accountProjection.Relacao,
                lastMovementAt),
            movements);
    }

    /// <summary>
    /// Garante uma conta rastreada para atualizacao no mesmo contexto transacional.
    /// </summary>
    private async Task<ContaCreditoLoja> EnsureTrackedAccountAsync(
        Guid lojaId,
        Guid pessoaId,
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var account = await _dbContext.ContasCreditoLoja
            .FirstOrDefaultAsync(x => x.LojaId == lojaId && x.PessoaId == pessoaId, cancellationToken);

        if (account is not null)
        {
            return account;
        }

        account = new ContaCreditoLoja
        {
            Id = Guid.NewGuid(),
            LojaId = lojaId,
            PessoaId = pessoaId,
            SaldoAtual = 0m,
            SaldoComprometido = 0m,
            StatusConta = CreditValues.AccountStatuses.Ativa,
            CriadoPorUsuarioId = usuarioId,
        };

        _dbContext.ContasCreditoLoja.Add(account);
        return account;
    }

    /// <summary>
    /// Exige autenticao, loja ativa e permissao de visualizacao do modulo.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureCreditViewContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

        await EnsureStoreMembershipAsync(usuarioId, lojaId, cancellationToken);

        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.CreditoVisualizar, AccessPermissionCodes.CreditoGerenciar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui acesso ao modulo de credito na loja ativa.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Exige permissao explicita de gerenciamento do credito.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureCreditManageContextAsync(CancellationToken cancellationToken)
    {
        var context = await EnsureCreditViewContextAsync(cancellationToken);
        var hasPermission = await HasPermissionAsync(
            context.UsuarioId,
            context.LojaId,
            [AccessPermissionCodes.CreditoGerenciar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui permissao para gerenciar credito.");
        }

        return context;
    }

    /// <summary>
    /// Exige permissao de consulta propria para consumos de portal e mobile.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureSelfCreditViewContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

        await EnsureStoreMembershipAsync(usuarioId, lojaId, cancellationToken);

        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [
                AccessPermissionCodes.CreditoVisualizar,
                AccessPermissionCodes.CreditoGerenciar,
                AccessPermissionCodes.PortalConsultar,
                AccessPermissionCodes.MobileConsultar,
            ],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao possui acesso para consultar o seu credito na loja ativa.");
        }

        return (usuarioId, lojaId);
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
    /// Converte a entidade em resumo consumido pela pagina e pelo detalhe.
    /// </summary>
    private static CreditAccountSummaryResponse MapSummary(
        ContaCreditoLoja account,
        Pessoa pessoa,
        PessoaLoja relacao,
        DateTimeOffset? ultimaMovimentacaoEm)
    {
        return new CreditAccountSummaryResponse(
            account.Id,
            pessoa.Id,
            pessoa.Nome,
            pessoa.Documento,
            pessoa.TipoPessoa,
            relacao.EhCliente,
            relacao.EhFornecedor,
            relacao.AceitaCreditoLoja,
            account.StatusConta,
            account.SaldoAtual,
            account.SaldoComprometido,
            RoundMoney(account.SaldoAtual - account.SaldoComprometido),
            ultimaMovimentacaoEm);
    }

    /// <summary>
    /// Arredonda valores monetarios para duas casas.
    /// </summary>
    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Normaliza um texto obrigatorio removendo espacos excedentes.
    /// </summary>
    private static string NormalizeRequiredText(string? value, string errorMessage)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza um texto opcional para armazenamento consistente.
    /// </summary>
    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    /// <summary>
    /// Exige uma conta ativa antes de receber novos creditos manuais.
    /// </summary>
    private static void EnsureAccountIsActive(string accountStatus)
    {
        if (!CreditValues.CanReceiveCredits(accountStatus))
        {
            throw new InvalidOperationException("A conta de credito esta bloqueada para novos lancamentos.");
        }
    }

    /// <summary>
    /// Atualiza os campos de auditoria da entidade.
    /// </summary>
    private static void TouchEntity(AuditEntityBase entity, Guid usuarioId)
    {
        entity.AtualizadoEm = DateTimeOffset.UtcNow;
        entity.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Gera um snapshot sintetico da conta para auditoria.
    /// </summary>
    private static object SnapshotAccount(ContaCreditoLoja account)
    {
        return new
        {
            account.Id,
            account.LojaId,
            account.PessoaId,
            account.StatusConta,
            account.SaldoAtual,
            account.SaldoComprometido,
        };
    }

    /// <summary>
    /// Gera um snapshot com os dados da pessoa ao criar a conta.
    /// </summary>
    private static object SnapshotAccount(ContaCreditoLoja account, Pessoa pessoa, PessoaLoja relacao)
    {
        return new
        {
            account.Id,
            account.LojaId,
            account.PessoaId,
            PessoaNome = pessoa.Nome,
            pessoa.Documento,
            account.StatusConta,
            relacao.EhCliente,
            relacao.EhFornecedor,
            relacao.AceitaCreditoLoja,
            account.SaldoAtual,
            account.SaldoComprometido,
        };
    }

    /// <summary>
    /// Gera um snapshot sintetico da movimentacao para auditoria.
    /// </summary>
    private static object SnapshotMovement(MovimentacaoCreditoLoja movement)
    {
        return new
        {
            movement.Id,
            movement.ContaCreditoLojaId,
            movement.TipoMovimentacao,
            movement.OrigemTipo,
            movement.OrigemId,
            movement.Valor,
            movement.SaldoAnterior,
            movement.SaldoPosterior,
            movement.Observacoes,
            movement.MovimentadoEm,
            movement.MovimentadoPorUsuarioId,
        };
    }

    // Projeta pessoa e relacao da loja para evitar dicionarios soltos no service.
    private sealed record StorePersonProjection(Pessoa Pessoa, PessoaLoja Relacao);

    // Projeta conta, pessoa e relacao para montar o detalhe da conta.
    private sealed record StoreAccountProjection(ContaCreditoLoja Account, Pessoa Pessoa, PessoaLoja Relacao);
}
