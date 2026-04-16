using Microsoft.EntityFrameworkCore;

using Renova.Domain.Access;
using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cargo;
using Renova.Service.Services.Acesso;
using Renova.Service.Parameters.Cargo;

namespace Renova.Service.Services.Cargo
{
    public class CargoService(RenovaDbContext context, ILojaAuthorizationService authorizationService) : ICargoService
    {
        private readonly RenovaDbContext _context = context;
        private readonly ILojaAuthorizationService _authorizationService = authorizationService;

        public async Task<IReadOnlyList<CargoDto>> GetAllAsync(OperacaoCargoParametros parametros, CancellationToken cancellationToken = default)
        {
            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioId,
                FuncionalidadeCatalogo.CargosVisualizar,
                cancellationToken);

            return await _context.Cargos
                .AsNoTracking()
                .Include(cargo => cargo.Funcionalidades)
                .ThenInclude(item => item.Funcionalidade)
                .Include(cargo => cargo.Funcionarios)
                .Where(cargo => cargo.LojaId == parametros.LojaId)
                .OrderBy(cargo => cargo.Nome)
                .ThenBy(cargo => cargo.Id)
                .Select(Mapear())
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<FuncionalidadeDto>> GetFuncionalidadesAsync(OperacaoCargoParametros parametros, CancellationToken cancellationToken = default)
        {
            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioId,
                FuncionalidadeCatalogo.CargosVisualizar,
                cancellationToken);

            return await _context.Funcionalidades
                .AsNoTracking()
                .OrderBy(item => item.Grupo)
                .ThenBy(item => item.Chave)
                .Select(item => new FuncionalidadeDto
                {
                    Id = item.Id,
                    Chave = item.Chave,
                    Grupo = item.Grupo,
                    Descricao = item.Descricao
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<CargoDto> CreateAsync(CriarCargoCommand request, OperacaoCargoParametros parametros, CancellationToken cancellationToken = default)
        {
            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioId,
                FuncionalidadeCatalogo.CargosAdicionar,
                cancellationToken);

            string nomeNormalizado = request.Nome.Trim();
            await ValidarAsync(nomeNormalizado, request.FuncionalidadeIds, parametros.LojaId, null, cancellationToken);

            CargoModel cargo = new()
            {
                Nome = nomeNormalizado,
                LojaId = parametros.LojaId,
                Funcionalidades = [.. request.FuncionalidadeIds
                    .Distinct()
                    .Select(funcionalidadeId => new CargoFuncionalidadeModel
                    {
                        FuncionalidadeId = funcionalidadeId
                    })]
            };

            _ = await _context.Cargos.AddAsync(cargo, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return await ObterCargoDtoAsync(cargo.Id, cancellationToken);
        }

        public async Task<CargoDto> EditAsync(EditarCargoCommand request, OperacaoCargoParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!parametros.CargoId.HasValue)
            {
                throw new ArgumentException("CargoId e obrigatorio.", nameof(parametros));
            }

            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioId,
                FuncionalidadeCatalogo.CargosEditar,
                cancellationToken);

            CargoModel cargo = await _context.Cargos
                .Include(item => item.Funcionalidades)
                .SingleOrDefaultAsync(
                    item => item.Id == parametros.CargoId.Value && item.LojaId == parametros.LojaId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Cargo informado nao foi encontrado.");

            string nomeNormalizado = request.Nome.Trim();
            await ValidarAsync(nomeNormalizado, request.FuncionalidadeIds, parametros.LojaId, cargo.Id, cancellationToken);

            cargo.Nome = nomeNormalizado;
            cargo.Funcionalidades.Clear();

            foreach (int funcionalidadeId in request.FuncionalidadeIds.Distinct())
            {
                cargo.Funcionalidades.Add(new CargoFuncionalidadeModel
                {
                    CargoId = cargo.Id,
                    FuncionalidadeId = funcionalidadeId
                });
            }

            _ = await _context.SaveChangesAsync(cancellationToken);
            return await ObterCargoDtoAsync(cargo.Id, cancellationToken);
        }

        public async Task DeleteAsync(OperacaoCargoParametros parametros, CancellationToken cancellationToken = default)
        {
            if (!parametros.CargoId.HasValue)
            {
                throw new ArgumentException("CargoId e obrigatorio.", nameof(parametros));
            }

            await _authorizationService.EnsurePermissionAsync(
                parametros.LojaId,
                parametros.UsuarioId,
                FuncionalidadeCatalogo.CargosExcluir,
                cancellationToken);

            CargoModel cargo = await _context.Cargos
                .Include(item => item.Funcionarios)
                .SingleOrDefaultAsync(
                    item => item.Id == parametros.CargoId.Value && item.LojaId == parametros.LojaId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Cargo informado nao foi encontrado.");

            if (cargo.Funcionarios.Count > 0)
            {
                throw new InvalidOperationException("Nao e possivel excluir um cargo com funcionarios vinculados.");
            }

            _ = _context.Cargos.Remove(cargo);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task ValidarAsync(
            string nome,
            IReadOnlyCollection<int> funcionalidadeIds,
            int lojaId,
            int? cargoIdAtual,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("Nome do cargo e obrigatorio.");
            }

            bool nomeJaExiste = await _context.Cargos
                .AnyAsync(
                    item => item.LojaId == lojaId
                        && item.Id != cargoIdAtual
                        && item.Nome == nome,
                    cancellationToken);

            if (nomeJaExiste)
            {
                throw new InvalidOperationException("Ja existe um cargo com este nome para a loja.");
            }

            if (funcionalidadeIds.Count == 0)
            {
                return;
            }

            int quantidadeEncontrada = await _context.Funcionalidades
                .CountAsync(item => funcionalidadeIds.Distinct().Contains(item.Id), cancellationToken);

            if (quantidadeEncontrada != funcionalidadeIds.Distinct().Count())
            {
                throw new ArgumentException("Uma ou mais funcionalidades informadas nao foram encontradas.");
            }
        }

        private async Task<CargoDto> ObterCargoDtoAsync(int cargoId, CancellationToken cancellationToken)
        {
            return await _context.Cargos
                .AsNoTracking()
                .Include(cargo => cargo.Funcionalidades)
                .ThenInclude(item => item.Funcionalidade)
                .Include(cargo => cargo.Funcionarios)
                .Where(cargo => cargo.Id == cargoId)
                .Select(Mapear())
                .SingleAsync(cancellationToken);
        }

        private static System.Linq.Expressions.Expression<Func<CargoModel, CargoDto>> Mapear()
        {
            return cargo => new CargoDto
            {
                Id = cargo.Id,
                Nome = cargo.Nome,
                LojaId = cargo.LojaId,
                QuantidadeFuncionarios = cargo.Funcionarios.Count,
                Funcionalidades = cargo.Funcionalidades
                    .OrderBy(item => item.Funcionalidade!.Grupo)
                    .ThenBy(item => item.Funcionalidade!.Chave)
                    .Select(item => new FuncionalidadeDto
                    {
                        Id = item.FuncionalidadeId,
                        Chave = item.Funcionalidade!.Chave,
                        Grupo = item.Funcionalidade.Grupo,
                        Descricao = item.Funcionalidade.Descricao
                    })
                    .ToList()
            };
        }
    }
}
