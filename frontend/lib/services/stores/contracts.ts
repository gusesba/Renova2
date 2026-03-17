// Agrupa os contratos usados pelo modulo de lojas no frontend.
export type StoreSummary = {
  id: string;
  nomeFantasia: string;
  razaoSocial: string;
  documento: string;
  telefone: string;
  email: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
  statusLoja: string;
  ativo: boolean;
  conjuntoCatalogoId: string;
  ehLojaAtiva: boolean;
  ehResponsavel: boolean;
  podeGerenciar: boolean;
};
