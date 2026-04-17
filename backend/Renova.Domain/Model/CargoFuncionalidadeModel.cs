namespace Renova.Domain.Model
{
    public class CargoFuncionalidadeModel
    {
        public int CargoId { get; set; }
        public CargoModel? Cargo { get; set; }
        public int FuncionalidadeId { get; set; }
        public FuncionalidadeModel? Funcionalidade { get; set; }
    }
}
