using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.GastoLoja;
using Renova.Service.Extensions;
using Renova.Service.Parameters.GastoLoja;
using Renova.Service.Queries.GastoLoja;

namespace Renova.Service.Services.GastoLoja
{
    public class GastoLojaService(RenovaDbContext context) : IGastoLojaService
    {
        private readonly RenovaDbContext _context = context;
        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<GastoLojaModel, int>>)(gasto => gasto.Id),
            ["data"] = (Expression<Func<GastoLojaModel, DateTime>>)(gasto => gasto.Data),
            ["natureza"] = (Expression<Func<GastoLojaModel, NaturezaGastoLoja>>)(gasto => gasto.Natureza),
            ["valor"] = (Expression<Func<GastoLojaModel, decimal>>)(gasto => gasto.Valor),
            ["descricao"] = (Expression<Func<GastoLojaModel, string>>)(gasto => gasto.Descricao ?? string.Empty)
        };

        public async Task<PaginacaoDto<GastoLojaBuscaDto>> GetAllAsync(
            ObterGastosLojaQuery request,
            OperacaoGastoLojaParametros parametros,
            CancellationToken cancellationToken = default)
        {
            if (!request.LojaId.HasValue)
            {
                throw new ArgumentException("LojaId e obrigatorio.", nameof(request));
            }

            _ = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId.Value, parametros.UsuarioId, cancellationToken);

            IQueryable<GastoLojaModel> query = _context.GastosLoja
                .Where(gasto => gasto.LojaId == request.LojaId.Value);

            if (request.DataInicial.HasValue)
            {
                DateTime dataInicialUtc = NormalizarDateTimeParaUtc(request.DataInicial.Value);
                query = query.Where(gasto => gasto.Data >= dataInicialUtc);
            }

            if (request.DataFinal.HasValue)
            {
                DateTime dataFinalUtc = NormalizarDateTimeParaUtc(request.DataFinal.Value);
                query = query.Where(gasto => gasto.Data <= dataFinalUtc);
            }

            if (request.Natureza.HasValue)
            {
                if (!Enum.IsDefined(request.Natureza.Value))
                {
                    throw new ArgumentException("Natureza do gasto da loja informada e invalida.", nameof(request));
                }

                query = query.Where(gasto => gasto.Natureza == request.Natureza.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Descricao))
            {
                string descricaoFiltro = request.Descricao.Trim().ToLowerInvariant();
                query = query.Where(gasto =>
                    gasto.Descricao != null && gasto.Descricao.ToLower().Contains(descricaoFiltro));
            }

            IQueryable<GastoLojaBuscaDto> queryProjetada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveis, "data")
                .ThenBy(gasto => gasto.Id)
                .Select(gasto => new GastoLojaBuscaDto
                {
                    Id = gasto.Id,
                    LojaId = gasto.LojaId,
                    Natureza = gasto.Natureza,
                    Valor = gasto.Valor,
                    Data = gasto.Data,
                    Descricao = gasto.Descricao
                });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        public async Task<GastoLojaDto> CreateAsync(
            CriarGastoLojaCommand request,
            OperacaoGastoLojaParametros parametros,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(request.Natureza))
            {
                throw new ArgumentException("Natureza do gasto da loja informada e invalida.", nameof(request));
            }

            if (request.Data == default)
            {
                throw new ArgumentException("Data do gasto da loja e obrigatoria.", nameof(request));
            }

            if (request.Valor <= 0)
            {
                throw new ArgumentException("Valor do gasto da loja deve ser maior que zero.", nameof(request));
            }

            if (request.Descricao?.Trim().Length > 500)
            {
                throw new ArgumentException("Descricao do gasto da loja deve ter no maximo 500 caracteres.", nameof(request));
            }

            _ = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            GastoLojaModel gasto = new()
            {
                LojaId = request.LojaId,
                Natureza = request.Natureza,
                Valor = decimal.Round(request.Valor, 2, MidpointRounding.AwayFromZero),
                Data = NormalizarDateTimeParaUtc(request.Data),
                Descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim()
            };

            _ = await _context.GastosLoja.AddAsync(gasto, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new GastoLojaDto
            {
                Id = gasto.Id,
                LojaId = gasto.LojaId,
                Natureza = gasto.Natureza,
                Valor = gasto.Valor,
                Data = gasto.Data,
                Descricao = gasto.Descricao
            };
        }

        private static DateTime NormalizarDateTimeParaUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => value
            };
        }
    }
}
