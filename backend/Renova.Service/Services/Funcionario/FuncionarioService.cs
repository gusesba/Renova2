using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Funcionario;
using Renova.Service.Parameters.Funcionario;

namespace Renova.Service.Services.Funcionario
{
    public class FuncionarioService(RenovaDbContext context) : IFuncionarioService
    {
        private readonly RenovaDbContext _context = context;

        public async Task<FuncionarioDto> CreateAsync(
            CriarFuncionarioCommand request,
            CriarFuncionarioParametros parametros,
            CancellationToken cancellationToken = default)
        {
            LojaModel loja = await ObterLojaDoUsuarioAsync(parametros.LojaId, parametros.UsuarioAutenticadoId, cancellationToken);

            UsuarioModel usuario = await _context.Usuarios
                .SingleOrDefaultAsync(usuarioAtual => usuarioAtual.Id == request.UsuarioId, cancellationToken)
                ?? throw new KeyNotFoundException("Usuario informado nao foi encontrado.");

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
                UsuarioId = usuario.Id
            };

            _ = await _context.Funcionarios.AddAsync(funcionario, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return CriarDto(usuario, loja.Id);
        }

        public async Task<IReadOnlyList<FuncionarioDto>> GetAllAsync(
            ObterFuncionariosParametros parametros,
            CancellationToken cancellationToken = default)
        {
            LojaModel loja = await ObterLojaDoUsuarioAsync(parametros.LojaId, parametros.UsuarioAutenticadoId, cancellationToken);

            return (IReadOnlyList<FuncionarioDto>)await _context.Funcionarios
                .Where(funcionario => funcionario.LojaId == loja.Id)
                .OrderBy(funcionario => funcionario.Usuario!.Nome)
                .ThenBy(funcionario => funcionario.UsuarioId)
                .Select(funcionario => new FuncionarioDto
                {
                    UsuarioId = funcionario.UsuarioId,
                    Nome = funcionario.Usuario!.Nome,
                    Email = funcionario.Usuario.Email,
                    LojaId = funcionario.LojaId
                })
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            ExcluirFuncionarioParametros parametros,
            CancellationToken cancellationToken = default)
        {
            LojaModel loja = await ObterLojaDoUsuarioAsync(parametros.LojaId, parametros.UsuarioAutenticadoId, cancellationToken);

            FuncionarioModel funcionario = await _context.Funcionarios
                .SingleOrDefaultAsync(funcionario =>
                    funcionario.LojaId == loja.Id &&
                    funcionario.UsuarioId == parametros.UsuarioId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Funcionario informado nao foi encontrado para a loja.");

            _ = _context.Funcionarios.Remove(funcionario);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<LojaModel> ObterLojaDoUsuarioAsync(int lojaId, int usuarioAutenticadoId, CancellationToken cancellationToken)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == usuarioAutenticadoId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == lojaId, cancellationToken);

            if (loja is null)
            {
                throw new KeyNotFoundException("Loja informada nao foi encontrada.");
            }

            if (loja.UsuarioId != usuarioAutenticadoId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

            return loja;
        }

        private static FuncionarioDto CriarDto(UsuarioModel usuario, int lojaId)
        {
            return new FuncionarioDto
            {
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                LojaId = lojaId
            };
        }
    }
}
