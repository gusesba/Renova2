using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.ConfigLoja;
using Renova.Service.Extensions;
using Renova.Service.Parameters.ConfigLoja;

namespace Renova.Service.Services.ConfigLoja
{
    public class ConfigLojaService(RenovaDbContext context) : IConfigLojaService
    {
        private readonly RenovaDbContext _context = context;

        public async Task<ConfigLojaDto> GetAsync(int lojaId, ObterConfigLojaParametros parametros, CancellationToken cancellationToken = default)
        {
            _ = await _context.ObterLojaAcessivelAoUsuarioAsync(lojaId, parametros.UsuarioId, cancellationToken);

            ConfigLojaModel config = await _context.ConfiguracoesLoja
                .Include(item => item.DescontosPermanencia)
                .Include(item => item.FormasPagamento)
                .SingleOrDefaultAsync(item => item.LojaId == lojaId, cancellationToken)
                ?? throw new KeyNotFoundException("Configuracao da loja nao encontrada.");

            return Mapear(config);
        }

        public async Task<ConfigLojaDto> SaveAsync(SalvarConfigLojaCommand request, SalvarConfigLojaParametros parametros, CancellationToken cancellationToken = default)
        {
            if (request.PercentualRepasseFornecedor < 0 || request.PercentualRepasseFornecedor > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Percentual de repasse ao fornecedor deve estar entre 0 e 100.");
            }

            if (request.PercentualRepasseVendedorCredito < 0 || request.PercentualRepasseVendedorCredito > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Percentual de repasse ao vendedor em credito deve estar entre 0 e 100.");
            }

            if (request.PercentualRepasseVendedorCredito < request.PercentualRepasseFornecedor)
            {
                throw new ArgumentException("Percentual de repasse ao vendedor em credito deve ser maior ou igual ao repasse normal.");
            }

            if (request.TempoPermanenciaProdutoMeses < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Tempo de permanencia do produto na loja deve ser de ao menos 1 mes.");
            }

            List<SalvarConfigLojaDescontoPermanenciaCommand> descontosPermanencia = request.DescontosPermanencia ?? [];
            List<SalvarConfigLojaFormaPagamentoCommand> formasPagamento = request.FormasPagamento ?? [];

            ValidarDescontosPermanencia(descontosPermanencia);
            ValidarFormasPagamento(formasPagamento);

            _ = await _context.ObterLojaAcessivelAoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            ConfigLojaModel? config = await _context.ConfiguracoesLoja
                .Include(item => item.DescontosPermanencia)
                .Include(item => item.FormasPagamento)
                .SingleOrDefaultAsync(item => item.LojaId == request.LojaId, cancellationToken);

            if (config is null)
            {
                config = new ConfigLojaModel
                {
                    LojaId = request.LojaId,
                    PercentualRepasseFornecedor = request.PercentualRepasseFornecedor,
                    PercentualRepasseVendedorCredito = request.PercentualRepasseVendedorCredito,
                    TempoPermanenciaProdutoMeses = request.TempoPermanenciaProdutoMeses,
                    DescontosPermanencia = MapearDescontosPermanencia(descontosPermanencia),
                    FormasPagamento = MapearFormasPagamento(formasPagamento)
                };

                _ = await _context.ConfiguracoesLoja.AddAsync(config, cancellationToken);
            }
            else
            {
                config.PercentualRepasseFornecedor = request.PercentualRepasseFornecedor;
                config.PercentualRepasseVendedorCredito = request.PercentualRepasseVendedorCredito;
                config.TempoPermanenciaProdutoMeses = request.TempoPermanenciaProdutoMeses;
                config.DescontosPermanencia.Clear();
                config.FormasPagamento.Clear();

                foreach (ConfigLojaDescontoPermanenciaModel desconto in MapearDescontosPermanencia(descontosPermanencia))
                {
                    config.DescontosPermanencia.Add(desconto);
                }

                foreach (ConfigLojaFormaPagamentoModel formaPagamento in MapearFormasPagamento(formasPagamento))
                {
                    config.FormasPagamento.Add(formaPagamento);
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            ConfigLojaModel configPersistida = await _context.ConfiguracoesLoja
                .Include(item => item.DescontosPermanencia)
                .Include(item => item.FormasPagamento)
                .SingleAsync(item => item.Id == config.Id, cancellationToken);

            return Mapear(configPersistida);
        }

        private static ConfigLojaDto Mapear(ConfigLojaModel config)
        {
            return new ConfigLojaDto
            {
                LojaId = config.LojaId,
                PercentualRepasseFornecedor = config.PercentualRepasseFornecedor,
                PercentualRepasseVendedorCredito = config.PercentualRepasseVendedorCredito,
                TempoPermanenciaProdutoMeses = config.TempoPermanenciaProdutoMeses,
                DescontosPermanencia = [.. config.DescontosPermanencia
                    .OrderBy(item => item.APartirDeMeses)
                    .Select(item => new ConfigLojaDescontoPermanenciaDto
                    {
                        APartirDeMeses = item.APartirDeMeses,
                        PercentualDesconto = item.PercentualDesconto
                    })],
                FormasPagamento = [.. config.FormasPagamento
                    .OrderBy(item => item.Nome)
                    .Select(item => new ConfigLojaFormaPagamentoDto
                    {
                        Id = item.Id,
                        Nome = item.Nome,
                        PercentualAjuste = item.PercentualAjuste
                    })]
            };
        }

        private static void ValidarDescontosPermanencia(List<SalvarConfigLojaDescontoPermanenciaCommand> descontosPermanencia)
        {
            if (descontosPermanencia.Count == 0)
            {
                return;
            }

            if (descontosPermanencia.Any(item => item.APartirDeMeses < 1))
            {
                throw new ArgumentOutOfRangeException(nameof(descontosPermanencia), "Meses para desconto por permanencia devem ser maiores que zero.");
            }

            if (descontosPermanencia.Any(item => item.PercentualDesconto < 0 || item.PercentualDesconto > 100))
            {
                throw new ArgumentOutOfRangeException(nameof(descontosPermanencia), "Percentual de desconto por permanencia deve estar entre 0 e 100.");
            }

            if (descontosPermanencia
                .GroupBy(item => item.APartirDeMeses)
                .Any(group => group.Count() > 1))
            {
                throw new ArgumentException("Nao e permitido informar mais de um desconto para a mesma quantidade de meses.", nameof(descontosPermanencia));
            }
        }

        private static void ValidarFormasPagamento(List<SalvarConfigLojaFormaPagamentoCommand> formasPagamento)
        {
            if (formasPagamento.Count == 0)
            {
                return;
            }

            if (formasPagamento.Any(item => string.IsNullOrWhiteSpace(item.Nome)))
            {
                throw new ArgumentException("Nome da forma de pagamento e obrigatorio.", nameof(formasPagamento));
            }

            if (formasPagamento.Any(item => item.Nome.Trim().Length > 100))
            {
                throw new ArgumentException("Nome da forma de pagamento deve ter no maximo 100 caracteres.", nameof(formasPagamento));
            }

            if (formasPagamento.Any(item => item.PercentualAjuste < -100 || item.PercentualAjuste > 100))
            {
                throw new ArgumentOutOfRangeException(nameof(formasPagamento), "Percentual da forma de pagamento deve estar entre -100 e 100.");
            }

            if (formasPagamento
                .GroupBy(item => item.Nome.Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() > 1))
            {
                throw new ArgumentException("Nao e permitido informar formas de pagamento com o mesmo nome.", nameof(formasPagamento));
            }
        }

        private static List<ConfigLojaDescontoPermanenciaModel> MapearDescontosPermanencia(List<SalvarConfigLojaDescontoPermanenciaCommand> descontosPermanencia)
        {
            return [.. descontosPermanencia
                .OrderBy(item => item.APartirDeMeses)
                .Select(item => new ConfigLojaDescontoPermanenciaModel
                {
                    APartirDeMeses = item.APartirDeMeses,
                    PercentualDesconto = item.PercentualDesconto
                })];
        }

        private static List<ConfigLojaFormaPagamentoModel> MapearFormasPagamento(List<SalvarConfigLojaFormaPagamentoCommand> formasPagamento)
        {
            return [.. formasPagamento
                .OrderBy(item => item.Nome.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(item => new ConfigLojaFormaPagamentoModel
                {
                    Nome = item.Nome.Trim(),
                    PercentualAjuste = item.PercentualAjuste
                })];
        }
    }
}
