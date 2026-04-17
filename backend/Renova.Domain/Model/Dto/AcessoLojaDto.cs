namespace Renova.Domain.Model.Dto
{
    public class AcessoLojaDto
    {
        public int LojaId { get; set; }
        public bool EhDono { get; set; }
        public int? CargoId { get; set; }
        public string? CargoNome { get; set; }
        public IReadOnlyList<string> Funcionalidades { get; set; } = [];
    }
}
