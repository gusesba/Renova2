using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Funcionario;
using Renova.Service.Parameters.Funcionario;

namespace Renova.Service.Services.Funcionario
{
    public interface IFuncionarioService
    {
        Task<FuncionarioDto> CreateAsync(
            CriarFuncionarioCommand request,
            CriarFuncionarioParametros parametros,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FuncionarioDto>> GetAllAsync(
            ObterFuncionariosParametros parametros,
            CancellationToken cancellationToken = default);
        Task DeleteAsync(
            ExcluirFuncionarioParametros parametros,
            CancellationToken cancellationToken = default);
    }
}
