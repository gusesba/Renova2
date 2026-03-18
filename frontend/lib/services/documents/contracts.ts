// Contratos consumidos pelo modulo 16 no frontend.
export type DocumentTypeCode =
  | "etiqueta"
  | "recibo_venda"
  | "comprovante_fornecedor"
  | "comprovante_consignacao";

export type DocumentTypeOption = {
  codigo: DocumentTypeCode;
  nome: string;
  descricao: string;
};

export type DocumentWorkspace = {
  lojaId: string;
  lojaNome: string;
  tiposDocumento: DocumentTypeOption[];
};

export type DocumentSearchItem = {
  id: string;
  titulo: string;
  subtitulo: string;
  meta: string;
};
