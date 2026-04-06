using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model.Dto;

namespace Renova.Service.Extensions
{
    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> ApplyOrdering<T>(
            this IQueryable<T> source,
            string? ordenarPor,
            string? direcao,
            IReadOnlyDictionary<string, LambdaExpression> camposOrdenaveis,
            string campoPadrao)
        {
            string campoNormalizado = string.IsNullOrWhiteSpace(ordenarPor)
                ? campoPadrao
                : ordenarPor.Trim().ToLowerInvariant();

            if (!camposOrdenaveis.TryGetValue(campoNormalizado, out LambdaExpression? seletor))
            {
                throw new ArgumentException("Campo de ordenacao invalido.", nameof(ordenarPor));
            }

            string direcaoNormalizada = string.IsNullOrWhiteSpace(direcao)
                ? "asc"
                : direcao.Trim().ToLowerInvariant();

            if (direcaoNormalizada is not ("asc" or "desc"))
            {
                throw new ArgumentException("Direcao de ordenacao invalida.", nameof(direcao));
            }

            string metodo = direcaoNormalizada == "desc" ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

            MethodCallExpression chamada = Expression.Call(
                typeof(Queryable),
                metodo,
                [typeof(T), seletor.ReturnType],
                source.Expression,
                Expression.Quote(seletor));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(chamada);
        }

        public static async Task<PaginacaoDto<T>> ToPagedResultAsync<T>(
            this IQueryable<T> source,
            int pagina,
            int tamanhoPagina,
            CancellationToken cancellationToken = default)
        {
            int totalItens = await source.CountAsync(cancellationToken);
            int totalPaginas = totalItens == 0 ? 0 : (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

            List<T> itens = await source
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync(cancellationToken);

            return new PaginacaoDto<T>
            {
                Itens = itens,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = totalItens,
                TotalPaginas = totalPaginas
            };
        }
    }
}
