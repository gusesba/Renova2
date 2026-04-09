namespace Renova.Domain.Model
{
    public class ClienteCreditoModel
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public LojaModel? Loja { get; set; }
        public int ClienteId { get; set; }
        public ClienteModel? Cliente { get; set; }
        public decimal Valor { get; set; }
    }
}
