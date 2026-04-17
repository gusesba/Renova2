namespace Renova.Domain.Model
{
    public class FuncionalidadeModel
    {
        public int Id { get; set; }
        public required string Chave { get; set; }
        public required string Grupo { get; set; }
        public required string Descricao { get; set; }
        public ICollection<CargoFuncionalidadeModel> Cargos { get; set; } = [];
    }
}
