namespace Renova.Domain.Model.Dto
{
    public class MovimentacaoDestinacaoSugestaoDto
    {
        public int LojaId { get; set; }
        public int TempoPermanenciaProdutoMeses { get; set; }
        public DateTime DataLimitePermanencia { get; set; }
        public IReadOnlyList<MovimentacaoDestinacaoProdutoDto> Produtos { get; set; } = [];
    }
}
