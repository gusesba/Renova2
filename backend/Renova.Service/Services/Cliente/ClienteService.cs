using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cliente;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Queries.Cliente;
using System.Linq.Expressions;

namespace Renova.Service.Services.Cliente
{
    public class ClienteService(RenovaDbContext context) : IClienteService
    {
        private readonly RenovaDbContext _context = context;
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<ClienteModel, int>>)(cliente => cliente.Id),
            ["nome"] = (Expression<Func<ClienteModel, string>>)(cliente => cliente.Nome),
            ["contato"] = (Expression<Func<ClienteModel, string>>)(cliente => cliente.Contato)
        };

        public async Task<ClienteDto> CreateAsync(CriarClienteCommand request, CriarClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();
            string contatoNormalizado = request.Contato.KeepOnlyDigits();

            if (contatoNormalizado.Length is not (10 or 11))
            {
                throw new ArgumentException("Contato deve conter 10 ou 11 numeros.", nameof(request));
            }

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

        public async Task<ClienteDto> EditAsync(EditarClienteCommand request, EditarClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();
            string contatoNormalizado = request.Contato.KeepOnlyDigits();

            if (contatoNormalizado.Length is not (10 or 11))
            {
                throw new ArgumentException("Contato deve conter 10 ou 11 numeros.", nameof(request));
            }

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            ClienteModel? cliente = await _context.Clientes
                .SingleOrDefaultAsync(clienteAtual => clienteAtual.Id == parametros.ClienteId, cancellationToken);

            if (cliente is null)
            {
                throw new KeyNotFoundException("Cliente informado nao foi encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == cliente.LojaId, cancellationToken);

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
                .AnyAsync(clienteAtual =>
                    clienteAtual.LojaId == cliente.LojaId &&
                    clienteAtual.Id != cliente.Id &&
                    clienteAtual.Nome == nomeNormalizado,
                    cancellationToken);

            if (clienteJaExiste)
            {
                throw new InvalidOperationException("Loja ja possui um cliente com este nome.");
            }

            cliente.Nome = nomeNormalizado;
            cliente.Contato = contatoNormalizado;
            cliente.UserId = request.UserId;

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

        public async Task DeleteAsync(ExcluirClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            ClienteModel? cliente = await _context.Clientes
                .SingleOrDefaultAsync(clienteAtual => clienteAtual.Id == parametros.ClienteId, cancellationToken);

            if (cliente is null)
            {
                throw new KeyNotFoundException("Cliente informado nao foi encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == cliente.LojaId, cancellationToken);

            if (loja is null || loja.UsuarioId != parametros.UsuarioId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

            if (await ClientePossuiRelacionamentosAtivosAsync(cliente.Id, cancellationToken))
            {
                throw new InvalidOperationException("Cliente possui relacionamentos ativos e nao pode ser excluido.");
            }

            _ = _context.Clientes.Remove(cliente);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<PaginacaoDto<ClienteDto>> GetAllAsync(ObterClientesQuery request, ObterClientesParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel? loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == request.LojaId.Value, cancellationToken);

            if (loja is null || loja.UsuarioId != parametros.UsuarioId)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

            IQueryable<ClienteModel> query = _context.Clientes
                .Where(cliente => cliente.LojaId == request.LojaId.Value);

            if (!string.IsNullOrWhiteSpace(request.Nome))
            {
                string nomeFiltro = request.Nome.Trim().ToLowerInvariant();
                query = query.Where(cliente => cliente.Nome.ToLower().Contains(nomeFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Contato))
            {
                string contatoFiltro = request.Contato.Trim().ToLowerInvariant();
                query = query.Where(cliente => cliente.Contato.ToLower().Contains(contatoFiltro));
            }

            IQueryable<ClienteModel> queryOrdenada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveis, "nome")
                .ThenBy(cliente => cliente.Id);

            IQueryable<ClienteDto> queryProjetada = queryOrdenada.Select(cliente => new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Contato = cliente.Contato,
                LojaId = cliente.LojaId,
                UserId = cliente.UserId
            });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        private static Task<bool> ClientePossuiRelacionamentosAtivosAsync(int clienteId, CancellationToken cancellationToken)
        {
            _ = clienteId;
            _ = cancellationToken;

            // TODO: quando existirem tabelas relacionadas ao cliente, validar dependencias ativas
            // antes da exclusao e bloquear com mensagem de negocio adequada.
            return Task.FromResult(false);
        }
    }
}
