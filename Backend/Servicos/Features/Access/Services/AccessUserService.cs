using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Contracts;

namespace Renova.Services.Features.Access.Services;

// Representa o servico de manutencao cadastral de usuarios.
public sealed class AccessUserService : IAccessUserService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o servico com persistencia, hash de senha e auditoria.
    /// </summary>
    public AccessUserService(
        RenovaDbContext dbContext,
        IPasswordHasher passwordHasher,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Lista os usuarios do sistema com o resumo de vinculo na loja ativa.
    /// </summary>
    public async Task<IReadOnlyList<UserSummaryResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var lojaAtivaId = EnsureActiveStore();

        var usuarios = await _dbContext.Usuarios
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        var vinculos = await CarregarVinculosAsync(lojaAtivaId, cancellationToken);
        return usuarios.Select(usuario => MapUser(usuario, vinculos)).ToArray();
    }

    /// <summary>
    /// Cria um novo usuario com senha inicial e auditoria.
    /// </summary>
    public async Task<UserSummaryResponse> CriarAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUserInput(request.Nome, request.Email, request.Telefone);

        if (await _dbContext.Usuarios.AnyAsync(x => x.Email.ToLower() == NormalizeEmail(request.Email), cancellationToken))
        {
            throw new InvalidOperationException("Já existe um usuário com o e-mail informado.");
        }

        if (request.PessoaId.HasValue && !await _dbContext.Pessoas.AnyAsync(x => x.Id == request.PessoaId.Value, cancellationToken))
        {
            throw new InvalidOperationException("A pessoa vinculada não foi encontrada.");
        }

        var senha = _passwordHasher.Hash(request.Senha);
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome.Trim(),
            Email = NormalizeEmail(request.Email),
            Telefone = request.Telefone.Trim(),
            SenhaHash = senha.Hash,
            SenhaSalt = senha.Salt,
            StatusUsuario = AccessStatusValues.Usuario.Ativo,
            PessoaId = request.PessoaId,
            CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
        };

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            _currentRequestContext.LojaAtivaId,
            "usuario",
            usuario.Id,
            "criado",
            null,
            new { usuario.Nome, usuario.Email, usuario.Telefone, usuario.PessoaId, usuario.StatusUsuario },
            cancellationToken);

        var vinculos = _currentRequestContext.LojaAtivaId is null
            ? new Dictionary<Guid, StoreMembershipSummaryResponse>()
            : await CarregarVinculosAsync(_currentRequestContext.LojaAtivaId.Value, cancellationToken);

        return MapUser(usuario, vinculos);
    }

    /// <summary>
    /// Atualiza os dados cadastrais do proprio usuario autenticado.
    /// </summary>
    public async Task<UserSummaryResponse> AtualizarAsync(Guid usuarioId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        EnsureCurrentUserCanEditSelf(usuarioId);
        ValidateUserInput(request.Nome, request.Email, request.Telefone);

        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(x => x.Id == usuarioId, cancellationToken)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var emailNormalizado = NormalizeEmail(request.Email);
        var emailEmUso = await _dbContext.Usuarios.AnyAsync(
            x => x.Id != usuarioId && x.Email.ToLower() == emailNormalizado,
            cancellationToken);

        if (emailEmUso)
        {
            throw new InvalidOperationException("Já existe um usuário com o e-mail informado.");
        }

        if (request.PessoaId.HasValue && !await _dbContext.Pessoas.AnyAsync(x => x.Id == request.PessoaId.Value, cancellationToken))
        {
            throw new InvalidOperationException("A pessoa vinculada não foi encontrada.");
        }

        var antes = new { usuario.Nome, usuario.Email, usuario.Telefone, usuario.PessoaId };

        usuario.Nome = request.Nome.Trim();
        usuario.Email = emailNormalizado;
        usuario.Telefone = request.Telefone.Trim();
        usuario.PessoaId = request.PessoaId;
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;
        usuario.AtualizadoPorUsuarioId = _currentRequestContext.UsuarioId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            _currentRequestContext.LojaAtivaId,
            "usuario",
            usuario.Id,
            "atualizado",
            antes,
            new { usuario.Nome, usuario.Email, usuario.Telefone, usuario.PessoaId },
            cancellationToken);

        var vinculos = _currentRequestContext.LojaAtivaId is null
            ? new Dictionary<Guid, StoreMembershipSummaryResponse>()
            : await CarregarVinculosAsync(_currentRequestContext.LojaAtivaId.Value, cancellationToken);

        return MapUser(usuario, vinculos);
    }

    /// <summary>
    /// Altera o status do usuario e revoga sessoes quando necessario.
    /// </summary>
    public async Task<UserSummaryResponse> AlterarStatusAsync(Guid usuarioId, ChangeUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (!AccessStatusValues.Usuario.Todos.Contains(request.StatusUsuario.Trim()))
        {
            throw new InvalidOperationException("Status de usuário inválido.");
        }

        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(x => x.Id == usuarioId, cancellationToken)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var status = request.StatusUsuario.Trim().ToLowerInvariant();
        var antes = usuario.StatusUsuario;

        usuario.StatusUsuario = status;
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;
        usuario.AtualizadoPorUsuarioId = _currentRequestContext.UsuarioId;

        if (!string.Equals(status, AccessStatusValues.Usuario.Ativo, StringComparison.OrdinalIgnoreCase))
        {
            var sessoes = await _dbContext.UsuarioSessoes
                .Where(x => x.UsuarioId == usuario.Id && x.RevogadoEm == null)
                .ToListAsync(cancellationToken);

            foreach (var sessao in sessoes)
            {
                sessao.RevogadoEm = DateTimeOffset.UtcNow;
                sessao.AtualizadoEm = DateTimeOffset.UtcNow;
                sessao.AtualizadoPorUsuarioId = _currentRequestContext.UsuarioId;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            _currentRequestContext.LojaAtivaId,
            "usuario",
            usuario.Id,
            "status_alterado",
            new { statusUsuario = antes },
            new { statusUsuario = usuario.StatusUsuario },
            cancellationToken);

        var vinculos = _currentRequestContext.LojaAtivaId is null
            ? new Dictionary<Guid, StoreMembershipSummaryResponse>()
            : await CarregarVinculosAsync(_currentRequestContext.LojaAtivaId.Value, cancellationToken);

        return MapUser(usuario, vinculos);
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
    /// Garante que apenas o proprio usuario edite seu cadastro.
    /// </summary>
    private void EnsureCurrentUserCanEditSelf(Guid usuarioId)
    {
        var currentUserId = _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");

        if (currentUserId != usuarioId)
        {
            throw new InvalidOperationException("Voce so pode editar o seu proprio usuario.");
        }
    }

    /// <summary>
    /// Carrega os vinculos e cargos dos usuarios na loja ativa.
    /// </summary>
    private async Task<Dictionary<Guid, StoreMembershipSummaryResponse>> CarregarVinculosAsync(Guid lojaAtivaId, CancellationToken cancellationToken)
    {
        var vinculos = await _dbContext.UsuarioLojas
            .AsNoTracking()
            .Where(x => x.LojaId == lojaAtivaId)
            .ToListAsync(cancellationToken);

        var vinculoIds = vinculos.Select(x => x.Id).ToList();
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

        return vinculos.ToDictionary(
            x => x.UsuarioId,
            x => new StoreMembershipSummaryResponse(
                x.Id,
                x.StatusVinculo,
                x.EhResponsavel,
                cargos
                    .Where(c => c.UsuarioLojaId == x.Id)
                    .Select(c => new RoleReferenceResponse(c.Id, c.Nome))
                    .ToArray()));
    }

    /// <summary>
    /// Monta a resposta final do usuario com o resumo de vinculo.
    /// </summary>
    private static UserSummaryResponse MapUser(Usuario usuario, IReadOnlyDictionary<Guid, StoreMembershipSummaryResponse> vinculos)
    {
        vinculos.TryGetValue(usuario.Id, out var vinculo);

        return new UserSummaryResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Telefone,
            usuario.StatusUsuario,
            usuario.PessoaId,
            vinculo);
    }

    /// <summary>
    /// Valida os campos obrigatorios de entrada do usuario.
    /// </summary>
    private static void ValidateUserInput(string nome, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("O nome do usuário é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new InvalidOperationException("O telefone do usuário é obrigatório.");
        }

        _ = NormalizeEmail(email);
    }

    /// <summary>
    /// Normaliza e valida o e-mail do usuario.
    /// </summary>
    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("O e-mail do usuário é obrigatório.");
        }

        return email.Trim().ToLowerInvariant();
    }
}
