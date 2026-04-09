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

            _ = await ObterLojaDoUsuarioAsync(request.LojaId, parametros.UsuarioId, cancellationToken);

            ConfigLojaModel? config = await _context.ConfiguracoesLoja
                .SingleOrDefaultAsync(item => item.LojaId == request.LojaId, cancellationToken);

            if (config is null)
            {
                config = new ConfigLojaModel
                {
                    LojaId = request.LojaId,
                    PercentualRepasseFornecedor = request.PercentualRepasseFornecedor,
                    PercentualRepasseVendedorCredito = request.PercentualRepasseVendedorCredito
                };

                _ = await _context.ConfiguracoesLoja.AddAsync(config, cancellationToken);
            }
            else
            {
                config.PercentualRepasseFornecedor = request.PercentualRepasseFornecedor;
                config.PercentualRepasseVendedorCredito = request.PercentualRepasseVendedorCredito;
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return Mapear(config);
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
                PercentualRepasseVendedorCredito = config.PercentualRepasseVendedorCredito
            };
        }
    }
}
