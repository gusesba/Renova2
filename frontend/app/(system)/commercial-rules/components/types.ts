import type {
  CommercialDiscountBand,
  PaymentMethod,
  StoreCommercialRule,
  SupplierCommercialRule,
} from "@/lib/services/commercial-rules";

// Estado editavel de uma faixa da politica de desconto.
export type DiscountBandFormState = {
  id: string;
  diasMinimos: string;
  percentualDesconto: string;
};

// Estado editavel da regra comercial padrao da loja.
export type StoreRuleFormState = {
  id: string;
  percentualRepasseDinheiro: string;
  percentualRepasseCredito: string;
  permitePagamentoMisto: boolean;
  tempoMaximoExposicaoDias: string;
  politicaDesconto: DiscountBandFormState[];
  ativo: boolean;
};

// Estado editavel da regra comercial especifica do fornecedor.
export type SupplierRuleFormState = {
  id: string;
  pessoaLojaId: string;
  percentualRepasseDinheiro: string;
  percentualRepasseCredito: string;
  permitePagamentoMisto: boolean;
  tempoMaximoExposicaoDias: string;
  politicaDesconto: DiscountBandFormState[];
  ativo: boolean;
};

// Estado editavel do cadastro de meio de pagamento.
export type PaymentMethodFormState = {
  id: string;
  nome: string;
  tipoMeioPagamento: string;
  taxaPercentual: string;
  prazoRecebimentoDias: string;
  ativo: boolean;
};

type StoreRuleSeed = Pick<
  StoreRuleFormState,
  | "percentualRepasseDinheiro"
  | "percentualRepasseCredito"
  | "permitePagamentoMisto"
  | "tempoMaximoExposicaoDias"
  | "politicaDesconto"
  | "ativo"
>;

// Gera uma chave local estavel para linhas dinamicas no frontend.
function createLocalId() {
  return globalThis.crypto?.randomUUID?.() ?? Math.random().toString(36).slice(2);
}

// Cria uma faixa vazia para a politica de desconto.
export function createEmptyDiscountBand(): DiscountBandFormState {
  return {
    id: createLocalId(),
    diasMinimos: "",
    percentualDesconto: "",
  };
}

// Define o formulario inicial da regra padrao da loja.
export function emptyStoreRuleForm(): StoreRuleFormState {
  return {
    id: "",
    percentualRepasseDinheiro: "",
    percentualRepasseCredito: "",
    permitePagamentoMisto: false,
    tempoMaximoExposicaoDias: "",
    politicaDesconto: [],
    ativo: true,
  };
}

// Define o formulario inicial da regra de fornecedor.
export function emptySupplierRuleForm(pessoaLojaId = ""): SupplierRuleFormState {
  return {
    id: "",
    pessoaLojaId,
    percentualRepasseDinheiro: "",
    percentualRepasseCredito: "",
    permitePagamentoMisto: false,
    tempoMaximoExposicaoDias: "",
    politicaDesconto: [],
    ativo: true,
  };
}

// Replica a politica de desconto em uma nova instancia editavel do formulario.
export function cloneDiscountBands(
  bands: DiscountBandFormState[],
): DiscountBandFormState[] {
  return bands.map((band) => ({
    ...band,
    id: createLocalId(),
  }));
}

// Monta uma nova regra de fornecedor herdando os campos da regra padrao da loja.
export function createSupplierRuleFromStoreRule(
  storeRule: StoreRuleSeed | null | undefined,
  pessoaLojaId = "",
): SupplierRuleFormState {
  if (!storeRule) {
    return emptySupplierRuleForm(pessoaLojaId);
  }

  return {
    id: "",
    pessoaLojaId,
    percentualRepasseDinheiro: storeRule.percentualRepasseDinheiro,
    percentualRepasseCredito: storeRule.percentualRepasseCredito,
    permitePagamentoMisto: storeRule.permitePagamentoMisto,
    tempoMaximoExposicaoDias: storeRule.tempoMaximoExposicaoDias,
    politicaDesconto: cloneDiscountBands(storeRule.politicaDesconto),
    ativo: storeRule.ativo,
  };
}

// Define o formulario inicial de meio de pagamento.
export function emptyPaymentMethodForm(type = ""): PaymentMethodFormState {
  return {
    id: "",
    nome: "",
    tipoMeioPagamento: type,
    taxaPercentual: "0",
    prazoRecebimentoDias: "0",
    ativo: true,
  };
}

// Converte a lista de faixas da API para o formato editavel do frontend.
export function mapDiscountBandsToForm(
  bands: CommercialDiscountBand[],
): DiscountBandFormState[] {
  return bands.map((band) => ({
    id: createLocalId(),
    diasMinimos: String(band.diasMinimos),
    percentualDesconto: String(band.percentualDesconto),
  }));
}

// Converte a regra da loja da API para o formato editavel.
export function mapStoreRuleToForm(rule: StoreCommercialRule): StoreRuleFormState {
  return {
    id: rule.id,
    percentualRepasseDinheiro: String(rule.percentualRepasseDinheiro),
    percentualRepasseCredito: String(rule.percentualRepasseCredito),
    permitePagamentoMisto: rule.permitePagamentoMisto,
    tempoMaximoExposicaoDias: String(rule.tempoMaximoExposicaoDias),
    politicaDesconto: mapDiscountBandsToForm(rule.politicaDesconto),
    ativo: rule.ativo,
  };
}

// Converte a regra do fornecedor da API para o formato editavel.
export function mapSupplierRuleToForm(
  rule: SupplierCommercialRule,
): SupplierRuleFormState {
  return {
    id: rule.id,
    pessoaLojaId: rule.pessoaLojaId,
    percentualRepasseDinheiro: String(rule.percentualRepasseDinheiro),
    percentualRepasseCredito: String(rule.percentualRepasseCredito),
    permitePagamentoMisto: rule.permitePagamentoMisto,
    tempoMaximoExposicaoDias: String(rule.tempoMaximoExposicaoDias),
    politicaDesconto: mapDiscountBandsToForm(rule.politicaDesconto),
    ativo: rule.ativo,
  };
}

// Converte o meio de pagamento da API para o formato editavel.
export function mapPaymentMethodToForm(
  method: PaymentMethod,
): PaymentMethodFormState {
  return {
    id: method.id,
    nome: method.nome,
    tipoMeioPagamento: method.tipoMeioPagamento,
    taxaPercentual: String(method.taxaPercentual),
    prazoRecebimentoDias: String(method.prazoRecebimentoDias),
    ativo: method.ativo,
  };
}
