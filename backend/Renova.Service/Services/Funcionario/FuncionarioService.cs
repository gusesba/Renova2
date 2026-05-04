using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Domain.Access;
using Renova.Service.Commands.Funcionario;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Funcionario;
using Renova.Service.Services.Acesso;

namespace Renova.Service.Services.Funcionario
{
    public class FuncionarioService(RenovaDbContext context, ILojaAuthorizationService? authorizationService = null) : IFuncionarioService
    {
        private readonly RenovaDbContext _context = context;
        private readonly ILojaAuthorizationService _authorizationService = authorizationService ?? NullLojaAuthorizationService.Instance;

        public async Task<FuncionarioDto> CreateAsync(
            CriarFuncionarioCommand request,
            CriarFuncionarioParametros parametros,
            CancellationToken cancellationToken = default)
        {
            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioAutenticadoId,
                FuncionalidadeCatalogo.FuncionariosAdicionar,
                cancellationToken);

            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(
                parametros.LojaId,
                parametros.UsuarioAutenticadoId,
                cancellationToken,
                lancarQuandoLojaNaoExistir: true);

            UsuarioModel usuario = await _context.Usuarios
                .SingleOrDefaultAsync(usuarioAtual => usuarioAtual.Id == request.UsuarioId, cancellationToken)
                ?? throw new KeyNotFoundException("Usuario informado nao foi encontrado.");

            CargoModel cargo = await _context.Cargos
                .SingleOrDefaultAsync(item => item.Id == request.CargoId && item.LojaId == loja.Id, cancellationToken)
                ?? throw new ArgumentException("Cargo informado nao foi encontrado para a loja.");

            bool funcionarioJaExiste = await _context.Funcionarios
                .AnyAsync(funcionario =>
                    funcionario.LojaId == loja.Id &&
                    funcionario.UsuarioId == usuario.Id,
                    cancellationToken);

            if (funcionarioJaExiste)
            {
                throw new InvalidOperationException("Usuario informado ja esta vinculado como funcionario da loja.");
            }

            FuncionarioModel funcionario = new()
            {
                LojaId = loja.Id,
                UsuarioId = usuario.Id,
                CargoId = cargo.Id
            };

            _ = await _context.Funcionarios.AddAsync(funcionario, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return CriarDto(usuario, loja.Id, cargo);
        }

        public async Task<IReadOnlyList<FuncionarioDto>> GetAllAsync(
            ObterFuncionariosParametros parametros,
            CancellationToken cancellationToken = default)
        {
            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioAutenticadoId,
                FuncionalidadeCatalogo.FuncionariosVisualizar,
                cancellationToken);

            List<FuncionarioModel> funcionarios = await _context.Funcionarios
                .Include(funcionario => funcionario.Usuario)
                .Where(funcionario => funcionario.LojaId == parametros.LojaId)
                .OrderBy(funcionario => funcionario.Usuario!.Nome)
                .ThenBy(funcionario => funcionario.UsuarioId)
                .ToListAsync(cancellationToken);

            return [.. funcionarios.Select(funcionario => new FuncionarioDto
            {
                UsuarioId = funcionario.UsuarioId,
                Nome = funcionario.Usuario?.Nome ?? string.Empty,
                Email = funcionario.Usuario?.Email ?? string.Empty,
                LojaId = funcionario.LojaId,
                CargoId = funcionario.CargoId,
                CargoNome = funcionario.Cargo?.Nome ?? "Funcionario"
            })];
        }

        public async Task<FuncionarioDto> UpdateCargoAsync(
            AtualizarFuncionarioCargoCommand request,
            ExcluirFuncionarioParametros parametros,
            CancellationToken cancellationToken = default)
        {
            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioAutenticadoId,
                FuncionalidadeCatalogo.FuncionariosEditar,
                cancellationToken);

            FuncionarioModel funcionario = await _context.Funcionarios
                .Include(item => item.Usuario)
                .SingleOrDefaultAsync(
                    item => item.LojaId == parametros.LojaId && item.UsuarioId == parametros.UsuarioId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Funcionario informado nao foi encontrado para a loja.");

            CargoModel cargo = await _context.Cargos
                .SingleOrDefaultAsync(item => item.Id == request.CargoId && item.LojaId == parametros.LojaId, cancellationToken)
                ?? throw new ArgumentException("Cargo informado nao foi encontrado para a loja.");

            funcionario.CargoId = cargo.Id;
            _ = await _context.SaveChangesAsync(cancellationToken);

            return CriarDto(funcionario.Usuario!, funcionario.LojaId, cargo);
        }

        public async Task DeleteAsync(
            ExcluirFuncionarioParametros parametros,
            CancellationToken cancellationToken = default)
        {
            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioAutenticadoId,
                FuncionalidadeCatalogo.FuncionariosRemover,
                cancellationToken);

            FuncionarioModel funcionario = await _context.Funcionarios
                .SingleOrDefaultAsync(funcionario =>
                    funcionario.LojaId == parametros.LojaId &&
                    funcionario.UsuarioId == parametros.UsuarioId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Funcionario informado nao foi encontrado para a loja.");

            _ = _context.Funcionarios.Remove(funcionario);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        private static FuncionarioDto CriarDto(UsuarioModel usuario, int lojaId, CargoModel cargo)
        {
            return new FuncionarioDto
            {
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                LojaId = lojaId,
                CargoId = cargo.Id,
                CargoNome = cargo.Nome
            };
        }
    }
}
