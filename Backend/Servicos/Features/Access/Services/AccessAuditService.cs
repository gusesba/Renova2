using System.Text.Json;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access.Abstractions;

namespace Renova.Services.Features.Access.Services;

// Representa o servico que grava eventos de acesso e trilhas de auditoria.
public sealed class AccessAuditService : IAccessAuditService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RenovaDbContext _dbContext;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o servico com persistencia e contexto da requisicao.
    /// </summary>
    public AccessAuditService(RenovaDbContext dbContext, ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Registra um evento operacional ligado ao ciclo de acesso do usuario.
    /// </summary>
    public async Task RegistrarEventoAcessoAsync(
        Guid usuarioId,
        string tipoEvento,
        object? detalhes = null,
        CancellationToken cancellationToken = default)
    {
        var evento = new UsuarioAcessoEvento
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            TipoEvento = tipoEvento,
            OcorridoEm = DateTimeOffset.UtcNow,
            Ip = _currentRequestContext.Ip,
            UserAgent = _currentRequestContext.UserAgent,
            DetalhesJson = detalhes is null ? null : JsonSerializer.Serialize(detalhes, SerializerOptions),
            CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
        };

        _dbContext.UsuarioAcessoEventos.Add(evento);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Registra uma alteracao de entidade para rastreabilidade administrativa.
    /// </summary>
    public async Task RegistrarAuditoriaAsync(
        Guid? lojaId,
        string entidade,
        Guid entidadeId,
        string acao,
        object? antes,
        object? depois,
        CancellationToken cancellationToken = default)
    {
        if (_currentRequestContext.UsuarioId is null)
        {
            return;
        }

        var auditoria = new AuditoriaEvento
        {
            Id = Guid.NewGuid(),
            LojaId = lojaId,
            UsuarioId = _currentRequestContext.UsuarioId.Value,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Acao = acao,
            AntesJson = antes is null ? null : JsonSerializer.Serialize(antes, SerializerOptions),
            DepoisJson = depois is null ? null : JsonSerializer.Serialize(depois, SerializerOptions),
            OcorridoEm = DateTimeOffset.UtcNow,
            CriadoPorUsuarioId = _currentRequestContext.UsuarioId,
        };

        _dbContext.AuditoriaEventos.Add(auditoria);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
