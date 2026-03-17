using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.People.Abstractions;
using Renova.Services.Features.People.Contracts;

namespace Renova.Services.Features.People.Services;

// Implementa o modulo 03 com cadastro mestre, relacao por loja e visao financeira da pessoa.
public sealed partial class PersonService : IPersonService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o service com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public PersonService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Lista as pessoas da loja ativa com resumo da relacao e do financeiro.
    /// </summary>
    public async Task<IReadOnlyList<PersonSummaryResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanViewPeopleAsync(context.UsuarioId, context.LojaId, cancellationToken);

        var items = await (
                from pessoaLoja in _dbContext.PessoaLojas
                join pessoa in _dbContext.Pessoas on pessoaLoja.PessoaId equals pessoa.Id
                join usuario in _dbContext.Usuarios on pessoa.Id equals usuario.PessoaId into usuarioGroup
                from usuario in usuarioGroup.DefaultIfEmpty()
                where pessoaLoja.LojaId == context.LojaId
                orderby pessoa.Nome
                select new
                {
                    Pessoa = pessoa,
                    PessoaLoja = pessoaLoja,
                    Usuario = usuario,
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return Array.Empty<PersonSummaryResponse>();
        }

        var pessoaIds = items.Select(x => x.Pessoa.Id).Distinct().ToArray();
        var financialSummaries = await BuildFinancialSummariesAsync(context.LojaId, pessoaIds, cancellationToken);

        return items
            .Select(item => new PersonSummaryResponse(
                item.Pessoa.Id,
                item.Pessoa.TipoPessoa,
                item.Pessoa.Nome,
                item.Pessoa.NomeSocial,
                item.Pessoa.Documento,
                item.Pessoa.Telefone,
                item.Pessoa.Email,
                item.Pessoa.Ativo,
                MapRelation(item.PessoaLoja),
                MapLinkedUser(item.Usuario),
                financialSummaries.GetValueOrDefault(item.Pessoa.Id) ?? EmptyFinancialSummary()))
            .ToArray();
    }

    /// <summary>
    /// Carrega o detalhe completo da pessoa no contexto da loja ativa.
    /// </summary>
    public async Task<PersonDetailResponse> ObterDetalheAsync(Guid pessoaId, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanViewPeopleAsync(context.UsuarioId, context.LojaId, cancellationToken);

        var detail = await LoadDetailAsync(context.LojaId, pessoaId, cancellationToken);
        return detail ?? throw new InvalidOperationException("Pessoa nao encontrada na loja ativa.");
    }

    /// <summary>
    /// Lista usuarios disponiveis para associacao com o cadastro da pessoa.
    /// </summary>
    public async Task<IReadOnlyList<PersonUserOptionResponse>> ListarUsuariosVinculaveisAsync(CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanManagePeopleAsync(context.UsuarioId, context.LojaId, cancellationToken);

        return await _dbContext.Usuarios
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new PersonUserOptionResponse(
                x.Id,
                x.Nome,
                x.Email,
                x.StatusUsuario,
                x.PessoaId))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Cria a pessoa ou reaproveita o cadastro mestre existente pelo documento.
    /// </summary>
    public async Task<PersonDetailResponse> CriarAsync(CreatePersonRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanManagePeopleAsync(context.UsuarioId, context.LojaId, cancellationToken);
        ValidateRequest(request.TipoPessoa, request.Nome, request.Documento, request.Telefone, request.Email, request.RelacaoLoja, request.ContasBancarias);

        var normalizedTipoPessoa = PeopleDocumentValidator.NormalizeTipoPessoa(request.TipoPessoa);
        var normalizedDocumento = PeopleDocumentValidator.NormalizeAndValidate(normalizedTipoPessoa, request.Documento);

        var pessoa = await _dbContext.Pessoas.FirstOrDefaultAsync(
            x => x.Documento == normalizedDocumento,
            cancellationToken);

        var personAction = "criada";
        object? personBefore = null;

        if (pessoa is null)
        {
            pessoa = new Pessoa
            {
                Id = Guid.NewGuid(),
                CriadoPorUsuarioId = context.UsuarioId,
            };

            ApplyPersonData(pessoa, request, normalizedTipoPessoa, normalizedDocumento, context.UsuarioId);
            _dbContext.Pessoas.Add(pessoa);
        }
        else
        {
            var relationExists = await _dbContext.PessoaLojas.AnyAsync(
                x => x.PessoaId == pessoa.Id && x.LojaId == context.LojaId,
                cancellationToken);

            if (relationExists)
            {
                throw new InvalidOperationException("Ja existe cadastro desta pessoa para a loja ativa.");
            }

            personAction = "vinculada";
            personBefore = SnapshotPerson(pessoa);
            ApplyPersonData(pessoa, request, normalizedTipoPessoa, normalizedDocumento, context.UsuarioId);
        }

        var relation = new PessoaLoja
        {
            Id = Guid.NewGuid(),
            PessoaId = pessoa.Id,
            LojaId = context.LojaId,
            CriadoPorUsuarioId = context.UsuarioId,
        };

        ApplyRelationData(relation, request.RelacaoLoja, context.UsuarioId);
        _dbContext.PessoaLojas.Add(relation);

        await SyncBankAccountsAsync(pessoa.Id, request.ContasBancarias, context.UsuarioId, cancellationToken);
        await SyncUserLinkAsync(pessoa.Id, request.UsuarioId, context.UsuarioId, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "pessoa",
            pessoa.Id,
            personAction,
            personBefore,
            SnapshotPerson(pessoa),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "pessoa_loja",
            relation.Id,
            "criada",
            null,
            SnapshotRelation(relation),
            cancellationToken);

        return await ObterDetalheAsync(pessoa.Id, cancellationToken);
    }

    /// <summary>
    /// Atualiza o cadastro mestre, o vinculo da loja e os dados bancarios da pessoa.
    /// </summary>
    public async Task<PersonDetailResponse> AtualizarAsync(Guid pessoaId, UpdatePersonRequest request, CancellationToken cancellationToken = default)
    {
        var context = await EnsureStoreContextAsync(cancellationToken);
        await EnsureCanManagePeopleAsync(context.UsuarioId, context.LojaId, cancellationToken);
        ValidateRequest(request.TipoPessoa, request.Nome, request.Documento, request.Telefone, request.Email, request.RelacaoLoja, request.ContasBancarias);

        var normalizedTipoPessoa = PeopleDocumentValidator.NormalizeTipoPessoa(request.TipoPessoa);
        var normalizedDocumento = PeopleDocumentValidator.NormalizeAndValidate(normalizedTipoPessoa, request.Documento);

        var pessoa = await _dbContext.Pessoas.FirstOrDefaultAsync(x => x.Id == pessoaId, cancellationToken)
            ?? throw new InvalidOperationException("Pessoa nao encontrada.");

        var relation = await _dbContext.PessoaLojas.FirstOrDefaultAsync(
                x => x.PessoaId == pessoaId && x.LojaId == context.LojaId,
                cancellationToken)
            ?? throw new InvalidOperationException("Pessoa nao encontrada na loja ativa.");

        var duplicatedDocument = await _dbContext.Pessoas.AnyAsync(
            x => x.Id != pessoaId && x.Documento == normalizedDocumento,
            cancellationToken);

        if (duplicatedDocument)
        {
            throw new InvalidOperationException("Ja existe outra pessoa com o documento informado.");
        }

        var personBefore = SnapshotPerson(pessoa);
        var relationBefore = SnapshotRelation(relation);

        ApplyPersonData(pessoa, request, normalizedTipoPessoa, normalizedDocumento, context.UsuarioId);
        ApplyRelationData(relation, request.RelacaoLoja, context.UsuarioId);

        await SyncBankAccountsAsync(pessoa.Id, request.ContasBancarias, context.UsuarioId, cancellationToken);
        await SyncUserLinkAsync(pessoa.Id, request.UsuarioId, context.UsuarioId, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "pessoa",
            pessoa.Id,
            "atualizada",
            personBefore,
            SnapshotPerson(pessoa),
            cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            context.LojaId,
            "pessoa_loja",
            relation.Id,
            "atualizada",
            relationBefore,
            SnapshotRelation(relation),
            cancellationToken);

        return await ObterDetalheAsync(pessoa.Id, cancellationToken);
    }

    /// <summary>
    /// Carrega o detalhe completo da pessoa com relacao, contas e resumo financeiro.
    /// </summary>
    private async Task<PersonDetailResponse?> LoadDetailAsync(
        Guid lojaId,
        Guid pessoaId,
        CancellationToken cancellationToken)
    {
        var item = await (
                from pessoaLoja in _dbContext.PessoaLojas
                join pessoa in _dbContext.Pessoas on pessoaLoja.PessoaId equals pessoa.Id
                join usuario in _dbContext.Usuarios on pessoa.Id equals usuario.PessoaId into usuarioGroup
                from usuario in usuarioGroup.DefaultIfEmpty()
                where pessoaLoja.LojaId == lojaId && pessoa.Id == pessoaId
                select new
                {
                    Pessoa = pessoa,
                    PessoaLoja = pessoaLoja,
                    Usuario = usuario,
                })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var accounts = await _dbContext.PessoaContasBancarias
            .AsNoTracking()
            .Where(x => x.PessoaId == pessoaId)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Banco)
            .ToListAsync(cancellationToken);

        var financialSummary = await BuildFinancialSummaryAsync(lojaId, pessoaId, cancellationToken);
        var history = await BuildFinancialHistoryAsync(lojaId, pessoaId, cancellationToken);

        return new PersonDetailResponse(
            item.Pessoa.Id,
            item.Pessoa.TipoPessoa,
            item.Pessoa.Nome,
            item.Pessoa.NomeSocial,
            item.Pessoa.Documento,
            item.Pessoa.Telefone,
            item.Pessoa.Email,
            item.Pessoa.Logradouro,
            item.Pessoa.Numero,
            item.Pessoa.Complemento,
            item.Pessoa.Bairro,
            item.Pessoa.Cidade,
            item.Pessoa.Uf,
            item.Pessoa.Cep,
            item.Pessoa.Observacoes,
            item.Pessoa.Ativo,
            MapRelation(item.PessoaLoja),
            MapLinkedUser(item.Usuario),
            accounts.Select(MapBankAccount).ToArray(),
            financialSummary,
            history);
    }

    /// <summary>
    /// Reune os totais financeiros por pessoa para a listagem da loja.
    /// </summary>
    private async Task<Dictionary<Guid, PersonFinancialSummaryResponse>> BuildFinancialSummariesAsync(
        Guid lojaId,
        IReadOnlyCollection<Guid> pessoaIds,
        CancellationToken cancellationToken)
    {
        var result = pessoaIds.ToDictionary(x => x, _ => EmptyFinancialSummary());
        if (pessoaIds.Count == 0)
        {
            return result;
        }

        var accounts = await _dbContext.ContasCreditoLoja
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId && pessoaIds.Contains(x.PessoaId))
            .ToListAsync(cancellationToken);

        var accountByPerson = accounts.ToDictionary(
            x => x.PessoaId,
            x => new PersonFinancialSummaryResponse(
                x.SaldoAtual,
                x.SaldoComprometido,
                0m,
                0,
                null));

        var pendingByPerson = await _dbContext.ObrigacoesFornecedor
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId && pessoaIds.Contains(x.PessoaId) && x.ValorEmAberto > 0)
            .GroupBy(x => x.PessoaId)
            .Select(group => new
            {
                PessoaId = group.Key,
                Quantidade = group.Count(),
                Total = group.Sum(x => x.ValorEmAberto),
                UltimaData = group.Max(x => (DateTimeOffset?)x.DataGeracao),
            })
            .ToListAsync(cancellationToken);

        var creditDates = await (
                from movimentacao in _dbContext.MovimentacoesCreditoLoja.AsNoTracking()
                join conta in _dbContext.ContasCreditoLoja.AsNoTracking() on movimentacao.ContaCreditoLojaId equals conta.Id
                where conta.LojaId == lojaId && pessoaIds.Contains(conta.PessoaId)
                group movimentacao by conta.PessoaId into groupedMovements
                select new
                {
                    PessoaId = groupedMovements.Key,
                    UltimaData = groupedMovements.Max(x => (DateTimeOffset?)x.MovimentadoEm),
                })
            .ToListAsync(cancellationToken);

        var paymentDates = await (
                from liquidacao in _dbContext.LiquidacoesObrigacaoFornecedor.AsNoTracking()
                join obrigacao in _dbContext.ObrigacoesFornecedor.AsNoTracking() on liquidacao.ObrigacaoFornecedorId equals obrigacao.Id
                where obrigacao.LojaId == lojaId && pessoaIds.Contains(obrigacao.PessoaId)
                group liquidacao by obrigacao.PessoaId into groupedPayments
                select new
                {
                    PessoaId = groupedPayments.Key,
                    UltimaData = groupedPayments.Max(x => (DateTimeOffset?)x.LiquidadoEm),
                })
            .ToListAsync(cancellationToken);

        foreach (var pessoaId in pessoaIds)
        {
            var account = accountByPerson.GetValueOrDefault(pessoaId) ?? EmptyFinancialSummary();
            var pending = pendingByPerson.FirstOrDefault(x => x.PessoaId == pessoaId);
            var lastDates = new[]
            {
                pending?.UltimaData,
                creditDates.FirstOrDefault(x => x.PessoaId == pessoaId)?.UltimaData,
                paymentDates.FirstOrDefault(x => x.PessoaId == pessoaId)?.UltimaData,
            };

            result[pessoaId] = account with
            {
                TotalPendencias = pending?.Total ?? 0m,
                QuantidadePendencias = pending?.Quantidade ?? 0,
                UltimaMovimentacaoEm = lastDates.Where(x => x.HasValue).Max(),
            };
        }

        return result;
    }

    /// <summary>
    /// Monta o resumo financeiro exibido no detalhe da pessoa.
    /// </summary>
    private async Task<PersonFinancialSummaryResponse> BuildFinancialSummaryAsync(
        Guid lojaId,
        Guid pessoaId,
        CancellationToken cancellationToken)
    {
        var summaries = await BuildFinancialSummariesAsync(lojaId, [pessoaId], cancellationToken);
        return summaries.GetValueOrDefault(pessoaId) ?? EmptyFinancialSummary();
    }

    /// <summary>
    /// Combina credito e pagamentos em um historico financeiro resumido.
    /// </summary>
    private async Task<IReadOnlyList<PersonFinancialEntryResponse>> BuildFinancialHistoryAsync(
        Guid lojaId,
        Guid pessoaId,
        CancellationToken cancellationToken)
    {
        var creditEntries = await (
                from movimentacao in _dbContext.MovimentacoesCreditoLoja.AsNoTracking()
                join conta in _dbContext.ContasCreditoLoja.AsNoTracking() on movimentacao.ContaCreditoLojaId equals conta.Id
                where conta.LojaId == lojaId && conta.PessoaId == pessoaId
                orderby movimentacao.MovimentadoEm descending
                select new PersonFinancialEntryResponse(
                    movimentacao.Id,
                    "credito_loja",
                    BuildCreditDescription(movimentacao),
                    movimentacao.Valor,
                    ResolveCreditDirection(movimentacao.TipoMovimentacao),
                    movimentacao.OrigemTipo,
                    movimentacao.MovimentadoEm))
            .Take(20)
            .ToListAsync(cancellationToken);

        var obligationEntries = await _dbContext.ObrigacoesFornecedor
            .AsNoTracking()
            .Where(x => x.LojaId == lojaId && x.PessoaId == pessoaId)
            .OrderByDescending(x => x.DataGeracao)
            .Take(20)
            .Select(x => new PersonFinancialEntryResponse(
                x.Id,
                "obrigacao",
                $"Obrigacao {x.TipoObrigacao}",
                x.ValorOriginal,
                "entrada",
                x.StatusObrigacao,
                x.DataGeracao))
            .ToListAsync(cancellationToken);

        var paymentEntries = await (
                from liquidacao in _dbContext.LiquidacoesObrigacaoFornecedor.AsNoTracking()
                join obrigacao in _dbContext.ObrigacoesFornecedor.AsNoTracking() on liquidacao.ObrigacaoFornecedorId equals obrigacao.Id
                where obrigacao.LojaId == lojaId && obrigacao.PessoaId == pessoaId
                orderby liquidacao.LiquidadoEm descending
                select new PersonFinancialEntryResponse(
                    liquidacao.Id,
                    "liquidacao_fornecedor",
                    $"Pagamento {liquidacao.TipoLiquidacao}",
                    liquidacao.Valor,
                    "saida",
                    obrigacao.TipoObrigacao,
                    liquidacao.LiquidadoEm))
            .Take(20)
            .ToListAsync(cancellationToken);

        return creditEntries
            .Concat(obligationEntries)
            .Concat(paymentEntries)
            .OrderByDescending(x => x.OcorridoEm)
            .Take(20)
            .ToArray();
    }

    /// <summary>
    /// Sincroniza o conjunto de contas bancarias da pessoa conforme o payload recebido.
    /// </summary>
    private async Task SyncBankAccountsAsync(
        Guid pessoaId,
        IReadOnlyList<PersonBankAccountRequest> accounts,
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var normalizedAccounts = NormalizeBankAccounts(accounts);
        var existingAccounts = await _dbContext.PessoaContasBancarias
            .Where(x => x.PessoaId == pessoaId)
            .ToListAsync(cancellationToken);

        var accountIds = normalizedAccounts
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        foreach (var existing in existingAccounts.Where(x => !accountIds.Contains(x.Id)))
        {
            _dbContext.PessoaContasBancarias.Remove(existing);
        }

        foreach (var account in normalizedAccounts)
        {
            if (account.Id.HasValue)
            {
                var existing = existingAccounts.FirstOrDefault(x => x.Id == account.Id.Value)
                    ?? throw new InvalidOperationException("Conta bancaria nao encontrada para a pessoa informada.");

                ApplyBankAccountData(existing, account, usuarioId);
                continue;
            }

            var entity = new PessoaContaBancaria
            {
                Id = Guid.NewGuid(),
                PessoaId = pessoaId,
                CriadoPorUsuarioId = usuarioId,
            };

            ApplyBankAccountData(entity, account, usuarioId);
            _dbContext.PessoaContasBancarias.Add(entity);
        }
    }

    /// <summary>
    /// Sincroniza o usuario vinculado a pessoa, respeitando a unicidade do relacionamento.
    /// </summary>
    private async Task SyncUserLinkAsync(
        Guid pessoaId,
        Guid? usuarioId,
        Guid actingUserId,
        CancellationToken cancellationToken)
    {
        var usersLinkedToPerson = await _dbContext.Usuarios
            .Where(x => x.PessoaId == pessoaId)
            .ToListAsync(cancellationToken);

        foreach (var linkedUser in usersLinkedToPerson.Where(x => x.Id != usuarioId))
        {
            linkedUser.PessoaId = null;
            linkedUser.AtualizadoEm = DateTimeOffset.UtcNow;
            linkedUser.AtualizadoPorUsuarioId = actingUserId;
        }

        if (usuarioId is null)
        {
            return;
        }

        var user = await _dbContext.Usuarios.FirstOrDefaultAsync(x => x.Id == usuarioId.Value, cancellationToken)
            ?? throw new InvalidOperationException("Usuario informado para vinculacao nao encontrado.");

        if (user.PessoaId.HasValue && user.PessoaId != pessoaId)
        {
            throw new InvalidOperationException("O usuario informado ja esta vinculado a outra pessoa.");
        }

        user.PessoaId = pessoaId;
        user.AtualizadoEm = DateTimeOffset.UtcNow;
        user.AtualizadoPorUsuarioId = actingUserId;
    }

    /// <summary>
    /// Garante usuario autenticado, loja ativa e vinculo valido no contexto atual.
    /// </summary>
    private async Task<(Guid UsuarioId, Guid LojaId)> EnsureStoreContextAsync(CancellationToken cancellationToken)
    {
        var usuarioId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        var lojaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para continuar.");

        var hasActiveMembership = await _dbContext.UsuarioLojas.AnyAsync(
            x => x.UsuarioId == usuarioId &&
                 x.LojaId == lojaId &&
                 x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                 (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
            cancellationToken);

        if (!hasActiveMembership)
        {
            throw new InvalidOperationException("Voce nao possui acesso a loja ativa informada.");
        }

        return (usuarioId, lojaId);
    }

    /// <summary>
    /// Exige permissao de visualizacao ou gerenciamento do modulo de pessoas.
    /// </summary>
    private async Task EnsureCanViewPeopleAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.PessoasVisualizar, AccessPermissionCodes.PessoasGerenciar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao tem acesso para visualizar pessoas na loja ativa.");
        }
    }

    /// <summary>
    /// Exige permissao de gerenciamento do modulo de pessoas.
    /// </summary>
    private async Task EnsureCanManagePeopleAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
        var hasPermission = await HasPermissionAsync(
            usuarioId,
            lojaId,
            [AccessPermissionCodes.PessoasGerenciar],
            cancellationToken);

        if (!hasPermission)
        {
            throw new InvalidOperationException("Voce nao tem acesso para gerenciar pessoas na loja ativa.");
        }
    }

    /// <summary>
    /// Verifica se o usuario possui ao menos uma das permissoes informadas na loja ativa.
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
    /// Aplica os campos do cadastro mestre da pessoa.
    /// </summary>
    private static void ApplyPersonData(
        Pessoa pessoa,
        CreatePersonRequest request,
        string normalizedTipoPessoa,
        string normalizedDocumento,
        Guid usuarioId)
    {
        pessoa.TipoPessoa = normalizedTipoPessoa;
        pessoa.Nome = request.Nome.Trim();
        pessoa.NomeSocial = request.NomeSocial.Trim();
        pessoa.Documento = normalizedDocumento;
        pessoa.Telefone = request.Telefone.Trim();
        pessoa.Email = NormalizeEmail(request.Email);
        pessoa.Logradouro = request.Logradouro.Trim();
        pessoa.Numero = request.Numero.Trim();
        pessoa.Complemento = request.Complemento.Trim();
        pessoa.Bairro = request.Bairro.Trim();
        pessoa.Cidade = request.Cidade.Trim();
        pessoa.Uf = request.Uf.Trim().ToUpperInvariant();
        pessoa.Cep = request.Cep.Trim();
        pessoa.Observacoes = request.Observacoes.Trim();
        pessoa.Ativo = request.Ativo;
        pessoa.InativadoEm = request.Ativo ? null : DateTimeOffset.UtcNow;
        pessoa.AtualizadoEm = DateTimeOffset.UtcNow;
        pessoa.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Aplica os campos do cadastro mestre da pessoa a partir do payload de update.
    /// </summary>
    private static void ApplyPersonData(
        Pessoa pessoa,
        UpdatePersonRequest request,
        string normalizedTipoPessoa,
        string normalizedDocumento,
        Guid usuarioId)
    {
        pessoa.TipoPessoa = normalizedTipoPessoa;
        pessoa.Nome = request.Nome.Trim();
        pessoa.NomeSocial = request.NomeSocial.Trim();
        pessoa.Documento = normalizedDocumento;
        pessoa.Telefone = request.Telefone.Trim();
        pessoa.Email = NormalizeEmail(request.Email);
        pessoa.Logradouro = request.Logradouro.Trim();
        pessoa.Numero = request.Numero.Trim();
        pessoa.Complemento = request.Complemento.Trim();
        pessoa.Bairro = request.Bairro.Trim();
        pessoa.Cidade = request.Cidade.Trim();
        pessoa.Uf = request.Uf.Trim().ToUpperInvariant();
        pessoa.Cep = request.Cep.Trim();
        pessoa.Observacoes = request.Observacoes.Trim();
        pessoa.Ativo = request.Ativo;
        pessoa.InativadoEm = request.Ativo ? null : DateTimeOffset.UtcNow;
        pessoa.AtualizadoEm = DateTimeOffset.UtcNow;
        pessoa.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Aplica os campos da relacao da pessoa com a loja ativa.
    /// </summary>
    private static void ApplyRelationData(PessoaLoja relation, PersonStoreRelationRequest request, Guid usuarioId)
    {
        relation.EhCliente = request.EhCliente;
        relation.EhFornecedor = request.EhFornecedor;
        relation.AceitaCreditoLoja = request.AceitaCreditoLoja;
        relation.PoliticaPadraoFimConsignacao = NormalizeConsignmentPolicy(request.PoliticaPadraoFimConsignacao);
        relation.ObservacoesInternas = request.ObservacoesInternas.Trim();
        relation.StatusRelacao = NormalizeRelationStatus(request.StatusRelacao);
        relation.AtualizadoEm = DateTimeOffset.UtcNow;
        relation.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Aplica os campos de uma conta bancaria da pessoa.
    /// </summary>
    private static void ApplyBankAccountData(PessoaContaBancaria account, PersonBankAccountRequest request, Guid usuarioId)
    {
        account.Banco = request.Banco.Trim();
        account.Agencia = request.Agencia.Trim();
        account.Conta = request.Conta.Trim();
        account.TipoConta = request.TipoConta.Trim();
        account.PixTipo = request.PixTipo.Trim();
        account.PixChave = request.PixChave.Trim();
        account.FavorecidoNome = request.FavorecidoNome.Trim();
        account.FavorecidoDocumento = PeopleDocumentValidator.NormalizeDocument(request.FavorecidoDocumento);
        account.Principal = request.Principal;
        account.AtualizadoEm = DateTimeOffset.UtcNow;
        account.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Valida os campos essenciais do payload da pessoa.
    /// </summary>
    private static void ValidateRequest(
        string tipoPessoa,
        string nome,
        string documento,
        string telefone,
        string email,
        PersonStoreRelationRequest relation,
        IReadOnlyList<PersonBankAccountRequest> accounts)
    {
        _ = PeopleDocumentValidator.NormalizeTipoPessoa(tipoPessoa);
        _ = PeopleDocumentValidator.NormalizeDocument(documento);

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Informe o nome da pessoa.");
        }

        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new InvalidOperationException("Informe o telefone da pessoa.");
        }

        _ = NormalizeEmail(email);

        if (!relation.EhCliente && !relation.EhFornecedor)
        {
            throw new InvalidOperationException("A pessoa deve ser marcada como cliente, fornecedor ou ambos.");
        }

        _ = NormalizeRelationStatus(relation.StatusRelacao);
        _ = NormalizeConsignmentPolicy(relation.PoliticaPadraoFimConsignacao);

        foreach (var account in accounts)
        {
            ValidateBankAccount(account);
        }
    }

    /// <summary>
    /// Valida os dados minimos de uma conta bancaria antes da persistencia.
    /// </summary>
    private static void ValidateBankAccount(PersonBankAccountRequest account)
    {
        if (string.IsNullOrWhiteSpace(account.Banco))
        {
            throw new InvalidOperationException("Informe o banco da conta bancaria.");
        }

        if (string.IsNullOrWhiteSpace(account.TipoConta))
        {
            throw new InvalidOperationException("Informe o tipo da conta bancaria.");
        }

        if (string.IsNullOrWhiteSpace(account.FavorecidoNome))
        {
            throw new InvalidOperationException("Informe o nome do favorecido.");
        }

        if (string.IsNullOrWhiteSpace(account.FavorecidoDocumento))
        {
            throw new InvalidOperationException("Informe o documento do favorecido.");
        }

        var hasPixType = !string.IsNullOrWhiteSpace(account.PixTipo);
        var hasPixKey = !string.IsNullOrWhiteSpace(account.PixChave);
        if (hasPixType != hasPixKey)
        {
            throw new InvalidOperationException("Informe o tipo e a chave PIX juntos.");
        }
    }

    /// <summary>
    /// Garante a existencia de apenas uma conta principal no payload.
    /// </summary>
    private static IReadOnlyList<PersonBankAccountRequest> NormalizeBankAccounts(IReadOnlyList<PersonBankAccountRequest> accounts)
    {
        if (accounts.Count == 0)
        {
            return Array.Empty<PersonBankAccountRequest>();
        }

        var normalized = accounts
            .Select(account => account with
            {
                Banco = account.Banco.Trim(),
                Agencia = account.Agencia.Trim(),
                Conta = account.Conta.Trim(),
                TipoConta = account.TipoConta.Trim(),
                PixTipo = account.PixTipo.Trim(),
                PixChave = account.PixChave.Trim(),
                FavorecidoNome = account.FavorecidoNome.Trim(),
                FavorecidoDocumento = account.FavorecidoDocumento.Trim(),
            })
            .Where(account =>
                !string.IsNullOrWhiteSpace(account.Banco) ||
                !string.IsNullOrWhiteSpace(account.PixChave) ||
                !string.IsNullOrWhiteSpace(account.Conta))
            .ToList();

        if (normalized.Count == 0)
        {
            return Array.Empty<PersonBankAccountRequest>();
        }

        if (normalized.Count(account => account.Principal) == 0)
        {
            normalized[0] = normalized[0] with { Principal = true };
        }

        var principalAssigned = false;
        for (var index = 0; index < normalized.Count; index++)
        {
            if (!normalized[index].Principal)
            {
                continue;
            }

            if (!principalAssigned)
            {
                principalAssigned = true;
                continue;
            }

            normalized[index] = normalized[index] with { Principal = false };
        }

        return normalized;
    }

    /// <summary>
    /// Mapeia a relacao pessoa x loja para o contrato do frontend.
    /// </summary>
    private static PersonStoreRelationResponse MapRelation(PessoaLoja relation)
    {
        return new PersonStoreRelationResponse(
            relation.Id,
            relation.LojaId,
            relation.EhCliente,
            relation.EhFornecedor,
            relation.AceitaCreditoLoja,
            relation.PoliticaPadraoFimConsignacao,
            relation.ObservacoesInternas,
            relation.StatusRelacao);
    }

    /// <summary>
    /// Mapeia uma conta bancaria persistida para o contrato da API.
    /// </summary>
    private static PersonBankAccountResponse MapBankAccount(PessoaContaBancaria account)
    {
        return new PersonBankAccountResponse(
            account.Id,
            account.Banco,
            account.Agencia,
            account.Conta,
            account.TipoConta,
            account.PixTipo,
            account.PixChave,
            account.FavorecidoNome,
            account.FavorecidoDocumento,
            account.Principal);
    }

    /// <summary>
    /// Mapeia o usuario associado a pessoa quando existir.
    /// </summary>
    private static PersonLinkedUserResponse? MapLinkedUser(Usuario? user)
    {
        return user is null
            ? null
            : new PersonLinkedUserResponse(
                user.Id,
                user.Nome,
                user.Email,
                user.StatusUsuario);
    }

    /// <summary>
    /// Define o resumo financeiro vazio para pessoas sem transacoes.
    /// </summary>
    private static PersonFinancialSummaryResponse EmptyFinancialSummary()
    {
        return new PersonFinancialSummaryResponse(0m, 0m, 0m, 0, null);
    }

    /// <summary>
    /// Gera uma descricao amigavel para movimentos de credito da loja.
    /// </summary>
    private static string BuildCreditDescription(MovimentacaoCreditoLoja movement)
    {
        return string.IsNullOrWhiteSpace(movement.Observacoes)
            ? $"Movimento de credito {movement.TipoMovimentacao}"
            : movement.Observacoes;
    }

    /// <summary>
    /// Traduz o tipo do movimento de credito para entrada ou saida.
    /// </summary>
    private static string ResolveCreditDirection(string movementType)
    {
        return movementType.Contains("debito", StringComparison.OrdinalIgnoreCase) ? "saida" : "entrada";
    }

    /// <summary>
    /// Normaliza o status da relacao pessoa x loja.
    /// </summary>
    private static string NormalizeRelationStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!PeopleStatusValues.StatusRelacao.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Status da relacao com a loja invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza a politica padrao de fim de consignacao do fornecedor.
    /// </summary>
    private static string NormalizeConsignmentPolicy(string policy)
    {
        var normalized = policy.Trim().ToLowerInvariant();
        if (!PeopleStatusValues.PoliticaFimConsignacao.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Politica padrao de fim de consignacao invalida.");
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza o email informado no cadastro da pessoa.
    /// </summary>
    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Informe o email da pessoa.");
        }

        return email.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Captura o estado resumido da pessoa para auditoria.
    /// </summary>
    private static object SnapshotPerson(Pessoa person)
    {
        return new
        {
            person.TipoPessoa,
            person.Nome,
            person.NomeSocial,
            person.Documento,
            person.Telefone,
            person.Email,
            person.Ativo,
        };
    }

    /// <summary>
    /// Captura o estado resumido da relacao com a loja para auditoria.
    /// </summary>
    private static object SnapshotRelation(PessoaLoja relation)
    {
        return new
        {
            relation.LojaId,
            relation.EhCliente,
            relation.EhFornecedor,
            relation.AceitaCreditoLoja,
            relation.PoliticaPadraoFimConsignacao,
            relation.StatusRelacao,
        };
    }
}
