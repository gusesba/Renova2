using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Contracts;

namespace Renova.Services.Features.Access.Services;

// Representa o servico de vinculacao de usuarios e cargos por loja.
public sealed class AccessStoreMembershipService : IAccessStoreMembershipService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o servico com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public AccessStoreMembershipService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Lista os vinculos da loja ativa com seus cargos.
    /// </summary>
    public async Task<IReadOnlyList<StoreMembershipResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();
        return await CarregarVinculosAsync(lojaAtivaId, cancellationToken);
    }

    /// <summary>
    /// Cria um novo vinculo de usuario na loja ativa.
    /// </summary>
    public async Task<StoreMembershipResponse> CriarAsync(
        CreateStoreMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();
        ValidateStatus(request.StatusVinculo);
        await ValidarUsuarioECargosAsync(request.UsuarioId, lojaAtivaId, request.CargoIds, cancellationToken);

        var existente = await _dbContext.UsuarioLojas.FirstOrDefaultAsync(
            x => x.UsuarioId == request.UsuarioId && x.LojaId == lojaAtivaId,
            cancellationToken);

        if (existente is not null)
        {
            throw new InvalidOperationException("O usuário já possui vínculo com a loja ativa.");
        }

        var usuarioLoja = new UsuarioLoja
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            LojaId = lojaAtivaId,
            StatusVinculo = NormalizeStatus(request.StatusVinculo),
            EhResponsavel = request.EhResponsavel,
            DataInicio = DateTimeOffset.UtcNow,
            DataFim = request.DataFim,
            CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
        };

        _dbContext.UsuarioLojas.Add(usuarioLoja);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var cargoId in request.CargoIds.Distinct())
        {
            _dbContext.UsuarioLojaCargos.Add(new UsuarioLojaCargo
            {
                Id = Guid.NewGuid(),
                UsuarioLojaId = usuarioLoja.Id,
                CargoId = cargoId,
                CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            lojaAtivaId,
            "usuario_loja",
            usuarioLoja.Id,
            "criado",
            null,
            new
            {
                usuarioLoja.UsuarioId,
                usuarioLoja.StatusVinculo,
                usuarioLoja.EhResponsavel,
                request.CargoIds,
            },
            cancellationToken);

        return (await CarregarVinculosAsync(lojaAtivaId, cancellationToken)).First(x => x.Id == usuarioLoja.Id);
    }

    /// <summary>
    /// Atualiza status e dados operacionais de um vinculo existente.
    /// </summary>
    public async Task<StoreMembershipResponse> AtualizarAsync(
        Guid usuarioLojaId,
        UpdateStoreMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();
        ValidateStatus(request.StatusVinculo);

        var usuarioLoja = await _dbContext.UsuarioLojas.FirstOrDefaultAsync(
            x => x.Id == usuarioLojaId && x.LojaId == lojaAtivaId,
            cancellationToken)
            ?? throw new InvalidOperationException("Vínculo de usuário não encontrado na loja ativa.");

        var antes = new
        {
            usuarioLoja.StatusVinculo,
            usuarioLoja.EhResponsavel,
            usuarioLoja.DataFim,
        };

        usuarioLoja.StatusVinculo = NormalizeStatus(request.StatusVinculo);
        usuarioLoja.EhResponsavel = request.EhResponsavel;
        usuarioLoja.DataFim = request.DataFim;
        usuarioLoja.AtualizadoEm = DateTimeOffset.UtcNow;
        usuarioLoja.AtualizadoPorUsuarioId = _currentRequestContext.UsuarioId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            lojaAtivaId,
            "usuario_loja",
            usuarioLoja.Id,
            "atualizado",
            antes,
            new
            {
                usuarioLoja.StatusVinculo,
                usuarioLoja.EhResponsavel,
                usuarioLoja.DataFim,
            },
            cancellationToken);

        return (await CarregarVinculosAsync(lojaAtivaId, cancellationToken)).First(x => x.Id == usuarioLoja.Id);
    }

    /// <summary>
    /// Atualiza os cargos associados a um vinculo de usuario.
    /// </summary>
    public async Task<StoreMembershipResponse> AtualizarCargosAsync(
        Guid usuarioLojaId,
        UpdateStoreMembershipRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();

        var usuarioLoja = await _dbContext.UsuarioLojas.FirstOrDefaultAsync(
            x => x.Id == usuarioLojaId && x.LojaId == lojaAtivaId,
            cancellationToken)
            ?? throw new InvalidOperationException("Vínculo de usuário não encontrado na loja ativa.");

        await ValidarUsuarioECargosAsync(usuarioLoja.UsuarioId, lojaAtivaId, request.CargoIds, cancellationToken);

        var existentes = await _dbContext.UsuarioLojaCargos
            .Where(x => x.UsuarioLojaId == usuarioLoja.Id)
            .ToListAsync(cancellationToken);

        var antes = existentes.Select(x => x.CargoId).ToArray();
        _dbContext.UsuarioLojaCargos.RemoveRange(existentes.Where(x => !request.CargoIds.Contains(x.CargoId)));

        var existingCargoIds = existentes.Select(x => x.CargoId).ToHashSet();
        foreach (var cargoId in request.CargoIds.Where(id => !existingCargoIds.Contains(id)).Distinct())
        {
            _dbContext.UsuarioLojaCargos.Add(new UsuarioLojaCargo
            {
                Id = Guid.NewGuid(),
                UsuarioLojaId = usuarioLoja.Id,
                CargoId = cargoId,
                CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            lojaAtivaId,
            "usuario_loja",
            usuarioLoja.Id,
            "cargos_atualizados",
            new { cargoIds = antes },
            new { cargoIds = request.CargoIds.Distinct().ToArray() },
            cancellationToken);

        return (await CarregarVinculosAsync(lojaAtivaId, cancellationToken)).First(x => x.Id == usuarioLoja.Id);
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
    /// Valida se o usuario existe e se os cargos pertencem a loja ativa.
    /// </summary>
    private async Task ValidarUsuarioECargosAsync(
        Guid usuarioId,
        Guid lojaAtivaId,
        IReadOnlyList<Guid> cargoIds,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.Usuarios.AnyAsync(x => x.Id == usuarioId, cancellationToken))
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        var validos = await _dbContext.Cargos.CountAsync(
            x => x.LojaId == lojaAtivaId && cargoIds.Contains(x.Id) && x.Ativo,
            cancellationToken);

        if (validos != cargoIds.Distinct().Count())
        {
            throw new InvalidOperationException("Um ou mais cargos informados não pertencem à loja ativa.");
        }
    }

    /// <summary>
    /// Carrega os vinculos da loja com seus cargos agregados.
    /// </summary>
    private async Task<IReadOnlyList<StoreMembershipResponse>> CarregarVinculosAsync(Guid lojaAtivaId, CancellationToken cancellationToken)
    {
        var vinculos = await (
                from usuarioLoja in _dbContext.UsuarioLojas
                join usuario in _dbContext.Usuarios on usuarioLoja.UsuarioId equals usuario.Id
                where usuarioLoja.LojaId == lojaAtivaId
                orderby usuario.Nome
                select new
                {
                    UsuarioLoja = usuarioLoja,
                    Usuario = usuario,
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var vinculoIds = vinculos.Select(x => x.UsuarioLoja.Id).ToList();
        var cargos = await (
                from usuarioLojaCargo in _dbContext.UsuarioLojaCargos
                join cargo in _dbContext.Cargos on usuarioLojaCargo.CargoId equals cargo.Id
                where vinculoIds.Contains(usuarioLojaCargo.UsuarioLojaId)
                select new
                {
                    usuarioLojaCargo.UsuarioLojaId,
                    cargo.Id,
                    cargo.Nome,
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return vinculos.Select(vinculo => new StoreMembershipResponse(
                vinculo.UsuarioLoja.Id,
                vinculo.Usuario.Id,
                vinculo.Usuario.Nome,
                vinculo.Usuario.Email,
                vinculo.UsuarioLoja.StatusVinculo,
                vinculo.UsuarioLoja.EhResponsavel,
                vinculo.UsuarioLoja.DataInicio,
                vinculo.UsuarioLoja.DataFim,
                cargos
                    .Where(x => x.UsuarioLojaId == vinculo.UsuarioLoja.Id)
                    .Select(x => new RoleReferenceResponse(x.Id, x.Nome))
                    .ToArray()))
            .ToArray();
    }

    /// <summary>
    /// Valida se o status de vinculo pertence ao catalogo aceito.
    /// </summary>
    private static void ValidateStatus(string status)
    {
        if (!AccessStatusValues.VinculoLoja.Todos.Contains(status.Trim()))
        {
            throw new InvalidOperationException("Status do vínculo é inválido.");
        }
    }

    /// <summary>
    /// Normaliza o status de vinculo para persistencia.
    /// </summary>
    private static string NormalizeStatus(string status)
    {
        return status.Trim().ToLowerInvariant();
    }
}
