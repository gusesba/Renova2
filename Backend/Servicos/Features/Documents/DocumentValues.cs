namespace Renova.Services.Features.Documents;

// Centraliza os tipos e regras simples do modulo 16.
public static class DocumentValues
{
    public static class DocumentTypes
    {
        public const string Etiqueta = "etiqueta";
        public const string ReciboVenda = "recibo_venda";
        public const string ComprovanteFornecedor = "comprovante_fornecedor";
        public const string ComprovanteConsignacao = "comprovante_consignacao";
    }

    /// <summary>
    /// Monta a lista fixa de documentos imprimiveis do sistema.
    /// </summary>
    public static IReadOnlyList<DocumentTypeOption> BuildDocumentTypes()
    {
        return
        [
            new(DocumentTypes.Etiqueta, "Etiqueta da peça", "Modelo único com código de barras e dados da peça."),
            new(DocumentTypes.ReciboVenda, "Recibo de venda", "Modelo único com dados da loja, itens e pagamentos."),
            new(DocumentTypes.ComprovanteFornecedor, "Pagamento ao fornecedor", "Modelo único com dados da liquidação."),
            new(DocumentTypes.ComprovanteConsignacao, "Devolução ou doação", "Comprovante do encerramento da consignação."),
        ];
    }
}

// Representa uma opcao fixa do modulo.
public sealed record DocumentTypeOption(string Codigo, string Nome, string Descricao);
