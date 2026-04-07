using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Produto;
using Renova.Service.Parameters.Produto;

namespace Renova.Service.Services.Produto
{
    public class ProdutoService(RenovaDbContext context) : IProdutoService
    {
        private readonly RenovaDbContext _context = context;

        public async Task<ProdutoDto> CreateAsync(CriarProdutoCommand request, CriarProdutoParametros parametros, CancellationToken cancellationToken = default)
        {
            if (request.Preco <= 0)
            {
                throw new ArgumentException("Preco deve ser maior que zero.", nameof(request));
            }

            string descricaoNormalizada = request.Descricao.Trim();

            LojaModel loja = await ObterLojaDoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            ClienteModel fornecedor = await _context.Clientes
                .SingleOrDefaultAsync(cliente => cliente.Id == request.FornecedorId, cancellationToken)
                ?? throw new ArgumentException("Fornecedor informado nao foi encontrado.", nameof(request));

            if (fornecedor.LojaId != loja.Id)
            {
                throw new ArgumentException("Fornecedor informado nao pertence a loja selecionada.", nameof(request));
            }

            ProdutoReferenciaModel produtoReferencia = await _context.ProdutosReferencia
                .SingleOrDefaultAsync(produto => produto.Id == request.ProdutoId, cancellationToken)
                ?? throw new ArgumentException("Produto informado nao foi encontrado.", nameof(request));

            MarcaModel marca = await _context.Marcas
                .SingleOrDefaultAsync(item => item.Id == request.MarcaId, cancellationToken)
                ?? throw new ArgumentException("Marca informada nao foi encontrada.", nameof(request));

            TamanhoModel tamanho = await _context.Tamanhos
                .SingleOrDefaultAsync(item => item.Id == request.TamanhoId, cancellationToken)
                ?? throw new ArgumentException("Tamanho informado nao foi encontrado.", nameof(request));

            CorModel cor = await _context.Cores
                .SingleOrDefaultAsync(item => item.Id == request.CorId, cancellationToken)
                ?? throw new ArgumentException("Cor informada nao foi encontrada.", nameof(request));

            if (produtoReferencia.LojaId != loja.Id || marca.LojaId != loja.Id || tamanho.LojaId != loja.Id || cor.LojaId != loja.Id)
            {
                throw new ArgumentException("Os registros auxiliares informados devem pertencer a loja selecionada.", nameof(request));
            }

            if (!Enum.IsDefined(request.Situacao))
            {
                throw new ArgumentException("Situacao informada e invalida.", nameof(request));
            }

            DateTime entrada = request.Entrada == default ? DateTime.UtcNow : request.Entrada;

            ProdutoEstoqueModel produto = new()
            {
                Preco = request.Preco,
                ProdutoId = request.ProdutoId,
                MarcaId = request.MarcaId,
                TamanhoId = request.TamanhoId,
                CorId = request.CorId,
                FornecedorId = request.FornecedorId,
                Descricao = descricaoNormalizada,
                Entrada = entrada,
                LojaId = request.LojaId,
                Situacao = request.Situacao,
                Consignado = request.Consignado
            };

            _ = await _context.ProdutosEstoque.AddAsync(produto, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return MapearProdutoDto(produto);
        }

        public Task<ProdutoAuxiliarDto> CreateProdutoAuxiliarAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return CriarAuxiliarAsync(
                request,
                parametros,
                _context.ProdutosReferencia,
                valor => new ProdutoReferenciaModel
                {
                    Valor = valor,
                    LojaId = request.LojaId
                },
                "Produto",
                cancellationToken);
        }

        public Task<ProdutoAuxiliarDto> CreateMarcaAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return CriarAuxiliarAsync(
                request,
                parametros,
                _context.Marcas,
                valor => new MarcaModel
                {
                    Valor = valor,
                    LojaId = request.LojaId
                },
                "Marca",
                cancellationToken);
        }

        public Task<ProdutoAuxiliarDto> CreateTamanhoAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return CriarAuxiliarAsync(
                request,
                parametros,
                _context.Tamanhos,
                valor => new TamanhoModel
                {
                    Valor = valor,
                    LojaId = request.LojaId
                },
                "Tamanho",
                cancellationToken);
        }

        public Task<ProdutoAuxiliarDto> CreateCorAsync(CriarProdutoAuxiliarCommand request, CriarProdutoAuxiliarParametros parametros, CancellationToken cancellationToken = default)
        {
            return CriarAuxiliarAsync(
                request,
                parametros,
                _context.Cores,
                valor => new CorModel
                {
                    Valor = valor,
                    LojaId = request.LojaId
                },
                "Cor",
                cancellationToken);
        }

        private async Task<ProdutoAuxiliarDto> CriarAuxiliarAsync<TModel>(
            CriarProdutoAuxiliarCommand request,
            CriarProdutoAuxiliarParametros parametros,
            DbSet<TModel> dbSet,
            Func<string, TModel> factory,
            string nomeEntidade,
            CancellationToken cancellationToken)
            where TModel : class
        {
            string valorNormalizado = request.Valor.Trim();

            _ = await ObterLojaDoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            bool valorJaExiste = await dbSet.AnyAsync(
                entity => EF.Property<int>(entity, "LojaId") == request.LojaId
                    && EF.Property<string>(entity, "Valor") == valorNormalizado,
                cancellationToken);

            if (valorJaExiste)
            {
                throw new InvalidOperationException($"{nomeEntidade} ja possui este valor cadastrado para a loja informada.");
            }

            TModel entity = factory(valorNormalizado);

            _ = await dbSet.AddAsync(entity, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new ProdutoAuxiliarDto
            {
                Id = ObterId(entity),
                Valor = ObterValor(entity),
                LojaId = ObterLojaId(entity)
            };
        }

        private async Task<LojaModel> ObterLojaDoUsuarioAsync(int lojaId, int usuarioId, CancellationToken cancellationToken)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == usuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel loja = await _context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == lojaId, cancellationToken)
                ?? throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");

            return loja.UsuarioId != usuarioId
                ? throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.")
                : loja;
        }

        private static ProdutoDto MapearProdutoDto(ProdutoEstoqueModel produto)
        {
            return new ProdutoDto
            {
                Id = produto.Id,
                Preco = produto.Preco,
                ProdutoId = produto.ProdutoId,
                MarcaId = produto.MarcaId,
                TamanhoId = produto.TamanhoId,
                CorId = produto.CorId,
                FornecedorId = produto.FornecedorId,
                Descricao = produto.Descricao,
                Entrada = produto.Entrada,
                LojaId = produto.LojaId,
                Situacao = produto.Situacao,
                Consignado = produto.Consignado
            };
        }

        private static int ObterId<TModel>(TModel entity)
            where TModel : class
        {
            return entity switch
            {
                ProdutoReferenciaModel item => item.Id,
                MarcaModel item => item.Id,
                TamanhoModel item => item.Id,
                CorModel item => item.Id,
                _ => throw new InvalidOperationException("Tipo auxiliar nao suportado.")
            };
        }

        private static string ObterValor<TModel>(TModel entity)
            where TModel : class
        {
            return entity switch
            {
                ProdutoReferenciaModel item => item.Valor,
                MarcaModel item => item.Valor,
                TamanhoModel item => item.Valor,
                CorModel item => item.Valor,
                _ => throw new InvalidOperationException("Tipo auxiliar nao suportado.")
            };
        }

        private static int ObterLojaId<TModel>(TModel entity)
            where TModel : class
        {
            return entity switch
            {
                ProdutoReferenciaModel item => item.LojaId,
                MarcaModel item => item.LojaId,
                TamanhoModel item => item.LojaId,
                CorModel item => item.LojaId,
                _ => throw new InvalidOperationException("Tipo auxiliar nao suportado.")
            };
        }
    }
}
