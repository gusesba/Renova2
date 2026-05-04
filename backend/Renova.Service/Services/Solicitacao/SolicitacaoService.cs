using Microsoft.EntityFrameworkCore;

using Renova.Domain.Access;
using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Solicitacao;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Solicitacao;
using Renova.Service.Queries.Solicitacao;
using Renova.Service.Services.Acesso;
using System.Linq.Expressions;

namespace Renova.Service.Services.Solicitacao
{
    public class SolicitacaoService(RenovaDbContext context, ILojaAuthorizationService authorizationService) : ISolicitacaoService
    {
        private readonly RenovaDbContext _context = context;
        private readonly ILojaAuthorizationService _authorizationService = authorizationService;

        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<SolicitacaoModel, int>>)(solicitacao => solicitacao.Id),
            ["descricao"] = (Expression<Func<SolicitacaoModel, string>>)(solicitacao => solicitacao.Descricao ?? string.Empty),
            ["produto"] = (Expression<Func<SolicitacaoModel, string>>)(solicitacao => solicitacao.Produto != null ? solicitacao.Produto.Valor : string.Empty),
            ["marca"] = (Expression<Func<SolicitacaoModel, string>>)(solicitacao => solicitacao.Marca != null ? solicitacao.Marca.Valor : string.Empty),
            ["tamanho"] = (Expression<Func<SolicitacaoModel, string>>)(solicitacao => solicitacao.Tamanho != null ? solicitacao.Tamanho.Valor : string.Empty),
            ["cor"] = (Expression<Func<SolicitacaoModel, string>>)(solicitacao => solicitacao.Cor != null ? solicitacao.Cor.Valor : string.Empty),
            ["cliente"] = (Expression<Func<SolicitacaoModel, string>>)(solicitacao => solicitacao.Cliente != null ? solicitacao.Cliente.Nome : string.Empty),
            ["precoMinimo"] = (Expression<Func<SolicitacaoModel, decimal?>>)(solicitacao => solicitacao.PrecoMinimo),
            ["precoMaximo"] = (Expression<Func<SolicitacaoModel, decimal?>>)(solicitacao => solicitacao.PrecoMaximo)
        };

        public async Task<SolicitacaoDto> CreateAsync(CriarSolicitacaoCommand request, CriarSolicitacaoParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!request.ClienteId.HasValue)
            {
                throw new ArgumentException("Cliente e obrigatorio.", nameof(request));
            }

            if (request.PrecoMaximo.HasValue && request.PrecoMaximo <= 0)
            {
                throw new ArgumentException("Preco maximo deve ser maior que zero.", nameof(request));
            }

            string descricaoNormalizada = request.Descricao?.Trim() ?? string.Empty;

            await _authorizationService.EnsurePermissionAsync(request.LojaId, parametros.UsuarioId, FuncionalidadeCatalogo.SolicitacoesAdicionar, cancellationToken);

            LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            ClienteModel cliente = await _context.Clientes
                .SingleOrDefaultAsync(item => item.Id == request.ClienteId.Value, cancellationToken)
                ?? throw new ArgumentException("Cliente informado nao foi encontrado.", nameof(request));

            if (cliente.LojaId != loja.Id)
            {
                throw new ArgumentException("Cliente informado nao pertence a loja selecionada.", nameof(request));
            }

            ProdutoReferenciaModel? produto = null;
            if (request.ProdutoId.HasValue)
            {
                produto = await _context.ProdutosReferencia
                    .SingleOrDefaultAsync(item => item.Id == request.ProdutoId.Value, cancellationToken)
                    ?? throw new ArgumentException("Produto informado nao foi encontrado.", nameof(request));

                if (produto.LojaId != loja.Id)
                {
                    throw new ArgumentException("Produto informado nao pertence a loja selecionada.", nameof(request));
                }
            }

            MarcaModel? marca = null;
            if (request.MarcaId.HasValue)
            {
                marca = await _context.Marcas
                    .SingleOrDefaultAsync(item => item.Id == request.MarcaId.Value, cancellationToken)
                    ?? throw new ArgumentException("Marca informada nao foi encontrada.", nameof(request));

                if (marca.LojaId != loja.Id)
                {
                    throw new ArgumentException("Marca informada nao pertence a loja selecionada.", nameof(request));
                }
            }

            TamanhoModel? tamanho = null;
            if (request.TamanhoId.HasValue)
            {
                tamanho = await _context.Tamanhos
                    .SingleOrDefaultAsync(item => item.Id == request.TamanhoId.Value, cancellationToken)
                    ?? throw new ArgumentException("Tamanho informado nao foi encontrado.", nameof(request));

                if (tamanho.LojaId != loja.Id)
                {
                    throw new ArgumentException("Tamanho informado nao pertence a loja selecionada.", nameof(request));
                }
            }

            CorModel? cor = null;
            if (request.CorId.HasValue)
            {
                cor = await _context.Cores
                    .SingleOrDefaultAsync(item => item.Id == request.CorId.Value, cancellationToken)
                    ?? throw new ArgumentException("Cor informada nao foi encontrada.", nameof(request));

                if (cor.LojaId != loja.Id)
                {
                    throw new ArgumentException("Cor informada nao pertence a loja selecionada.", nameof(request));
                }
            }

            SolicitacaoModel solicitacao = new()
            {
                ProdutoId = request.ProdutoId,
                MarcaId = request.MarcaId,
                TamanhoId = request.TamanhoId,
                CorId = request.CorId,
                ClienteId = request.ClienteId,
                Descricao = descricaoNormalizada,
                PrecoMinimo = request.PrecoMinimo,
                PrecoMaximo = request.PrecoMaximo,
                LojaId = request.LojaId
            };

            _ = await _context.Solicitacoes.AddAsync(solicitacao, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new SolicitacaoDto
            {
                Id = solicitacao.Id,
                ProdutoId = solicitacao.ProdutoId,
                MarcaId = solicitacao.MarcaId,
                TamanhoId = solicitacao.TamanhoId,
                CorId = solicitacao.CorId,
                ClienteId = solicitacao.ClienteId,
                Descricao = solicitacao.Descricao ?? string.Empty,
                PrecoMinimo = solicitacao.PrecoMinimo,
                PrecoMaximo = solicitacao.PrecoMaximo,
                LojaId = solicitacao.LojaId,
                ProdutosCompativeis = await ObterProdutosCompativeisAsync(solicitacao, cancellationToken)
            };
        }

        public async Task<PaginacaoDto<SolicitacaoBuscaDto>> GetAllAsync(ObterSolicitacoesQuery request, ObterSolicitacoesParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            await _authorizationService.EnsurePermissionAsync(request.LojaId.Value, parametros.UsuarioId, FuncionalidadeCatalogo.SolicitacoesVisualizar, cancellationToken);

            IQueryable<SolicitacaoModel> query = _context.Solicitacoes
                .Where(solicitacao => solicitacao.LojaId == request.LojaId.Value);

            if (request.Id.HasValue)
            {
                query = query.Where(solicitacao => solicitacao.Id == request.Id.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Descricao))
            {
                string descricaoFiltro = request.Descricao.Trim().ToLowerInvariant();
                query = query.Where(solicitacao => solicitacao.Descricao != null && solicitacao.Descricao.ToLower().Contains(descricaoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Produto))
            {
                string produtoFiltro = request.Produto.Trim().ToLowerInvariant();
                query = query.Where(solicitacao => solicitacao.Produto != null && solicitacao.Produto.Valor.ToLower().Contains(produtoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Marca))
            {
                string marcaFiltro = request.Marca.Trim().ToLowerInvariant();
                query = query.Where(solicitacao => solicitacao.Marca != null && solicitacao.Marca.Valor.ToLower().Contains(marcaFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Tamanho))
            {
                string tamanhoFiltro = request.Tamanho.Trim().ToLowerInvariant();
                query = query.Where(solicitacao => solicitacao.Tamanho != null && solicitacao.Tamanho.Valor.ToLower().Contains(tamanhoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Cor))
            {
                string corFiltro = request.Cor.Trim().ToLowerInvariant();
                query = query.Where(solicitacao => solicitacao.Cor != null && solicitacao.Cor.Valor.ToLower().Contains(corFiltro));
            }

            if (!string.IsNullOrWhiteSpace(request.Cliente))
            {
                string clienteFiltro = request.Cliente.Trim().ToLowerInvariant();
                query = query.Where(solicitacao => solicitacao.Cliente != null && solicitacao.Cliente.Nome.ToLower().Contains(clienteFiltro));
            }

            if (request.PrecoInicial.HasValue)
            {
                query = query.Where(solicitacao => solicitacao.PrecoMinimo >= request.PrecoInicial.Value);
            }

            if (request.PrecoFinal.HasValue)
            {
                query = query.Where(solicitacao => solicitacao.PrecoMaximo <= request.PrecoFinal.Value);
            }

            IQueryable<SolicitacaoBuscaDto> queryProjetada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveis, "descricao")
                .ThenBy(solicitacao => solicitacao.Id)
                .Select(solicitacao => new SolicitacaoBuscaDto
                {
                    Id = solicitacao.Id,
                    ProdutoId = solicitacao.ProdutoId,
                    Produto = solicitacao.Produto != null ? solicitacao.Produto.Valor : string.Empty,
                    MarcaId = solicitacao.MarcaId,
                    Marca = solicitacao.Marca != null ? solicitacao.Marca.Valor : string.Empty,
                    TamanhoId = solicitacao.TamanhoId,
                    Tamanho = solicitacao.Tamanho != null ? solicitacao.Tamanho.Valor : string.Empty,
                    CorId = solicitacao.CorId,
                    Cor = solicitacao.Cor != null ? solicitacao.Cor.Valor : string.Empty,
                    ClienteId = solicitacao.ClienteId,
                    Cliente = solicitacao.Cliente != null ? solicitacao.Cliente.Nome : string.Empty,
                    Descricao = solicitacao.Descricao ?? string.Empty,
                    PrecoMinimo = solicitacao.PrecoMinimo,
                    PrecoMaximo = solicitacao.PrecoMaximo,
                    LojaId = solicitacao.LojaId
                });

            PaginacaoDto<SolicitacaoBuscaDto> pagina = await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);

            foreach (SolicitacaoBuscaDto item in pagina.Itens)
            {
                item.ProdutosCompativeis = await ObterProdutosCompativeisAsync(item, cancellationToken);
            }

            return pagina;
        }

        public async Task DeleteAsync(ExcluirSolicitacaoParametros parametros, CancellationToken cancellationToken = default)
        {
            SolicitacaoModel? solicitacao = await _context.Solicitacoes
                .SingleOrDefaultAsync(item => item.Id == parametros.SolicitacaoId, cancellationToken);

            if (solicitacao is null)
            {
                throw new KeyNotFoundException("Solicitacao informada nao foi encontrada.");
            }

            await _authorizationService.EnsurePermissionAsync(
                solicitacao.LojaId,
                parametros.UsuarioId,
                FuncionalidadeCatalogo.SolicitacoesExcluir,
                cancellationToken);

            _ = _context.Solicitacoes.Remove(solicitacao);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        private Task<List<ProdutoCompativelDto>> ObterProdutosCompativeisAsync(SolicitacaoModel solicitacao, CancellationToken cancellationToken)
        {
            return ObterProdutosCompativeisAsync(
                solicitacao.ProdutoId,
                solicitacao.MarcaId,
                solicitacao.TamanhoId,
                solicitacao.CorId,
                solicitacao.LojaId,
                solicitacao.PrecoMinimo,
                solicitacao.PrecoMaximo,
                cancellationToken);
        }

        private Task<List<ProdutoCompativelDto>> ObterProdutosCompativeisAsync(SolicitacaoBuscaDto solicitacao, CancellationToken cancellationToken)
        {
            return ObterProdutosCompativeisAsync(
                solicitacao.ProdutoId,
                solicitacao.MarcaId,
                solicitacao.TamanhoId,
                solicitacao.CorId,
                solicitacao.LojaId,
                solicitacao.PrecoMinimo,
                solicitacao.PrecoMaximo,
                cancellationToken);
        }

        private Task<List<ProdutoCompativelDto>> ObterProdutosCompativeisAsync(
            int? produtoId,
            int? marcaId,
            int? tamanhoId,
            int? corId,
            int lojaId,
            decimal? precoMinimo,
            decimal? precoMaximo,
            CancellationToken cancellationToken)
        {
            return _context.ProdutosEstoque
                .Where(produto =>
                    produto.LojaId == lojaId
                    && (!produtoId.HasValue || produto.ProdutoId == produtoId.Value)
                    && (!marcaId.HasValue || produto.MarcaId == marcaId.Value)
                    && (!tamanhoId.HasValue || produto.TamanhoId == tamanhoId.Value)
                    && (!corId.HasValue || produto.CorId == corId.Value)
                    && (!precoMinimo.HasValue || produto.Preco >= precoMinimo.Value)
                    && (!precoMaximo.HasValue || produto.Preco <= precoMaximo.Value))
                .OrderBy(produto => produto.Descricao)
                .ThenBy(produto => produto.Id)
                .Select(produto => new ProdutoCompativelDto
                {
                    Id = produto.Id,
                    Etiqueta = produto.Etiqueta,
                    Produto = produto.Produto != null ? produto.Produto.Valor : string.Empty,
                    Marca = produto.Marca != null ? produto.Marca.Valor : string.Empty,
                    Tamanho = produto.Tamanho != null ? produto.Tamanho.Valor : string.Empty,
                    Cor = produto.Cor != null ? produto.Cor.Valor : string.Empty,
                    Fornecedor = produto.Fornecedor != null ? produto.Fornecedor.Nome : string.Empty,
                    Descricao = produto.Descricao,
                    Preco = produto.Preco
                })
                .ToListAsync(cancellationToken);
        }

    }
}
