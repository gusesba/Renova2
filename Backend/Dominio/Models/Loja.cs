namespace Renova.Domain.Models;

public class Loja : AtivavelEntityBase
{
    public string NomeFantasia { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string StatusLoja { get; set; } = string.Empty;
}
