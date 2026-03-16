namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato que expoe o contexto autenticado da requisicao atual.
public interface ICurrentRequestContext
{
    Guid? UsuarioId { get; }

    Guid? SessaoId { get; }

    Guid? LojaAtivaId { get; }

    string Ip { get; }

    string UserAgent { get; }

    bool IsAuthenticated { get; }
}
