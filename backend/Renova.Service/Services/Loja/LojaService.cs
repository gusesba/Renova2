using Microsoft.EntityFrameworkCore;

using Renova.Domain.Access;
using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Loja;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Loja;
using Renova.Service.Services.Acesso;

namespace Renova.Service.Services.Loja
{
    public class LojaService(RenovaDbContext context, ILojaAuthorizationService? authorizationService = null) : ILojaService
    {
        private readonly RenovaDbContext _context = context;
        private readonly ILojaAuthorizationService _authorizationService = authorizationService ?? NullLojaAuthorizationService.Instance;
        private const string MensagemLojaComRegistrosAtivos = "Nao e possivel excluir loja com registros ativos";

        public async Task<LojaDto> CreateAsync(CriarLojaCommand request, CriarLojaParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            bool lojaJaExiste = await _context.Lojas
                .AnyAsync(loja => loja.UsuarioId == parametros.UsuarioId && loja.Nome == nomeNormalizado, cancellationToken);

            if (lojaJaExiste)
            {
                throw new InvalidOperationException("Usuario ja possui uma loja com este nome.");
            }

            LojaModel loja = new()
            {
                Nome = nomeNormalizado,
                UsuarioId = parametros.UsuarioId
            };

            _ = await _context.Lojas.AddAsync(loja, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new LojaDto
            {
                Id = loja.Id,
                Nome = loja.Nome
            };
        }

        public async Task<LojaDto> EditAsync(EditarLojaCommand request, EditarLojaParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();

            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(
                parametros.LojaId,
                parametros.UsuarioId,
                cancellationToken,
                lancarQuandoLojaNaoExistir: true);

            await _authorizationService.EnsurePermissionAsync(loja.Id, parametros.UsuarioId, FuncionalidadeCatalogo.LojasEditar, cancellationToken);

            bool lojaJaExiste = await _context.Lojas
                .AnyAsync(lojaAtual =>
                    lojaAtual.UsuarioId == loja.UsuarioId &&
                    lojaAtual.Id != loja.Id &&
                    lojaAtual.Nome == nomeNormalizado,
                    cancellationToken);

            if (lojaJaExiste)
            {
                throw new InvalidOperationException("Usuario ja possui uma loja com este nome.");
            }

            loja.Nome = nomeNormalizado;

            _ = await _context.SaveChangesAsync(cancellationToken);

            return new LojaDto
            {
                Id = loja.Id,
                Nome = loja.Nome
            };
        }

        public async Task DeleteAsync(ExcluirLojaParametros parametros, CancellationToken cancellationToken = default)
        {
            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(
                parametros.LojaId,
                parametros.UsuarioId,
                cancellationToken,
                lancarQuandoLojaNaoExistir: true);

            await _authorizationService.EnsurePermissionAsync(loja.Id, parametros.UsuarioId, FuncionalidadeCatalogo.LojasExcluir, cancellationToken);

            if (await LojaPossuiRegistrosAtivosAsync(loja.Id, cancellationToken))
            {
                throw new InvalidOperationException(MensagemLojaComRegistrosAtivos);
            }

            _ = _context.Lojas.Remove(loja);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<LojaDto>> GetAllAsync(ObterLojasParametros parametros, CancellationToken cancellationToken = default)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            return (IReadOnlyList<LojaDto>)await _context.ObterLojasAcessiveisAoUsuario(parametros.UsuarioId)
                .Distinct()
                .OrderBy(loja => loja.Nome)
                .ThenBy(loja => loja.Id)
                .Select(loja => new LojaDto
                {
                    Id = loja.Id,
                    Nome = loja.Nome
                })
                .ToListAsync(cancellationToken);
        }

        private async Task<bool> LojaPossuiRegistrosAtivosAsync(int lojaId, CancellationToken cancellationToken)
        {
            return await _context.ConfiguracoesLoja.AnyAsync(config => config.LojaId == lojaId, cancellationToken)
                || await _context.Clientes.AnyAsync(cliente => cliente.LojaId == lojaId, cancellationToken)
                || await _context.ClientesCreditos.AnyAsync(credito => credito.LojaId == lojaId, cancellationToken)
                || await _context.ProdutosReferencia.AnyAsync(produto => produto.LojaId == lojaId, cancellationToken)
                || await _context.Marcas.AnyAsync(marca => marca.LojaId == lojaId, cancellationToken)
                || await _context.Tamanhos.AnyAsync(tamanho => tamanho.LojaId == lojaId, cancellationToken)
                || await _context.Cores.AnyAsync(cor => cor.LojaId == lojaId, cancellationToken)
                || await _context.ProdutosEstoque.AnyAsync(produto => produto.LojaId == lojaId, cancellationToken)
                || await _context.Movimentacoes.AnyAsync(movimentacao => movimentacao.LojaId == lojaId, cancellationToken)
                || await _context.Pagamentos.AnyAsync(pagamento => pagamento.LojaId == lojaId, cancellationToken)
                || await _context.PagamentosCredito.AnyAsync(pagamento => pagamento.LojaId == lojaId, cancellationToken);
        }
    }
}
