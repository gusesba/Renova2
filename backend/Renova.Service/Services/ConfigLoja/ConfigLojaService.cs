using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.ConfigLoja;
using Renova.Service.Parameters.ConfigLoja;

namespace Renova.Service.Services.ConfigLoja
{
    public class ConfigLojaService(RenovaDbContext context) : IConfigLojaService
    {
        private readonly RenovaDbContext _context = context;

        public async Task<ConfigLojaDto> GetAsync(int lojaId, ObterConfigLojaParametros parametros, CancellationToken cancellationToken = default)
        {
            _ = await ObterLojaDoUsuarioAsync(lojaId, parametros.UsuarioId, cancellationToken);

            ConfigLojaModel config = await _context.ConfiguracoesLoja
                .Include(item => item.DescontosPermanencia)
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

            ValidarDescontosPermanencia(descontosPermanencia);

            _ = await ObterLojaDoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            ConfigLojaModel? config = await _context.ConfiguracoesLoja
                .Include(item => item.DescontosPermanencia)
                .SingleOrDefaultAsync(item => item.LojaId == request.LojaId, cancellationToken);

            if (config is null)
            {
                config = new ConfigLojaModel
                {
                    LojaId = request.LojaId,
                    PercentualRepasseFornecedor = request.PercentualRepasseFornecedor,
                    PercentualRepasseVendedorCredito = request.PercentualRepasseVendedorCredito,
                    TempoPermanenciaProdutoMeses = request.TempoPermanenciaProdutoMeses,
                    DescontosPermanencia = MapearDescontosPermanencia(descontosPermanencia)
                };

                _ = await _context.ConfiguracoesLoja.AddAsync(config, cancellationToken);
            }
            else
            {
                config.PercentualRepasseFornecedor = request.PercentualRepasseFornecedor;
                config.PercentualRepasseVendedorCredito = request.PercentualRepasseVendedorCredito;
                config.TempoPermanenciaProdutoMeses = request.TempoPermanenciaProdutoMeses;
                config.DescontosPermanencia.Clear();

                foreach (ConfigLojaDescontoPermanenciaModel desconto in MapearDescontosPermanencia(descontosPermanencia))
                {
                    config.DescontosPermanencia.Add(desconto);
                }
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            ConfigLojaModel configPersistida = await _context.ConfiguracoesLoja
                .Include(item => item.DescontosPermanencia)
                .SingleAsync(item => item.Id == config.Id, cancellationToken);

            return Mapear(configPersistida);
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
    }
}
