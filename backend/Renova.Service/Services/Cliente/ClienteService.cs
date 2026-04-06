using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cliente;
using Renova.Service.Parameters.Cliente;

namespace Renova.Service.Services.Cliente
{
    public class ClienteService(RenovaDbContext context) : IClienteService
    {
        private readonly RenovaDbContext _context = context;

        public async Task<ClienteDto> CreateAsync(CriarClienteCommand request, CriarClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();
            string contatoNormalizado = request.Contato.Trim();

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == request.LojaId, cancellationToken);

            if (loja is null || loja.UsuarioId != parametros.UsuarioId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

            if (request.UserId.HasValue)
            {
                bool contaExiste = await _context.Usuarios
                    .AnyAsync(usuario => usuario.Id == request.UserId.Value, cancellationToken);

                if (!contaExiste)
                {
                    throw new InvalidOperationException("Conta informada para vinculo nao foi encontrada.");
                }
            }

            bool clienteJaExiste = await _context.Clientes
                .AnyAsync(cliente => cliente.LojaId == request.LojaId && cliente.Nome == nomeNormalizado, cancellationToken);

            if (clienteJaExiste)
            {
                throw new InvalidOperationException("Loja ja possui um cliente com este nome.");
            }

            ClienteModel cliente = new()
            {
                Nome = nomeNormalizado,
                Contato = contatoNormalizado,
                LojaId = request.LojaId,
                UserId = request.UserId
            };

            _ = await _context.Clientes.AddAsync(cliente, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Contato = cliente.Contato,
                LojaId = cliente.LojaId,
                UserId = cliente.UserId
            };
        }
    }
}
