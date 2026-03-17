// Reune o estado editavel do modulo para evitar tipos soltos entre os componentes da pagina.
export type PersonBankAccountFormState = {
  id: string;
  banco: string;
  agencia: string;
  conta: string;
  tipoConta: string;
  pixTipo: string;
  pixChave: string;
  favorecidoNome: string;
  favorecidoDocumento: string;
  principal: boolean;
};

export type PersonFormState = {
  id: string;
  tipoPessoa: "fisica" | "juridica";
  nome: string;
  nomeSocial: string;
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
  observacoes: string;
  ativo: boolean;
  perfilRelacionamento: "cliente" | "fornecedor" | "ambos";
  aceitaCreditoLoja: boolean;
  politicaPadraoFimConsignacao: "devolver" | "doar";
  observacoesInternas: string;
  statusRelacao: "ativo" | "inativo";
  usuarioId: string;
  contasBancarias: PersonBankAccountFormState[];
};

// Define o template inicial para uma nova conta bancaria no formulario.
export const emptyBankAccountForm = (): PersonBankAccountFormState => ({
  id: "",
  banco: "",
  agencia: "",
  conta: "",
  tipoConta: "corrente",
  pixTipo: "",
  pixChave: "",
  favorecidoNome: "",
  favorecidoDocumento: "",
  principal: false,
});

// Define o estado inicial para um novo cadastro de pessoa.
export const emptyPersonForm = (): PersonFormState => ({
  id: "",
  tipoPessoa: "fisica",
  nome: "",
  nomeSocial: "",
  documento: "",
  telefone: "",
  email: "",
  logradouro: "",
  numero: "",
  complemento: "",
  bairro: "",
  cidade: "",
  uf: "",
  cep: "",
  observacoes: "",
  ativo: true,
  perfilRelacionamento: "cliente",
  aceitaCreditoLoja: false,
  politicaPadraoFimConsignacao: "devolver",
  observacoesInternas: "",
  statusRelacao: "ativo",
  usuarioId: "",
  contasBancarias: [],
});
