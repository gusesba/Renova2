using Microsoft.EntityFrameworkCore;

using Renova.Domain.Access;
using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Extensions;

namespace Renova.Service.Services.Acesso
{
    public class LojaAuthorizationService(RenovaDbContext context) : ILojaAuthorizationService
    {
        private readonly RenovaDbContext _context = context;

        public async Task EnsureStoreAccessAsync(int lojaId, int usuarioId, CancellationToken cancellationToken = default)
        {
            _ = await _context.ObterLojaAcessivelAoUsuarioAsync(lojaId, usuarioId, cancellationToken);
        }

        public async Task EnsurePermissionAsync(int lojaId, int usuarioId, string funcionalidadeChave, CancellationToken cancellationToken = default)
        {
            AcessoLojaDto acesso = await GetAccessAsync(lojaId, usuarioId, cancellationToken);

            if (acesso.EhDono || acesso.Funcionalidades.Contains(funcionalidadeChave, StringComparer.Ordinal))
            {
                return;
            }

            FuncionalidadeCatalogItem funcionalidade = FuncionalidadeCatalogo.ObterPorChave(funcionalidadeChave);
            throw new UnauthorizedAccessException(
                $"Voce nao tem permissao para {funcionalidade.Descricao.ToLowerInvariant()}");
        }

        public async Task<AcessoLojaDto> GetAccessAsync(int lojaId, int usuarioId, CancellationToken cancellationToken = default)
        {
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(lojaId, usuarioId, cancellationToken);

            if (loja.UsuarioId == usuarioId)
            {
                return new AcessoLojaDto
                {
                    LojaId = lojaId,
                    EhDono = true,
                    Funcionalidades = FuncionalidadeCatalogo.TodasAsChaves
                };
            }

            FuncionarioModel funcionario = await _context.Funcionarios
                .AsNoTracking()
                .Include(item => item.Cargo)
                .ThenInclude(cargo => cargo!.Funcionalidades)
                .ThenInclude(item => item.Funcionalidade)
                .SingleOrDefaultAsync(
                    item => item.LojaId == lojaId && item.UsuarioId == usuarioId,
                    cancellationToken)
                ?? throw new UnauthorizedAccessException("Funcionario nao encontrado para a loja informada.");

            return new AcessoLojaDto
            {
                LojaId = lojaId,
                EhDono = false,
                CargoId = funcionario.CargoId,
                CargoNome = funcionario.Cargo?.Nome,
                Funcionalidades = [.. funcionario.Cargo?.Funcionalidades
                    .Select(item => item.Funcionalidade!.Chave)
                    .Distinct()
                    .OrderBy(item => item, StringComparer.Ordinal).ToArray() ?? []]
            };
        }
    }
}
