namespace Renova.Domain.Model.Dto
{
    public class LiberacaoUsuarioStatusDto
    {
        public required string Status { get; set; }
        public DateTime? LiberadoAte { get; set; }
    }
}
