namespace Renova.Domain.Model
{
    public class CargoModel
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public ICollection<FuncionarioModel> Funcionarios { get; set; } = [];
        public ICollection<CargoFuncionalidadeModel> Funcionalidades { get; set; } = [];
    }
}
