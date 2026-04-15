using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Funcionario;
using Renova.Service.Extensions;
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
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(
                parametros.LojaId,
                parametros.UsuarioAutenticadoId,
                cancellationToken,
                lancarQuandoLojaNaoExistir: true);

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
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(
                parametros.LojaId,
                parametros.UsuarioAutenticadoId,
                cancellationToken,
                lancarQuandoLojaNaoExistir: true);

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
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(
                parametros.LojaId,
                parametros.UsuarioAutenticadoId,
                cancellationToken,
                lancarQuandoLojaNaoExistir: true);

            FuncionarioModel funcionario = await _context.Funcionarios
                .SingleOrDefaultAsync(funcionario =>
                    funcionario.LojaId == loja.Id &&
                    funcionario.UsuarioId == parametros.UsuarioId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Funcionario informado nao foi encontrado para a loja.");

            _ = _context.Funcionarios.Remove(funcionario);
            _ = await _context.SaveChangesAsync(cancellationToken);
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
