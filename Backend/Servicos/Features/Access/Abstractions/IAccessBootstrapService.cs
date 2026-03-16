namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato de inicializacao tecnica do modulo de acesso.
public interface IAccessBootstrapService
{
    /// <summary>
    /// Garante a carga inicial de permissoes, cargos base e dados de bootstrap.
    /// </summary>
    Task InicializarAsync(bool isDevelopment, CancellationToken cancellationToken = default);
}
