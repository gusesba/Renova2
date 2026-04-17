namespace Renova.Domain.Model.Dto
{
    public class CargoDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public int LojaId { get; set; }
        public IReadOnlyList<FuncionalidadeDto> Funcionalidades { get; set; } = [];
        public int QuantidadeFuncionarios { get; set; }
    }
}
