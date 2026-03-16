using Microsoft.EntityFrameworkCore;
using Renova.Persistence;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Contracts;

namespace Renova.Services.Features.Access.Services;

// Representa o servico de manutencao de cargos e permissoes por loja.
public sealed class AccessRoleService : IAccessRoleService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o servico com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public AccessRoleService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Lista os cargos cadastrados na loja ativa.
    /// </summary>
    public async Task<IReadOnlyList<RoleResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();
        return await CarregarCargosAsync(lojaAtivaId, cancellationToken);
    }

    /// <summary>
    /// Lista o catalogo ativo de permissoes do sistema.
    /// </summary>
    public async Task<IReadOnlyList<PermissionResponse>> ListarPermissoesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissoes
            .AsNoTracking()
            .Where(x => x.Ativo)
            .OrderBy(x => x.Modulo)
            .ThenBy(x => x.Nome)
            .Select(x => new PermissionResponse(x.Id, x.Codigo, x.Nome, x.Descricao, x.Modulo, x.Ativo))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Cria um novo cargo e vincula as permissoes selecionadas.
    /// </summary>
    public async Task<RoleResponse> CriarAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();
        var permissionIds = await ValidarPermissoesAsync(request.PermissaoIds, cancellationToken);

        var cargo = new Domain.Models.Cargo
        {
            Id = Guid.NewGuid(),
            LojaId = lojaAtivaId,
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao.Trim(),
            Ativo = true,
            CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
        };

        _dbContext.Cargos.Add(cargo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var permissionId in permissionIds)
        {
            _dbContext.CargoPermissoes.Add(new Domain.Models.CargoPermissao
            {
                Id = Guid.NewGuid(),
                CargoId = cargo.Id,
                PermissaoId = permissionId,
                CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            lojaAtivaId,
            "cargo",
            cargo.Id,
            "criado",
            null,
            new { cargo.Nome, cargo.Descricao, permissionIds },
            cancellationToken);

        return (await CarregarCargosAsync(lojaAtivaId, cancellationToken)).First(x => x.Id == cargo.Id);
    }

    /// <summary>
    /// Atualiza os dados principais de um cargo da loja ativa.
    /// </summary>
    public async Task<RoleResponse> AtualizarAsync(Guid cargoId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();
        var cargo = await _dbContext.Cargos.FirstOrDefaultAsync(
            x => x.Id == cargoId && x.LojaId == lojaAtivaId,
            cancellationToken)
            ?? throw new InvalidOperationException("Cargo não encontrado na loja ativa.");

        var antes = new { cargo.Nome, cargo.Descricao, cargo.Ativo };

        cargo.Nome = request.Nome.Trim();
        cargo.Descricao = request.Descricao.Trim();
        cargo.Ativo = request.Ativo;
        cargo.InativadoEm = request.Ativo ? null : DateTimeOffset.UtcNow;
        cargo.AtualizadoEm = DateTimeOffset.UtcNow;
        cargo.AtualizadoPorUsuarioId = _currentRequestContext.UsuarioId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            lojaAtivaId,
            "cargo",
            cargo.Id,
            "atualizado",
            antes,
            new { cargo.Nome, cargo.Descricao, cargo.Ativo },
            cancellationToken);

        return (await CarregarCargosAsync(lojaAtivaId, cancellationToken)).First(x => x.Id == cargo.Id);
    }

    /// <summary>
    /// Substitui a matriz de permissoes de um cargo.
    /// </summary>
    public async Task<RoleResponse> AtualizarPermissoesAsync(
        Guid cargoId,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();
        var cargo = await _dbContext.Cargos.FirstOrDefaultAsync(
            x => x.Id == cargoId && x.LojaId == lojaAtivaId,
            cancellationToken)
            ?? throw new InvalidOperationException("Cargo não encontrado na loja ativa.");

        var permissionIds = await ValidarPermissoesAsync(request.PermissaoIds, cancellationToken);
        var antes = await _dbContext.CargoPermissoes
            .Where(x => x.CargoId == cargo.Id)
            .Select(x => x.PermissaoId)
            .ToListAsync(cancellationToken);

        var existentes = await _dbContext.CargoPermissoes
            .Where(x => x.CargoId == cargo.Id)
            .ToListAsync(cancellationToken);

        _dbContext.CargoPermissoes.RemoveRange(existentes.Where(x => !permissionIds.Contains(x.PermissaoId)));

        var existingPermissionIds = existentes.Select(x => x.PermissaoId).ToHashSet();
        foreach (var permissionId in permissionIds.Where(id => !existingPermissionIds.Contains(id)))
        {
            _dbContext.CargoPermissoes.Add(new Domain.Models.CargoPermissao
            {
                Id = Guid.NewGuid(),
                CargoId = cargo.Id,
                PermissaoId = permissionId,
                CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            lojaAtivaId,
            "cargo",
            cargo.Id,
            "permissoes_atualizadas",
            new { permissaoIds = antes },
            new { permissaoIds = permissionIds },
            cancellationToken);

        return (await CarregarCargosAsync(lojaAtivaId, cancellationToken)).First(x => x.Id == cargo.Id);
    }

    /// <summary>
    /// Garante a existencia de uma loja ativa na sessao.
    /// </summary>
    private Guid EnsureActiveStore()
    {
        return _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Nenhuma loja ativa foi definida na sessão.");
    }

    /// <summary>
    /// Valida se todas as permissoes informadas existem e estao ativas.
    /// </summary>
    private async Task<HashSet<Guid>> ValidarPermissoesAsync(IReadOnlyList<Guid> permissaoIds, CancellationToken cancellationToken)
    {
        var ids = permissaoIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToHashSet();

        var total = await _dbContext.Permissoes.CountAsync(x => ids.Contains(x.Id) && x.Ativo, cancellationToken);
        if (total != ids.Count)
        {
            throw new InvalidOperationException("Uma ou mais permissões informadas são inválidas.");
        }

        return ids;
    }

    /// <summary>
    /// Carrega os cargos da loja com suas permissoes agregadas.
    /// </summary>
    private async Task<IReadOnlyList<RoleResponse>> CarregarCargosAsync(Guid lojaAtivaId, CancellationToken cancellationToken)
    {
        var cargos = await _dbContext.Cargos
            .AsNoTracking()
            .Where(x => x.LojaId == lojaAtivaId)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        var cargoIds = cargos.Select(x => x.Id).ToList();
        var permissoes = await (
                from cargoPermissao in _dbContext.CargoPermissoes
                join permissao in _dbContext.Permissoes on cargoPermissao.PermissaoId equals permissao.Id
                where cargoIds.Contains(cargoPermissao.CargoId)
                orderby permissao.Modulo, permissao.Nome
                select new
                {
                    cargoPermissao.CargoId,
                    Permissao = new PermissionResponse(
                        permissao.Id,
                        permissao.Codigo,
                        permissao.Nome,
                        permissao.Descricao,
                        permissao.Modulo,
                        permissao.Ativo),
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return cargos
            .Select(cargo => new RoleResponse(
                cargo.Id,
                cargo.Nome,
                cargo.Descricao,
                cargo.Ativo,
                permissoes
                    .Where(x => x.CargoId == cargo.Id)
                    .Select(x => x.Permissao)
                    .ToArray()))
            .ToArray();
    }
}
